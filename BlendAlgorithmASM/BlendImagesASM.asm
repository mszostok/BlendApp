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
.NOCREF ; wy³¹czenie generowania listy symboli 

INCLUDE   c:\masm32\include\windows.inc 

.LIST ; w³¹czenie generowania listy symboli

;********************************************************************************
;    Segment deklaracji zmiennych
;******************************************************************************** 


;********************************************************************************
;    Segment kodu
;********************************************************************************
;*
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

blendTwoImages PROC rads: PTR DWORD,

	call checkSSE2	; sprawdzenie dostêpnoœci instrukcji SSE2, informacja zwrócona do EAX
	ret
blendTwoImages ENDP
END