;********************************************************************************
;*		Projekt z Jêzyków Asemblerowych 
;*		Autor: Mateusz Szostok
;*
;*		Grupa: 4
;*		Sekcja: 1
;*
;*		Algortym realizuj¹cy wa¿one nak³adanie dwóch obrazów.
;*
;*		Wersja: 1.0
;********************************************************************************

.586	; Wykonywane bêd¹ instrukcje procesora Intel Pentium
		; w trybie adresacji rzeczywistej.
.XMM	; Umo¿liwienie korzystania z instrukcji strumieniowych (SIMD).
.MODEL FLAT, STDCALL ; mModel pamiêci dla platformy 32-bitowej 
					 ; z konwencj¹ nazewnictwa i wywo³wania funkcji stdcall.

OPTION CASEMAP:NONE ; wy³¹czenie uwzglêdniania wielkoœci liter

; Zmiejszenie wagi pliku symboli i zwiêkszenie jego przejrzystoœci poprzez 
; pominiêcie deklaracji i definicji do³¹czonych plików bibliotecznych, 
; oraz wy³¹czenie listingu.
.NOLIST ; wy³¹czenie generowania listingu

INCLUDE   c:\masm32\include\windows.inc 

.LIST ; w³¹czenie generowania listy symboli

;********************************************************************************
;    Segment deklaracji zmiennych
;******************************************************************************** 
.data

Pixel		STRUCT 4 ; definicja struktury przechowuj¹cej definicjê kolorów danego 
					 ; piksela z offsetem ka¿dego elementu podzielnym przez 4 
					 ; (wiêszka zajêtoœæ pamiêci ale szybsze dzia³anie)
  b			DD	?	 ; 4 bajtowa zmienna oznaczaj¹ca kolor niebieski (blue)
  g			DD  ?	 ; 4 bajtowa zmienna oznaczaj¹ca kolor zielony (greeen)
  r			DD  ?	 ; 4 bajtowa zmienna oznaczaj¹ca kolor czerwony (red)
  a			DD  ?	 ; 4 bajtowa zmienna oznaczaj¹ca kolor niebieski (blue)

Pixel		ENDS	 ; koniec definicji strukury Pixel

;********************************************************************************
;    Segment deklaracji sta³ych
;********************************************************************************
.const 

maxAlpha DD 255	            ; sta³a oznaczaj¹ca maksymaln¹ wartoœæ przeŸroczystoœci
							; jak¹ mo¿e wprowadziæ u¿ytkownik dla obrazu nak³adanego
numberOfIntegerInLoop DD 4	; liczba edytowanych zmiennych w pêtli g³ownej blendLoop
bytesPerPixel DD 4			; liczba bajtów z jakiej sk³ada siê pojedyñczy piksel

;********************************************************************************
;    Segment kodu
;********************************************************************************
.code

;********************************************************************************
;*
;*		Funkcja sprawdzaj¹ca dostêpnoœæ instrukcji SSE2.
;*		
;*		Zwraca informacje poprzez akumlator:
;*		   -1 - rozkaz cpuid nie jest wspierany 
;*			0 - instrukcje SSE2 s¹ niedostêpne
;*			1 - zestaw instrukcji SSE2 jest dostêpny
;*			
;*		Autor: Mateusz Szostok
;*
;*		Wersja: 1.0
;********************************************************************************

checkSSE2 PROC NEAR USES edx ; nie dodaje EAX poniewa¿ przez niego zostanie przekazana informacja
	xor eax, eax	; ustawienie wartoœci zero w eax
	cpuid			; wywo³anie instrucji w celu sprawdzenia czy jest wspierana -
					; jeœli w EAX bêdzie zero, oznacza to, ¿e nasz CPU nie wspiera tej instrukcji,
					; w przeciwnym wypadku umieœci w rejestrze najwy¿szy parametr jaki mo¿e byæ 
					; umieszczony podczas wywo³ywania cpuid 
	test eax, eax	; ustawienie flagi ZR, tylko wtedy kiedy jest zero w eax
	jz NoSupport	; jeœli ZR = 1 nale¿y pomin¹c dalsze instrukcje (instrukcja cpuid nie wspierana)
	mov eax, 1		; umieszczenie parametru dla wywo³ywanej funkcji
					; cpuid z parametrem 1 powoduje za³adowanie wartoœci flag wspieranych technologi
					; do rejestru EBX
	cpuid			; ponowne wywo³anie w celu uzyskania wartoœci flag
	test edx, 2000000H ; sprawdzenie waroœci na 26 bicie poprzez maskowanie bitów 
	jnz  SSE2Available ; jeœli jest 1 na 26 bicie oznacza ¿e posiadamy zestaw instrukcji SSE2
	mov eax, 0		; jeœli nie zosta³ wykonany skok to znaczy, ¿e instrukcje nie s¹ 
	ret				; powrót z procedury

