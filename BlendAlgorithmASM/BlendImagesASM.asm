;********************************************************************************
;*		Projekt z J�zyk�w Asemblerowych 
;*		Autor: Mateusz Szostok
;*
;*		Grupa: 4
;*		Sekcja: 1
;*
;*		Algortym realizuj�cy wa�one nak�adanie dw�ch obraz�w.
;*
;*		Wersja: 1.0
;********************************************************************************

.586	; Wykonywane b�d� instrukcje procesora Intel Pentium
		; w trybie adresacji rzeczywistej.
.XMM	; Umo�liwienie korzystania z instrukcji strumieniowych (SIMD).
.MODEL FLAT, STDCALL ; mModel pami�ci dla platformy 32-bitowej 
					 ; z konwencj� nazewnictwa i wywo�wania funkcji stdcall.

OPTION CASEMAP:NONE ; wy��czenie uwzgl�dniania wielko�ci liter

; Zmiejszenie wagi pliku symboli i zwi�kszenie jego przejrzysto�ci poprzez 
; pomini�cie deklaracji i definicji do��czonych plik�w bibliotecznych, 
; oraz wy��czenie listingu.
.NOLIST ; wy��czenie generowania listingu

INCLUDE   c:\masm32\include\windows.inc 

.LIST ; w��czenie generowania listy symboli

;********************************************************************************
;    Segment deklaracji zmiennych
;******************************************************************************** 
.data

Pixel		STRUCT 4 ; definicja struktury przechowuj�cej definicj� kolor�w danego 
					 ; piksela z offsetem ka�dego elementu podzielnym przez 4 
					 ; (wi�szka zaj�to�� pami�ci ale szybsze dzia�anie)
  b			DD	?	 ; 4 bajtowa zmienna oznaczaj�ca kolor niebieski (blue)
  g			DD  ?	 ; 4 bajtowa zmienna oznaczaj�ca kolor zielony (greeen)
  r			DD  ?	 ; 4 bajtowa zmienna oznaczaj�ca kolor czerwony (red)
  a			DD  ?	 ; 4 bajtowa zmienna oznaczaj�ca kolor niebieski (blue)

Pixel		ENDS	 ; koniec definicji strukury Pixel

;********************************************************************************
;    Segment deklaracji sta�ych
;********************************************************************************
.const 

maxAlpha DD 255	            ; sta�a oznaczaj�ca maksymaln� warto�� prze�roczysto�ci
							; jak� mo�e wprowadzi� u�ytkownik dla obrazu nak�adanego
numberOfIntegerInLoop DD 4	; liczba edytowanych zmiennych w p�tli g�ownej blendLoop
bytesPerPixel DD 4			; liczba bajt�w z jakiej sk�ada si� pojedy�czy piksel

;********************************************************************************
;    Segment kodu
;********************************************************************************
.code

;********************************************************************************
;*
;*		Funkcja sprawdzaj�ca dost�pno�� instrukcji SSE2.
;*		
;*		Zwraca informacje poprzez akumlator:
;*		   -1 - rozkaz cpuid nie jest wspierany 
;*			0 - instrukcje SSE2 s� niedost�pne
;*			1 - zestaw instrukcji SSE2 jest dost�pny
;*			
;*		Autor: Mateusz Szostok
;*
;*		Wersja: 1.0
;********************************************************************************

checkSSE2 PROC NEAR USES edx ; nie dodaje EAX poniewa� przez niego zostanie przekazana informacja
	xor eax, eax	; ustawienie warto�ci zero w eax
	cpuid			; wywo�anie instrucji w celu sprawdzenia czy jest wspierana -
					; je�li w EAX b�dzie zero, oznacza to, �e nasz CPU nie wspiera tej instrukcji,
					; w przeciwnym wypadku umie�ci w rejestrze najwy�szy parametr jaki mo�e by� 
					; umieszczony podczas wywo�ywania cpuid 
	test eax, eax	; ustawienie flagi ZR, tylko wtedy kiedy jest zero w eax
	jz NoSupport	; je�li ZR = 1 nale�y pomin�c dalsze instrukcje (instrukcja cpuid nie wspierana)
	mov eax, 1		; umieszczenie parametru dla wywo�ywanej funkcji
					; cpuid z parametrem 1 powoduje za�adowanie warto�ci flag wspieranych technologi
					; do rejestru EBX
	cpuid			; ponowne wywo�anie w celu uzyskania warto�ci flag
	test edx, 2000000H ; sprawdzenie waro�ci na 26 bicie poprzez maskowanie bit�w 
	jnz  SSE2Available ; je�li jest 1 na 26 bicie oznacza �e posiadamy zestaw instrukcji SSE2
	mov eax, 0		; je�li nie zosta� wykonany skok to znaczy, �e instrukcje nie s� 
	ret				; powr�t z procedury

