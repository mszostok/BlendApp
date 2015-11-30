namespace BlendApp.Models
{
    using Microsoft.Win32;
    using System;
    using System.IO;
    using System.Windows;
    using System.Windows.Media.Imaging;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Obiekt tej klasy odpowiedzialny jest zarówno za wczytanie jak i nałożenie
    /// dwóch obrazów uwzględniając wagę dla obrazu nakładanego.
    /// </summary>
    public unsafe class BlendImagesSystem
    {
        [DllImport("BlendAlgorithm.dll")]
        public static extern void blendToImages(byte[][] bitmaps, int[] coords, int alpha);

        [DllImport("BlendAlgorithmASM.dll")]
        public static unsafe extern int blendTwoImages(int** bitmaps, int* coords, int alpha);

        #region Members
        private AppSettings appSettings;    // referencja do obiektu przechowujacego ustawienia użytkownika

        private int maxWidth;            // maksymalna szerokość pliku graficznego
        private int maxHeight;          // maksymalna wysokość pliku graficznego

        private int threadPixelsStep;  // ilość pikseli przypadająca na jeden wątek

        private byte[] img1PixelsByte;    // tablica pikseli na której wykonywane 
        private byte[] img2PixelsByte;

        private int[] img1PixelsInt;    
        private int[] img2PixelsInt;

        private int** intBitmapsList;
        private byte[][] byteBitmapsList;

        private List<Thread> threadList;            // lista utworzonych wątków podczas wykonywania obliczeń
        private List<BitmapImage> bmpList;         // lista wczytanych plików graficznych podanych przez użytkownika
        private List<BitmapImage> croppedBmpList; // lista plików graficznych o tych samych rozmiarach
        #endregion

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialData"> referencja do aktualnego obiektuz ustawieniami </param>
        public BlendImagesSystem(AppSettings initialData)
        {
            appSettings = initialData;

            threadList = new List<Thread>();
            bmpList = new List<BitmapImage>();
            croppedBmpList = new List<BitmapImage>();
            maxWidth = maxHeight = int.MaxValue;

        }
        #endregion

        #region Functions

        /// <summary>
        /// Wyświetlenie dialogu do zapisu bitmpay będącej wynikiem nałożenia
        /// przekazanych obrazów.
        /// </summary>
        /// <param name="resultBitmap">bitmapa która ma zostać zapisana</param>
        private void SaveDialog(WriteableBitmap resultBitmap)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Pliki BMP | *.bmp";
            if (saveFileDialog.ShowDialog() == true)
            {

                FileStream saveStream = new FileStream(saveFileDialog.FileName, FileMode.OpenOrCreate);
                BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(resultBitmap));
                encoder.Save(saveStream);
                saveStream.Close();

                //ustawienie ścieżki dla podglądu wyniku działania programu
                  appSettings.ResultImage = saveFileDialog.FileName;
            }
        }

        /// <summary>
        /// Załadowanie obrazu podanych przez użytkownika
        /// </summary>
        private void LoadImagesFromUserPaths()
        {
            /*
             * Jesli w przyszłości od użytkownika będzie pobierane więcej ścieżek
             * w postaci kolekcji, to wtedy będzie można je ładować w prosty sposób
             * poprzez pętle foreach
             */
            if (File.Exists(appSettings.Img1Path) == false)
            {
                throw new UriFormatException("Wrong path1");
            }
            if (File.Exists(appSettings.Img2Path) == false)
            {
                throw new UriFormatException("Wrong path2");
            }


            bmpList.Add(new BitmapImage(new Uri(appSettings.Img1Path)));
            bmpList.Add(new BitmapImage(new Uri(appSettings.Img2Path)));
        }

        /// <summary>
        /// Ustalenia maksymalnej wielkości tablicy poprzez
        /// sprawdzenie znalezienie bitmapy onamniejszej 
        /// szerokości oraz wysokości.
        /// </summary>
        private void SetMaxArraySize()
        {

            foreach (BitmapImage bmp in bmpList)
            {
                maxWidth = Math.Min(maxWidth, (int)Math.Round(bmp.Width));
                maxHeight = Math.Min(maxHeight, (int)Math.Round(bmp.Height));
            }
        }

        /// <summary>
        /// Przycięcie wszystkich bitmap do tej samej wielkości
        /// </summary>
        private void CropAllImagesToMaxArraySize()
        {
            foreach (BitmapImage bmp in bmpList)
            {
                CroppedBitmap cb = new CroppedBitmap(bmp, new Int32Rect(0, 0, maxWidth, maxHeight));

                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                MemoryStream memoryStream = new MemoryStream();
                BitmapImage croppedBmp = new BitmapImage();

                encoder.Frames.Add(BitmapFrame.Create(cb));
                encoder.Save(memoryStream);

                croppedBmp.BeginInit();
                croppedBmp.StreamSource = new MemoryStream(memoryStream.ToArray());
                croppedBmp.EndInit();

                croppedBmpList.Add(croppedBmp);

                memoryStream.Close();
            }
        }


        private int*  convertCoords(int[] coords){
            int[] coordsTmp = { coords[0], coords[1] };
            fixed (int* coordsPtr = coordsTmp)
            {
                return coordsPtr;
            }
        }

        /// <summary>
        /// Utworzenie nowego wątku nakładającego obrazy.
        /// </summary>
        /// <param name="start">definiuje indeks początkowy od którego 
        /// wątek będzie wykoywał obliczenia</param>
        /// <param name="stop">
        /// definiuje ineks końcowy na którym wątek zatrzyma obliczenia</param>
        private unsafe void createNewThread(int[] coords)
        {
            if (appSettings.LoadAsmLibrary == true)
            {
                int* coordsPtr = convertCoords(coords);

                   var t = new Thread(() =>
                                blendTwoImages(intBitmapsList, coordsPtr, (int)appSettings.Alpha));
                   threadList.Add(t);
            }
            else
            {
                int[] coordsTmp = { coords[0], coords[1] };
                
                var t = new Thread(() =>
                BlendAlgorithm.BlendImage.blendToImages(byteBitmapsList, coordsTmp, (int)appSettings.Alpha));
                threadList.Add(t);
            }
           
        }

        /// <summary>
        /// Utorzenie listy wątków które odpowiedzialne są za nałożenie 
        /// obrazów na siebie.
        /// </summary>
        /// <param name="threadNumber">ilość wątków jaka ma zostać utworzona.</param>
        private void createThreadList(int threadNumber)
        {
            int[] coords = { 0, threadPixelsStep };

            for (int i = 0; i < threadNumber; ++i)
            {
                createNewThread(coords);

                coords[0] += threadPixelsStep;
                coords[1] += threadPixelsStep;

            }
        }

        /// <summary>
        /// Metoda typu Coarse-grained, realizująca ważone nakładanie obrazów.
        /// </summary>
        public unsafe void BlendImages()
        {
            #region Load Bitmap
            LoadImagesFromUserPaths();

            SetMaxArraySize();
            CropAllImagesToMaxArraySize();

            WriteableBitmap img1Bitmap = new WriteableBitmap(croppedBmpList[0]);
            WriteableBitmap img2Bitmap = new WriteableBitmap(croppedBmpList[1]);
            #endregion

            #region Initialization

            int linePadding; // liczba bajtów wyrównujacych wiersz do wielokrotności 4
            int moduloFromDivWidthByFour = ((int)Math.Round(croppedBmpList[0].Width) * 3) % 4;

            /*
             * Ustalenie odpowiedniego wyrównania, jeśli wiersz dzieli się przez 4 bez reszty, wtedy w zapisie dodatkowe bajty nie występują
             * w przeciwnym wypadku wyrównanie równa się liczbie brakujących bajtów aby wiersz był wielokrotnością 4.
             */
            linePadding = (moduloFromDivWidthByFour == 0) ? 0 : (4 - moduloFromDivWidthByFour);

            int stride = ((int)Math.Round(croppedBmpList[0].Width) * 4) + linePadding; // rozmiar wiersza w bajtach

            int arraySize = stride * (int)Math.Round(croppedBmpList[0].Height);       // rozmiar tablicy pikseli (ilosc_bajtow_w_wierszu * liczba_wierszy)

            img1PixelsByte = new byte[arraySize];            // tablice pikseli
            img2PixelsByte = new byte[arraySize];
       
            img1Bitmap.CopyPixels(img1PixelsByte, stride, 0);  // kopiowanie tablicy pikseli z bitmapy do tablicy bajtów, z ustalonym krokiem, zaczynając od 0
            img2Bitmap.CopyPixels(img2PixelsByte, stride, 0);
            #endregion

            /*
             * Jeśli będzie używany algortym z biblioteki ASM należy 
             * utworzyć tablicę pikseli, w postaci wskaźników.
             */
            if (appSettings.LoadAsmLibrary == true) 
            {
                intBitmapsList = createIntArrayFromByte();
            }
            else
            {
                byteBitmapsList = new byte[2][];

                byteBitmapsList[0] = img1PixelsByte;
                byteBitmapsList[1] = img2PixelsByte;
            }
            #region Main Algorithm

            int bytesPerPixel =  img1Bitmap.Format.BitsPerPixel / 8;
            int pixelsNumber = arraySize / bytesPerPixel;  // liczba wszystkich pikseli 

            threadPixelsStep = pixelsNumber / appSettings.ThreadNumber; // liczba pikseli dla pojedyńczego wątku

            /**
             * Jeśli nie można dokonać równego podziału to zostaje stworzonych n - 1 wątków
             * z regularnym krokiem, gdzie n - liczba wątków zadana przez użytkownika,
             * natomiast wątek n który wykonuje obliczenia dla pozostałej reszty pikseli.
             * 
             * W przeciwnym wypadku zostaje utworzonych n regularnych wątków.
             */
            if ( threadPixelsStep * bytesPerPixel * appSettings.ThreadNumber != arraySize) 
            {
                int delta = pixelsNumber - ( threadPixelsStep * appSettings.ThreadNumber);               

                createThreadList(appSettings.ThreadNumber-1);   // utworzenie n-1 wątków

                int start = threadPixelsStep * (appSettings.ThreadNumber - 1);
                int[] coords = { start, arraySize / bytesPerPixel };
                createNewThread(coords); // utworzenie n-tego wątku obliczającego pozostałe piksele

            }
            else
            {
                createThreadList(appSettings.ThreadNumber);
            }

            Stopwatch clock = new Stopwatch();
            clock.Start();

            //uruchomienie wszystkich wątków
            foreach (Thread th in threadList)
            {
                th.Start();
            }


            //oczekiwanie na zakończenie obliczeń przez wszystkie wątki
            foreach (Thread th in threadList)
            {
                th.Join();
            }


            clock.Stop();
            appSettings.ResultTime = "Czas wykonania " + clock.ElapsedMilliseconds.ToString() + " ms.";

            #endregion

            #region Save Result Bitmap

            if (appSettings.LoadAsmLibrary == true)
            {
                copyAsmResultToImg1PixelsByte();
            }
            Int32Rect rect = new Int32Rect(0, 0, maxWidth, maxHeight);
            img1Bitmap.WritePixels(rect, img1PixelsByte, stride, 0); // zapisanie wyniku obliczeń do bitmapy

            SaveDialog(img1Bitmap); //zapisanie bitmapy na dysku
            #endregion
        }

        /// <summary>
        /// Metoda wykorzystywana do zainicjalizowania zmiennej <code>bitmapList</code>
        /// która wykorzystywana jest przez algortym asemblera.
        /// </summary>
        /// <returns>Zwraca tablicę wskaźników na wskaźniki w stylu C/C++</returns>
        private unsafe int** createIntArrayFromByte()
        {
            img1PixelsInt = new int[img1PixelsByte.Length];
            img2PixelsInt = new int[img2PixelsByte.Length];

            for (int i = 0; i < img1PixelsByte.Length; ++i)
            {
                img1PixelsInt[i] = img1PixelsByte[i];
                img2PixelsInt[i] = img2PixelsByte[i];
            }

            fixed (int* img1Ptr = &img1PixelsInt[0], img2Ptr = &img2PixelsInt[0])
            {
                int*[] PtrArray = new int*[2];

                PtrArray[0] = img1Ptr;
                PtrArray[1] = img2Ptr;


                fixed (int** bitmaps = &PtrArray[0])
                {
                    return bitmaps;
                }
               

            }
        }

        /// <summary>
        /// Przekopiowanie obliczeń wykonanych przez bibliotekę asemblerową, 
        /// do tablicy bajtów pixeli obrazu bazowego.
        /// </summary>
        private unsafe void copyAsmResultToImg1PixelsByte()
        {
            for (int i = 0; i < img1PixelsByte.Length; ++i)
            {
                img1PixelsByte[i] = (byte)img1PixelsInt[i];
            }
        }
        #endregion
      
    }


}