NoSupport:
	mov eax, -1		; rozkaz cpuid nie jest wspierany
	ret				; powrót z procedury

SSE2Available:
	mov eax, 1		; rozkazy s¹ dostêpne
	ret				; powrót z procedury

checkSSE2 ENDP 


;********************************************************************************
;*
;*		Procedura dokonuj¹ca wa¿onego nak³adania obrazów. Wynik na³o¿enia 
;*		obrazów bêdzie umieszczony w tablicy przekazanej jako obraz na
;*		który nale¿y na³o¿yæ drugi podany obraz.
;*		
;*		Wejœcie: 
;*			-bitmaps - dwuwymiarowa tablica typu int** gdzie, 
;*							bitmapList[0] - obraz bazowy
;*							bitmapList[1] - obraz nak³adany
;*													 
;*			-coords	- dwuwymarowa tablica typu int* gdzie,
;*							coords[0] - indeks pocz¹tkowy od którego wykonywane
;*										bêd¹ obliczenia na tablicy bitmap.
;*							coords[1] - indeks na którym nale¿y zakoñczyæ obliczenia
;*										na tablicy bitmap. 
;*			-alpha - waga nak³adanego obrazu (przeŸroczystoœæ) w zakresie od 0 do 255. 										
;*
;*		Wyjœcie:
;*		   -2 - zosta³y wykryte przek³amania w procedurze
;*		   -1 - rozkaz cpuid nie jest wspierany 
;*			0 - instrukcje SSE2 s¹ niedostêpne
;*			1 - procedura na³o¿enia obrazów przebieg³a pomyœlnie
;*
;*		Autor: Mateusz Szostok
;*
;*		Wersja: 1.0
;********************************************************************************

blendTwoImages PROC USES ebx edx ecx,		; te rejestry bêd¹ wykorzystywane w procedurze (nast¹pi ich automatyczne od³o¿enie i przywrócenie)
					bitmaps: PTR PTR DWORD,	; obrazy wejœciowe
					coords: PTR DWORD,		; indeksy pocz¹tkowe i koñcowe
					alpha: DWORD			; wartoœæ przeŸroczystoœci obrazu nak³adanego
	;============================== ZMIENNE LOKALNE =============================
	LOCAL	alphaBottom: DWORD, alphaTop: DWORD, ; wartoœci przeŸroczystoœci kolejno dla obrazu bazowego oraz nak³adanego
			start: DWORD, stop: DWORD,			 ; kolejno indeks pocz¹tkowy i koñcowy dla tablic pikseli
			rgba: Pixel							 ; struktura przechowuj¹ca pojedyñczy piksel

	;============================= OPERACJE WSTÊPNE =============================
	call checkSSE2	; sprawdzenie dostêpnoœci instrukcji SSE2, informacja zwrócona do EAX

	cmp eax, -1						; sprawdzenie czy procedura zwróci³a wartoœæ -1 
	je endProcWithCPUIDErr			; jeœli tak to nastêpuje zakoñczenie procedury
									; z powodu braku wspierania instrukcji CPUID
	cmp eax, 0						; sprawdzenie czy procedura zwróci³a wartoœæ 0 
	je endProcWithSSE2AvailableErr	; jeœli tak to nastêpuje zakoñczenie procedury
									; z powodu braku instrukcji SSE2
	cmp eax, 1						; sprawdzenie czy procedura zwróci³a wartosc 1
	jne	untrustedCheckSSE2Proc		; jeœli nie to nale¿y przerwaæ dalsze obliczenia, poniewa¿
									; funkcja CheckSSE2 nie zwróci³a ¿adnej z oczekwianych wartoœci 
									; wiêc nie dzia³a poprawnie, a wiêc nie mo¿emy jej ufaæ


	;================== USTAWIENIE ODPOWIEDNICH WAG DLA OBRAZÓW ==================
	mov eax, alpha					; wczytanie wagi(alpha) dla obrazu nak³adanego
	mov alphaTop, eax				; zainicjalizowanie zmiennej wag¹ obrazu nak³adanego
	mov eax, maxAlpha				; za³adowanie maksymalnej dozwolonej wagi 
	sub eax, alphaTop				; ró¿nica, która jest wartoœci¹ wagi(alpha) dla drugiego obrazu
	mov alphaBottom, eax			; za³adowanie wartoœci wagi drugiego obrazu
	
	MOVD	xmm5, alphaTop			; przepisanie wartoœci wagi obrazu nak³adanego do xmm5
	SHUFPS xmm5, xmm5, 0h			; powielenie jej na wszystkie 4 pola
	CVTDQ2PS xmm5, xmm5				; konwersja z double word na single-precision float

	MOVD	xmm6, alphaBottom		; przepisanie wartoœci drugiej wagi do xmm5
	SHUFPS xmm6, xmm6, 0h			; powielenie jej na wszystkie 4 pola
	CVTDQ2PS xmm6, xmm6				; konwersja z double word na single-precision float
									; poniewa¿ nie ma rozkazu dzielenia dla double word w SIMD

	MOVD	xmm7, maxAlpha			; przepisanie wartoœci wagi maksymalnej
	SHUFPS xmm7, xmm7, 0h			; powielenie jej na wszystkie 4 pola
	CVTDQ2PS xmm7, xmm7				; konwersja z double word na single-precision float

	DIVPS xmm5, xmm7				; zmiana zakresu z 0-255 na 0-1
	DIVPS xmm6, xmm7				; zmiana zakresu jak wy¿ej

	;=================== ODCZYTANIE ZAKRESU MODYFIKACJI PIKSELI ==================
	mov ebx, [coords]				; zapisanie wskaŸnika na tablicê
	mov ecx, bytesPerPixel			; zna³adowanie mno¿nika

	mov eax, [ebx + 0*SIZEOF DWORD] ; pobranie pierwszej komórki
	mul ecx							; coords[0] mówi od którego piksela zacz¹æ obliczenia
									; nale¿y pomno¿yæ t¹ wartoœæ poprzez liczbê bajtów na piksel
									; aby uzykaæ odpowiednie przemieszczenie w 
	mov start, eax					; zainicjalizowanie zmiennej poprawn¹ wartoœci¹

	mov eax, [ebx + 1*SIZEOF DWORD] ; pobranie drugiej komórki
	mul ecx							; zasada ta sama jw.
	mov stop, eax					; zainicjalizowanie zmiennej poprawn¹ wartoœci¹

	;========================= ODCZYTANIE TABLIC OBRAZÓW =========================
	mov eax, [bitmaps]				; zapisanie wskaŸnika na wiersze 
	mov ebx, [eax + 0*SIZEOF DWORD]	; zapisanie wskaŸnika na pierwszy wiersz (obraz bazowy)
	mov edx, [eax + 1*SIZEOF DWORD]	; zapisanie wskaŸnika na drugi wiersz (obraz nak³adany)


	;========================= WA¯ONE NAK£ADANIE OBRAZÓW =========================
	mov ecx, start					; zainicjalizowanie licznika pêtli