NoSupport:
	mov eax, -1		; rozkaz cpuid nie jest wspierany
	ret				; powr�t z procedury

SSE2Available:
	mov eax, 1		; rozkazy s� dost�pne
	ret				; powr�t z procedury

checkSSE2 ENDP 


;********************************************************************************
;*
;*		Procedura dokonuj�ca wa�onego nak�adania obraz�w. Wynik na�o�enia 
;*		obraz�w b�dzie umieszczony w tablicy przekazanej jako obraz na
;*		kt�ry nale�y na�o�y� drugi podany obraz.
;*		
;*		Wej�cie: 
;*			-bitmaps - dwuwymiarowa tablica typu int** gdzie, 
;*							bitmapList[0] - obraz bazowy
;*							bitmapList[1] - obraz nak�adany
;*													 
;*			-coords	- dwuwymarowa tablica typu int* gdzie,
;*							coords[0] - indeks pocz�tkowy od kt�rego wykonywane
;*										b�d� obliczenia na tablicy bitmap.
;*							coords[1] - indeks na kt�rym nale�y zako�czy� obliczenia
;*										na tablicy bitmap. 
;*			-alpha - waga nak�adanego obrazu (prze�roczysto��) w zakresie od 0 do 255. 										
;*
;*		Wyj�cie:
;*		   -2 - zosta�y wykryte przek�amania w procedurze
;*		   -1 - rozkaz cpuid nie jest wspierany 
;*			0 - instrukcje SSE2 s� niedost�pne
;*			1 - procedura na�o�enia obraz�w przebieg�a pomy�lnie
;*
;*		Autor: Mateusz Szostok
;*
;*		Wersja: 1.0
;********************************************************************************

blendTwoImages PROC USES ebx edx ecx,		; te rejestry b�d� wykorzystywane w procedurze (nast�pi ich automatyczne od�o�enie i przywr�cenie)
					bitmaps: PTR PTR DWORD,	; obrazy wej�ciowe
					coords: PTR DWORD,		; indeksy pocz�tkowe i ko�cowe
					alpha: DWORD			; warto�� prze�roczysto�ci obrazu nak�adanego
	;============================== ZMIENNE LOKALNE =============================
	LOCAL	alphaBottom: DWORD, alphaTop: DWORD, ; warto�ci prze�roczysto�ci kolejno dla obrazu bazowego oraz nak�adanego
			start: DWORD, stop: DWORD,			 ; kolejno indeks pocz�tkowy i ko�cowy dla tablic pikseli
			rgba: Pixel							 ; struktura przechowuj�ca pojedy�czy piksel

	;============================= OPERACJE WST�PNE =============================
	call checkSSE2	; sprawdzenie dost�pno�ci instrukcji SSE2, informacja zwr�cona do EAX

	cmp eax, -1						; sprawdzenie czy procedura zwr�ci�a warto�� -1 
	je endProcWithCPUIDErr			; je�li tak to nast�puje zako�czenie procedury
									; z powodu braku wspierania instrukcji CPUID
	cmp eax, 0						; sprawdzenie czy procedura zwr�ci�a warto�� 0 
	je endProcWithSSE2AvailableErr	; je�li tak to nast�puje zako�czenie procedury
									; z powodu braku instrukcji SSE2
	cmp eax, 1						; sprawdzenie czy procedura zwr�ci�a wartosc 1
	jne	untrustedCheckSSE2Proc		; je�li nie to nale�y przerwa� dalsze obliczenia, poniewa�
									; funkcja CheckSSE2 nie zwr�ci�a �adnej z oczekwianych warto�ci 
									; wi�c nie dzia�a poprawnie, a wi�c nie mo�emy jej ufa�


	;================== USTAWIENIE ODPOWIEDNICH WAG DLA OBRAZ�W ==================
	mov eax, alpha					; wczytanie wagi(alpha) dla obrazu nak�adanego
	mov alphaTop, eax				; zainicjalizowanie zmiennej wag� obrazu nak�adanego
	mov eax, maxAlpha				; za�adowanie maksymalnej dozwolonej wagi 
	sub eax, alphaTop				; r�nica, kt�ra jest warto�ci� wagi(alpha) dla drugiego obrazu
	mov alphaBottom, eax			; za�adowanie warto�ci wagi drugiego obrazu
	
	MOVD	xmm5, alphaTop			; przepisanie warto�ci wagi obrazu nak�adanego do xmm5
	SHUFPS xmm5, xmm5, 0h			; powielenie jej na wszystkie 4 pola
	CVTDQ2PS xmm5, xmm5				; konwersja z double word na single-precision float

	MOVD	xmm6, alphaBottom		; przepisanie warto�ci drugiej wagi do xmm5
	SHUFPS xmm6, xmm6, 0h			; powielenie jej na wszystkie 4 pola
	CVTDQ2PS xmm6, xmm6				; konwersja z double word na single-precision float
									; poniewa� nie ma rozkazu dzielenia dla double word w SIMD

	MOVD	xmm7, maxAlpha			; przepisanie warto�ci wagi maksymalnej
	SHUFPS xmm7, xmm7, 0h			; powielenie jej na wszystkie 4 pola
	CVTDQ2PS xmm7, xmm7				; konwersja z double word na single-precision float

	DIVPS xmm5, xmm7				; zmiana zakresu z 0-255 na 0-1
	DIVPS xmm6, xmm7				; zmiana zakresu jak wy�ej

	;=================== ODCZYTANIE ZAKRESU MODYFIKACJI PIKSELI ==================
	mov ebx, [coords]				; zapisanie wska�nika na tablic�
	mov ecx, bytesPerPixel			; zna�adowanie mno�nika

	mov eax, [ebx + 0*SIZEOF DWORD] ; pobranie pierwszej kom�rki
	mul ecx							; coords[0] m�wi od kt�rego piksela zacz�� obliczenia
									; nale�y pomno�y� t� warto�� poprzez liczb� bajt�w na piksel
									; aby uzyka� odpowiednie przemieszczenie w 
	mov start, eax					; zainicjalizowanie zmiennej poprawn� warto�ci�

	mov eax, [ebx + 1*SIZEOF DWORD] ; pobranie drugiej kom�rki
	mul ecx							; zasada ta sama jw.
	mov stop, eax					; zainicjalizowanie zmiennej poprawn� warto�ci�

	;========================= ODCZYTANIE TABLIC OBRAZ�W =========================
	mov eax, [bitmaps]				; zapisanie wska�nika na wiersze 
	mov ebx, [eax + 0*SIZEOF DWORD]	; zapisanie wska�nika na pierwszy wiersz (obraz bazowy)
	mov edx, [eax + 1*SIZEOF DWORD]	; zapisanie wska�nika na drugi wiersz (obraz nak�adany)


	;========================= WA�ONE NAK�ADANIE OBRAZ�W =========================
	mov ecx, start					; zainicjalizowanie licznika p�tli
