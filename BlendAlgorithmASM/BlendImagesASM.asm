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
.NOCREF ; wy��czenie generowania listy symboli 

INCLUDE   c:\masm32\include\windows.inc 

.LIST ; w��czenie generowania listy symboli

;********************************************************************************
;    Segment deklaracji zmiennych
;******************************************************************************** 


;********************************************************************************
;    Segment kodu
;********************************************************************************
;*
;*		Funkcja sprawdzaj�ca dost�pno�� instrukcji SSE2.
;*		
;*		Zwraca informacje poprzez akumlator:
;*			1 - zestaw instrukcji SSE2 jest dost�pny
;*			0 - rozkaz cpuid nie jest wspierany b�d� instrukcje SSE2 s� niedost�pne
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
;*		Parametry: 
;*			-bitmapList - dwuwymiarowa tablica typu byte** gdzie, 
;*							bitmapList[0] - obraz bazowy
;*							bitmapList[1] - obraz nak�adany
;*													 
;*			-coords	- dwuwymarowa tablica typu int** gdzie,
;*							coords[0] - indeks pocz�tkowy od kt�rego wykonywane
;*										b�d� obliczenia na tablicy bitmap.
;*							coords[1] - indeks na kt�rym nale�y zako�czy� obliczenia
;*										na tablicy bitmap. 
;*			-alpha - waga nak�adanego obrazu (prze�roczysto��) w zakresie od 0 do 255. 										
;*
;*		Zwraca informacje poprzez akumlator:
;*			1 - procedura na�o�enia obraz�w przebieg�a pomy�lnie
;*			0 - wyst�pi� b��d podczas wykonywania procedury.
;*
;*		Autor: Mateusz Szostok
;*
;*		Wersja: 1.0
;********************************************************************************

blendTwoImages PROC rads: PTR DWORD,

	call checkSSE2	; sprawdzenie dost�pno�ci instrukcji SSE2, informacja zwr�cona do EAX
	ret
blendTwoImages ENDP
END