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


;********************************************************************************
;    Segment deklaracji sta³ych
;********************************************************************************
.const 

maxAlpha DD 255

;********************************************************************************
;    Segment kodu
;********************************************************************************
.code

;********************************************************************************
;*		Funkcja sprawdzaj¹ca dostêpnoœæ instrukcji SSE2.
;*		
;*		Zwraca informacje poprzez akumlator:
;*			1 - zestaw instrukcji SSE2 jest dostêpny
;*			0 - rozkaz cpuid nie jest wspierany b¹dŸ instrukcje SSE2 s¹ niedostêpne
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
;*		Parametry: 
;*			-bitmapList - dwuwymiarowa tablica typu byte** gdzie, 
;*							bitmapList[0] - obraz bazowy
;*							bitmapList[1] - obraz nak³adany
;*													 
;*			-coords	- dwuwymarowa tablica typu int** gdzie,
;*							coords[0] - indeks pocz¹tkowy od którego wykonywane
;*										bêd¹ obliczenia na tablicy bitmap.
;*							coords[1] - indeks na którym nale¿y zakoñczyæ obliczenia
;*										na tablicy bitmap. 
;*			-alpha - waga nak³adanego obrazu (przeŸroczystoœæ) w zakresie od 0 do 255. 										
;*
;*		Zwraca informacje poprzez akumlator:
;*			1 - procedura na³o¿enia obrazów przebieg³a pomyœlnie
;*			0 - wyst¹pi³ b³¹d podczas wykonywania procedury.
;*
;*		Autor: Mateusz Szostok
;*
;*		Wersja: 1.0
;********************************************************************************

blendTwoImages PROC bitmaps: PTR PTR DWORD, alpha: DWORD

	;============================== ZMIENNE LOKALNE =============================
	LOCAL alphaBottom: DWORD, alphaTop: DWORD


	;============================= OPERACJE WSTÊPNE =============================
	call checkSSE2	; sprawdzenie dostêpnoœci instrukcji SSE2, informacja zwrócona do EAX

	cmp eax, -1					   ; sprawdzenie czy procedura zwróci³a wartoœæ -1 
	je endProcWithCPUIDErr		   ; jeœli tak to nastêpuje zakoñczenie procedury
								   ; z powodu braku wspierania instrukcji CPUID
	cmp eax, 0					   ; sprawdzenie czy procedura zwróci³a wartoœæ 0 
	je endProcWithSSE2AvailableErr ; jeœli tak to nastêpuje zakoñczenie procedury
								   ; z powodu braku instrukcji SSE2
	cmp eax, 1					   ; sprawdzenie czy procedura zwróci³a wartosc 1
	jne	untrustedCheckSSE2Proc	   ; jeœli nie to nale¿y przerwaæ dalsze obliczenia, poniewa¿
								   ; funkcja CheckSSE2 nie zwróci³a ¿adnej z oczekwianych wartoœci 
								   ; wiêc nie dzia³a poprawnie, a wiêc nie mo¿emy jej ufaæ


	;================== USTAWIENIE ODPOWIEDNICH WAG DLA OBRAZÓW ==================
	mov eax, alpha				   ; wczytanie wagi(alpha) dla obrazu bazowego
	mov alphaTop, eax			   ; zainicjalizowanie zmiennej wag¹ obrazu bazowego
	mov eax, maxAlpha			   ; za³adowanie maksymalnej dozwolonej wagi 
	sub eax, alphaTop			   ; ró¿nica, która jest wartoœci¹ wagi(alpha) dla drugiego obrazu
	mov alphaBottom, eax		   ; zainicjalizowanie zmiennej wartoœci¹ wagi drugiego obrazu
	
	MOVD	xmm5, alphaTop		   ; przepisanie wartoœci wagi obrazu bazowego do xmm5
	shufps xmm5, xmm5, 0h		   ; powielenie jej na wszystkie 4 pola
	CVTDQ2PS xmm5, xmm5			   ; konwersja z double word na single-precision float

	MOVD	xmm6, alphaBottom      ; przepisanie wartoœci drugiej wagi do xmm5
	shufps xmm6, xmm6, 0h		   ; powielenie jej na wszystkie 4 pola
	CVTDQ2PS xmm6, xmm6			   ; konwersja z double word na single-precision float
								   ; poniewa¿ nie ma rozkazu dzielenia dla double word w SIMD

	MOVD	xmm7, maxAlpha		   ; przepisanie wartoœci wagi maksymalnej
	shufps xmm7, xmm7, 0h		   ; powielenie jej na wszystkie 4 pola
	CVTDQ2PS xmm7, xmm7			   ; konwersja z double word na single-precision float

	DIVPS xmm5, xmm7			   ; zmiana zakresu z 0-255 na 0-1
	DIVPS xmm6, xmm7			   ; zmiana zakresu jak wy¿ej

	;========================= ODCZYTANIE TABLIC OBRAZÓW =========================
	mov eax, [bitmaps]			   ; zapisanie wskaŸnika na wiersze 
	mov ebx, [eax + 0*4]		   ; zapisanie wskaŸnika na pierwszy wiersz (obraz bazowy)
	mov edx, [eax + 1*4]		   ; zapisanie wskaŸnika na drugi wiersz (obraz nak³adany)


	;========================= WA¯ONE NAK£ADANIE OBRAZÓW =========================
	mov ecx, 0;
	MOVDQU xmm1, [ebx + 4*ecx]	   ; za³adowanie 4 wartoœci (Move unaligned double quad words)
	MOVDQU xmm2, [edx + 4*ecx]	   ; za³adowanie 4 wartoœci (Move unaligned double quad words)
	
	CVTDQ2PS xmm1, xmm1			   ; konwersja z double word na single-precision float
	CVTDQ2PS xmm2, xmm2			   ; konwersja z double word na single-precision float

	MULPS xmm1, xmm6			   ; mno¿enie równoleg³e W*bitmap1(x,y)
	MULPS xmm2, xmm5			   ; mno¿enie równoleg³e (W-1)*bitmap2(x,y)
	
	ADDPS xmm1, xmm2			   ; dodanie wartoœci ( W*bitmap(x,y) +(1-W)*bitmap2(x,y) )

	CVTTPS2DQ xmm1, xmm1		   ; konwersja z single-precision na double words z obcieciem
	CVTTPS2DQ xmm2, xmm2
	
	movdqu [ebx + 4*ecx], xmm1	   ; zapisanie wartoœci w komórkach bitmapy bazowej
	mov eax, 255				   ; domyœlna wartoœc alpha dla bitmapy
	mov [ebx + 4*ecx + 4*3], eax   ; zapisanie jej w odpowiedniej komórce
	
	
endProcWithSSE2AvailableErr:
endProcWithCPUIDErr:
	ret

untrustedCheckSSE2Proc:
	mov eax, -2
	ret

blendTwoImages ENDP
END 