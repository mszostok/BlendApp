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


    public class BlendImagesSystem
    {
        //[DllImport("BlendAlgorithm.dll")]
        //public static extern void blendToImages(byte[] imgBottom, byte[] imgTop, float alpha, int start, int stop);


        #region Members
        private AppSettings appSettings;

        private int maxWidth;
        private int maxHeight;
        private int threadPixelsStep;
        private byte[] img1Pixels;
        private byte[] img2Pixels;

        private List<Thread> threadList;
        private List<BitmapImage> bmpList;
        private List<BitmapImage> croppedBmpList;
        #endregion

        #region Constructors
        public BlendImagesSystem(AppSettings initialData)
        {
            appSettings = initialData;
            threadList = new List<Thread>();
            bmpList = new List<BitmapImage>();
            croppedBmpList = new List<BitmapImage>();
            maxWidth = maxHeight = int.MaxValue;

        }
        #endregion

        #region Properties

        #endregion

        #region Functions
        /**
         * Wyświetlenie dialogu do zapisu bitmapy będącej wynikiem nałożenia
         * przekazanych obrazów.
         */
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

        private void SetMaxArraySize()
        {

            foreach (BitmapImage bmp in bmpList)
            {
                maxWidth = Math.Min(maxWidth, (int)Math.Round(bmp.Width));
                maxHeight = Math.Min(maxHeight, (int)Math.Round(bmp.Height));
            }
        }

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

        private void LoadImagesFromUserPaths()
        {
            /*
             * Jesli w przyszłości od użytkownika będzie pobierane więcej ścieżek
             * w postaci kolekcji, to wtedy będzie można je ładować w prosty sposób
             * poprzez pętle foreach
             */
            bmpList.Add(new BitmapImage(new Uri(appSettings.Img1Path)));
            bmpList.Add(new BitmapImage(new Uri(appSettings.Img2Path)));
        }

        private void createNewThread(int start, int stop)
        {
            if (appSettings.LoadAsmLibrary == true)
            {
                return;
            }
            else
            {
                var t = new Thread(() =>
                BlendAlgorithm.BlendImage.blendToImages(img1Pixels, img2Pixels, appSettings.Alpha, start, stop));
                t.Start();
                threadList.Add(t);
                Thread.Sleep(5); // ustabilizowanie wątku, jeśli nie odczekamy czasu granicznego to pobierze on złe dane.
            }
           
        }

        private void createThreadList(int threadNumber)
        {
            int start = 0;
            int stop =  threadPixelsStep;

            for (int i = 0; i < threadNumber; ++i)
            {
                createNewThread(start, stop);

                start +=  threadPixelsStep;
                stop +=  threadPixelsStep;

            }
        }
        /// <summary>
        /// Główna funkcja realizująca ważone nakładanie obrazów.
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

            img1Pixels = new byte[arraySize];            // tablice pikseli
            img2Pixels = new byte[arraySize];
       
            img1Bitmap.CopyPixels(img1Pixels, stride, 0);  // kopiowanie tablicy pikseli z bitmapy do tablicy bajtów, z ustalonym krokiem, zaczynając od 0
            img2Bitmap.CopyPixels(img2Pixels, stride, 0);
            #endregion

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
                createNewThread( start, arraySize/ bytesPerPixel); // utworzenie n-tego wątku obliczającego pozostałe piksele

            }
            else
            {
                createThreadList(appSettings.ThreadNumber);
            }

            //oczekiwanie na zakończenie obliczeń przez wszystkie wątki
            foreach (Thread th in threadList)
            {
                th.Join();
            }

            #endregion

            #region Save Result Bitmap

            Int32Rect rect = new Int32Rect(0, 0, maxWidth, maxHeight);
            img1Bitmap.WritePixels(rect, img1Pixels, stride, 0); // zapisanie wyniku obliczeń do bitmapy

            SaveDialog(img1Bitmap); //zapisanie bitmapy na dysku
            #endregion
        }
        #endregion
      
    }


}
