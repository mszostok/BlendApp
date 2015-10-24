namespace BlendApp.Models
{
    using Microsoft.Win32;
    using System;
    using System.IO;
    using System.Windows;
    using System.Windows.Media.Imaging;

    public class BlendImagesSystem
    {
        

        #region Members
        private AppSettings appSettings;
        #endregion

        #region Constructors
        public BlendImagesSystem(AppSettings initialData)
        {
            appSettings = initialData;
        }
        #endregion

        #region Properties

        #endregion

        #region Functions
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
        
        public void BlendImages()
        {

            try
            {
                #region Load Bitmap
                BitmapImage img1 = new BitmapImage(new Uri(appSettings.Img1Path));
                BitmapImage img2 = new BitmapImage(new Uri(appSettings.Img2Path));
                
                WriteableBitmap img1Bitmap = new WriteableBitmap(img1);
                WriteableBitmap img2Bitmap = new WriteableBitmap(img2);
                #endregion

                #region Algorithm - DLL

                #region Initialization

                // ustawienie kanału alfa (przeźroczystości)
                float alpha = 100;
                float alphaDst = (float)alpha / 255.0F;
                float alphaSrc = 1.0F - alphaDst;

                int img1LinePadding; // liczba bajtów wyrównujacych wiersz do wielokrotności 4
                int img2LinePadding;
                int img1ModuloFromDivWidthByFour = ((int)img1.Width * 3) % 4;
                int img2ModuloFromDivWidthByFour = ((int)img2.Width * 3) % 4;

                /**
                 * ustalenie odpowiedniego wyrównania, jeśli wiersz dzieli się przez 4 bez reszty, wtedy w zapisie dodatkowe bajty nie występują
                 * w przeciwnym wypadku wyrównanie równa się liczbie brakujących bajtów aby wiersz był wielokrotnością 4.
                 */
                img1LinePadding = (img1ModuloFromDivWidthByFour == 0) ? 0 : (4 - img1ModuloFromDivWidthByFour);
                img2LinePadding = (img2ModuloFromDivWidthByFour == 0) ? 0 : (4 - img2ModuloFromDivWidthByFour);

                int img1Stride = ((int)img1.Width * 4) + img1LinePadding; // rozmiar wiersza w bajtach
                int img2Stride = ((int)img2.Width * 4) + img2LinePadding;

                int img1ArraySize = img1Stride * (int)img1.Height;       // rozmiar tablicy pikseli (ilosc_bajtow_w_wierszu * liczba_wierszy)
                int img2ArraySize = img2Stride * (int)img2.Height; 

                byte[] img1Pixels = new byte[img1ArraySize];            // tablice pikseli
                byte[] img2Pixels = new byte[img2ArraySize];  

                img1Bitmap.CopyPixels(img1Pixels, img1Stride, 0);  // kopiowanie tablicy pikseli z bitmapy, z ustalonym krokiem, zaczynając od 0
                img2Bitmap.CopyPixels(img2Pixels, img2Stride, 0);


                #endregion

                #region Main Algorithm 
                

        

                for (int i = 0, j = 0; i < img1Pixels.Length / 4; ++i)
                {

                    img1Pixels[j] = (byte)((alphaSrc * (float)img1Pixels[j]) +
                                    (alphaDst * (float)img2Pixels[j]));
                    ++j;
                    img1Pixels[j] = (byte)((alphaSrc * (float)img1Pixels[j]) +
                                    (alphaDst * (float)img2Pixels[j]));
                    ++j;
                    img1Pixels[j] = (byte)((alphaSrc * (float)img1Pixels[j]) +
                                    (alphaDst * (float)img2Pixels[j]));
                    ++j;
                    img1Pixels[j] = 255;
                    ++j;
                  

                }

                #endregion

                #endregion

                #region Save Result Bitmap
                Int32Rect rect = new Int32Rect(0, 0, (int)img1.Width, (int)img1.Height);
                img1Bitmap.WritePixels(rect, img1Pixels, img1Stride, 0);

                SaveDialog(img1Bitmap);
                #endregion

            }
            catch (UriFormatException ex)
            {

               // ErrLabel.Content = ex.Message;
            }
          
        }
        #endregion
       
    }
}
