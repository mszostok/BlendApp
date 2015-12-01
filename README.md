# ![](http://i.imgur.com/du0LvG2.png?1) BlendApp

Aplikacja stworzona z my�l� o wa�onym nak�adaniu obraz�w w formacie .bmp, a tym samy podgl�du wyniku dzia�ania algorytmu.


# **Co umo�liwia ?**

#### **1) Wybranie liczby w�tk�w**

U�ytkownik mo�e zada� liczb� w�tk�w, kt�ra oznacza na ile cz�ci zostan� podzielone obrazy, 

wywo�uj�c dla ka�dej z nich algorytm dodawania obraz�w.

##### Dodatkowo program dostarcza r�wnie� informacji jak� jest rekomendowana tj. wolna liczba w�tk�w do wykorzystania.


#### **2) Wybranie biblioteki**

Mo�liwe jest wskazanie odpowiedniej biblioteki napisanej w j�zku asemblera lub C#, 
kt�ra udost�pnia funkcj� realizuj�c� **ten sam** algorytm wa�onego nak�adania obraz�w. 

#### **3) Podanie wagi nak�adanego obrazu**

Warto�� ta powinna by� z przedzia�u ***od 0 do 255*** i oznacza ona z jak� "si��" zostanie na�o�ony drugi obraz, na obraz bazowy. 

  * 0 - obraz nak�adany nie b�dzie widoczny
    * 
    * 
  * 255 - obraz bazowy zostanie ca�kowicie przys�oni�ty przez obraz nak�adany

#### **5) Wy�wietlenie obrazu wynikowego**

Klikaj�c przycisk *Po��cz* zostaniemy poproszeni o wskazanie miejsca gdzie nale�y zapisa� obraz wynikowy, 
a nast�pnie zostanie on wy�wietlony w oknie aplikacji.


# **Zale�no�ci i ograniczenia**

Aktualna wersja aplikacji by�a pisana i testowana w �rodowisku *32-bitowy*. Jednak uruchomienie jej w systemie *64-bitowy* r�wnie� nie powinno przysporzy� problem�w.

 * Ograniczenia
    * Obs�ugiwanym formatem pliku jest **24-bitowa grafika bitmapowa bez kompresji RLE o DPI r�wnym 96**,
    ograniczenia te nie wynikaj� z zaimplementowanego algorytmu, tylko z zastosowanej technologi WPF 
    odczytuj�cej bitmapy.

# **License**


```
This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
```
#### Copyright (C) 2015 **Mateusz Szostok**
