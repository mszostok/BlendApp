
namespace BlendApp.ViewModels
{
    using BlendApp.Commands;
    using BlendApp.Models;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    public class MainWindowViewModel
    {
        #region Members
        private AppSettings appSettings;
        private WindowData window;
        #endregion

        #region Constructors
        public MainWindowViewModel()
        {
            appSettings = new AppSettings();
            window = new WindowData();
            BlendImagesCommand = new BlendAppSettingsCommand(this);
            ImgSelectCommand = new ImgSelectAppSettingsCommand(this);

        }
        #endregion

        #region Properties
        public bool CanBlend
        {
            get
            {
                return true;
            }
        }
        

        public AppSettings AppSettings
        {
            get
            {
                return appSettings;
            }
        }

        public WindowData Window
        {
            get
            {
                return window;
            }
        }

        #endregion

        #region Functions
        // Wyświetlenie dialogu do otwarcie bitmapy
        private string OpenDialog()
        {
            
            // Utworzenie dialogu do wyboru katalogu
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Ustawienie filtrów
            dlg.DefaultExt = ".bmp";
            dlg.Filter = "Bitmap (.bmp)|*.bmp";

            // Wyświetlenie okna dialogowego 
            Nullable<bool> result = dlg.ShowDialog();

            // Pobranie nazwy i ustawnienie jej w odpowiednim textbox'ie
            if (result == true)
            {
                return dlg.FileName;
            }
            return "";
        }

        private void Img1Select()
        {
            AppSettings.Img1Path = OpenDialog();
        }

        private void Img2Select()
        {
            AppSettings.Img2Path = OpenDialog();
        }

        // Wyświetlenie 'okna wynikowego' poprzez zwinięcie expandera
        private void ShowResult()
        {
            window.IsExpanded = false;
        }

        public void OpenDialogForImage(string param)
        {
            switch (param)
            {
                case "Img1": Img1Select();
                    break;
                case "Img2": Img2Select();
                    break;
            }
        }

        /**
         * Realizowanie nałożenia dwóch obrazow poprzez utworzenie obiektu będącego
         * sytemem który udostępnia taką funkcjonalnosć
         */
        public void BlendImages()
        {
            appSettings.ResultImage = "";
            BlendImagesSystem blendSystem = new BlendImagesSystem(appSettings);

            try
            {
                blendSystem.BlendImages();
                ShowResult();
            }
            catch (UriFormatException)
            {
                MessageBox.Show("Brak poprawnych ścieżek!", "Bład");                
            }
            catch (IOException)
            {
                MessageBox.Show("Proszę zapisać plik pod inną nazwą.", "Bład");
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("DPI obrazów musi być równe 96.", "Bład");
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Bład : {0}", ex.Message), "Bład");
            }


        }
        #endregion

        #region Commands
        public ICommand ImgSelectCommand
        {
            get;
            private set;
        }

        public ICommand BlendImagesCommand
        {
            get;
            private set;
        }
        #endregion

    }
}