blendLoop:
	MOVDQU xmm1, [ebx + ecx*SIZEOF DWORD]	; za³adowanie 4 wartoœci (Move unaligned double quad words)
	MOVDQU xmm2, [edx + ecx*SIZEOF DWORD]	; za³adowanie 4 wartoœci (Move unaligned double quad words)
	
	CVTDQ2PS xmm1, xmm1				; konwersja z double word na single-precision float
	CVTDQ2PS xmm2, xmm2				; konwersja z double word na single-precision float

	MULPS xmm1, xmm6				; mno¿enie równoleg³e alphaBottom*bitmapBottom(x,y)
	MULPS xmm2, xmm5				; mno¿enie równoleg³e alphaTop*bitmapTop(x,y)
	
	ADDPS xmm1, xmm2				; dodanie wartoœci rgb o odpowiednich wagach

	CVTTPS2DQ xmm1, xmm1			; konwersja z single-precision na double words z obciêciem
	CVTTPS2DQ xmm2, xmm2			; konwersja z single-precision na double words z obciêciem
	
	MOVDQU [rgba], xmm1				; pobranie wartoœci do struktury w celu wy³uskania parametru 'a'
	mov [rgba.Pixel].a, 255			; przywrócenie domyœlnej wartoœci parametru 'a'
	MOVDQU xmm1, [rgba]				; za³adowanie poprawnych wartoœci do rejestru

	MOVDQU [ebx + ecx*SIZEOF DWORD], xmm1	    ; nadpisanie wartoœci w komórkach bitmapy bazowej

	add ecx, bytesPerPixel			; zwiêkszenie licznika o jeden piksel który zosta³ ju¿ obliczony
	cmp ecx, stop					; sprawdzenie czy nale¿y ju¿ skoñczyæ
jne blendLoop						; jeœli ecx != stop to wróæ na pocz¹tek pêtli, w przeciwnym wypadku
									; zakoñcz obliczenia
done:
	mov eax, 1						; zapisanie informacji wyjœciowej ( wszystko zosta³o wykonane poprawnie)
	ret								; powrót z procedury
	
endProcWithSSE2AvailableErr:		; zakoñczenie procedury z b³edem który jest ju¿ za³adowany w akumlatorze
endProcWithCPUIDErr:
	ret								; powrót z procedury 
untrustedCheckSSE2Proc:
	mov eax, -2						; za³adowanie kodu b³edu 
	ret								; oraz powrót z procedury
blendTwoImages ENDP
END									; koniec pliku