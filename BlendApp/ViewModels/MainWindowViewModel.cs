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


        #region Constructors
        public MainWindowViewModel()
        {
            AppSettings = new AppSettings();
            Window = new WindowData();
            BlendImagesCommand = new BlendAppSettingsCommand(this);
            ImgSelectCommand = new ImgSelectAppSettingsCommand(this);

        }
        #endregion

        #region Properties
        // przechowyanie ustawień użytkownika
        public AppSettings AppSettings { get; set; }

        // ustawienia wyświetlanego okna
        public WindowData Window {get; set;}

        #endregion

        #region Functions

        /// <summary>
        /// Wyświetlenie dialogu do otwarcie bitmapy
        /// </summary>
        /// <returns>zwraca ścieżkę do pliku graficznego.</returns>
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

        /// <summary>
        /// Pobranie ścieżki do pierwszego obrazu.
        /// </summary>
        private void Img1Select()
        {
            AppSettings.Img1Path = OpenDialog();
        }

        /// <summary>
        /// Pobranie ścieżki do drugiego obrazu.
        /// </summary>
        private void Img2Select()
        {
            AppSettings.Img2Path = OpenDialog();
        }

        /// <summary>
        /// Wyświetlenie 'okna wynikowego' poprzez zwinięcie expandera.
        /// </summary>
        private void ShowResult()
        {
            Window.IsExpanded = false;
        }

        /// <summary>
        /// Stworzenie dialogu dla danego obrazu.
        /// </summary>
        /// <param name="param">identyfikator wywołującego metodę.</param>
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

        /// <summary>
        /// Realizowanie nałożenia dwóch obrazow poprzez utworzenie obiektu będącego 
        /// systemem który udostępnia taką funkcjonalność.
        /// </summary>
        public void BlendImages()
        {
            AppSettings.ResultImage = "";
            BlendImagesSystem blendSystem = new BlendImagesSystem(AppSettings);

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
