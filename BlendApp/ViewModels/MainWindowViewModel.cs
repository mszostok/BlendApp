
namespace BlendApp.ViewModels
{
    using BlendApp.Commands;
    // using BlendApp.Commands;
    using BlendApp.Models;
    using System;
    using System.Diagnostics;
    using System.Windows.Controls;
    using System.Windows.Input;

    class MainWindowViewModel
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
        public void BlendImages()
        {
            Debug.Assert(false, String.Format("Liczba watkow: {0} ", AppSettings.ThreadNumer));

        }

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

        public void Img1Select()
        {
            AppSettings.Img1Path = OpenDialog();
        }

        public void Img2Select()
        {
            AppSettings.Img2Path = OpenDialog();
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