blendLoop:
	MOVDQU xmm1, [ebx + ecx*SIZEOF DWORD]	; za�adowanie 4 warto�ci (Move unaligned double quad words)
	MOVDQU xmm2, [edx + ecx*SIZEOF DWORD]	; za�adowanie 4 warto�ci (Move unaligned double quad words)
	
	CVTDQ2PS xmm1, xmm1				; konwersja z double word na single-precision float
	CVTDQ2PS xmm2, xmm2				; konwersja z double word na single-precision float

	MULPS xmm1, xmm6				; mno�enie r�wnoleg�e alphaBottom*bitmapBottom(x,y)
	MULPS xmm2, xmm5				; mno�enie r�wnoleg�e alphaTop*bitmapTop(x,y)
	
	ADDPS xmm1, xmm2				; dodanie warto�ci rgb o odpowiednich wagach

	CVTTPS2DQ xmm1, xmm1			; konwersja z single-precision na double words z obci�ciem
	CVTTPS2DQ xmm2, xmm2			; konwersja z single-precision na double words z obci�ciem
	
	MOVDQU [rgba], xmm1				; pobranie warto�ci do struktury w celu wy�uskania parametru 'a'
	mov [rgba.Pixel].a, 255			; przywr�cenie domy�lnej warto�ci parametru 'a'
	MOVDQU xmm1, [rgba]				; za�adowanie poprawnych warto�ci do rejestru

	MOVDQU [ebx + ecx*SIZEOF DWORD], xmm1	    ; nadpisanie warto�ci w kom�rkach bitmapy bazowej

	add ecx, bytesPerPixel			; zwi�kszenie licznika o jeden piksel kt�ry zosta� ju� obliczony
	cmp ecx, stop					; sprawdzenie czy nale�y ju� sko�czy�
jne blendLoop						; je�li ecx != stop to wr�� na pocz�tek p�tli, w przeciwnym wypadku
									; zako�cz obliczenia
done:
	mov eax, 1						; zapisanie informacji wyj�ciowej ( wszystko zosta�o wykonane poprawnie)
	ret								; powr�t z procedury
	
endProcWithSSE2AvailableErr:		; zako�czenie procedury z b�edem kt�ry jest ju� za�adowany w akumlatorze
endProcWithCPUIDErr:
	ret								; powr�t z procedury 
untrustedCheckSSE2Proc:
	mov eax, -2						; za�adowanie kodu b�edu 
	ret								; oraz powr�t z procedury
blendTwoImages ENDP
END									; koniec pliku