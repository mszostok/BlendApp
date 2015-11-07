namespace BlendApp.Models
{
    using System;
    using System.ComponentModel;

    /**
     * Obiekt tej klasy przechowuje wszystkie ustawienia aplikacji, dzięki implenetacji
     * INotifyPropertyChanged umożliwia uaktualnianie tych właściwości na bieżąco. 
     * Natomiast implementacja IDataErrorInfo pozwala na prostą walidację danych - jeśli
     * użytkownik nie wprowadzi ścieżek dostępu do plików graficznyc przycisk "Połącz" 
     * będzie zablokowany. Należy mieć na uwadzę, iż ścieżki te nie muszą być poprawne -
     * tym czy są zajmuj się klasa BlendImagesSystem. Reszty właściwosci nie trzeba sprawdzać
     * gdyż posiadają wartości domyślne.
     */ 
    public class AppSettings : INotifyPropertyChanged, IDataErrorInfo
    {

        #region Constructors
        public AppSettings()
        {
            ThreadNumberRecommended = "Rekomendowana liczba wątków : " + NumberOfFreeLogicalProcessor().ToString();  
        }
        #endregion

        #region Alpha
        // wartość przeźroczystości
        private int alpha = 155;
        public int Alpha
        {
            get
            {
                return alpha;
            }
            set
            {
                if (alpha != value)
                {
                    alpha = value;
                    RaisePropertyChanged("Alpha");
                }
            }
        }
        #endregion

        #region ThreadNumberRecomended
        // Liczba rekomendowanej liczby wątków - domyślnie (liczba_wątków_procesora - 1)
        // (minus jeden bo program główny to pierwszy wątek) 
        public string ThreadNumberRecommended { get; set; }                                   
        #endregion

        #region ThreadCount
        // Liczba wątków ustalona przez użytkownika - domyślnie jest liczbą rekomendowaną
        private int threadNumber = NumberOfFreeLogicalProcessor();   
        public int ThreadNumber
        {
            get
            {
                return threadNumber;
            }
            set
            {
                if (threadNumber != value)
                {
                    threadNumber = value;
                    RaisePropertyChanged("TreadNumber");
                }
            }
        }
        #endregion

        #region Library Choice

        public string LibraryChoice      // Nazwa bibliteki udostępniającej nakładanie obrazów,
        {                               // która zostanie załadowana - domyślnie bibliteka C#
            get
            {
                
                return loadAsmLibrary ? "Asm" : "C#";
            }

        }

        private bool loadAsmLibrary;
        public bool LoadAsmLibrary
        {
            get
            {
                return loadAsmLibrary;
            }
            set
            {
                if (loadAsmLibrary != value)
                {
                    loadAsmLibrary = value;
                    RaisePropertyChanged("LoadAsmLibrary");
                }

            }
        }
        #endregion

        #region Images paths Img1Path Img2Path
        string img1Path = "C:\\Users\\indianer\\Pictures\\blendApp\\czerw.bmp";
        public string Img1Path
        {
            get
            {
                return img1Path;
            }
            set
            {
                if (img1Path != value)
                {
                    img1Path = value;
                    RaisePropertyChanged("Img1Path");
                }
            }
        }

        string img2Path = "C:\\Users\\indianer\\Pictures\\blendApp\\ziel.bmp";
        public string Img2Path
        {
            get
            {
                return img2Path;
            }
            set
            {
                if (img2Path != value)
                {
                    img2Path = value;
                    RaisePropertyChanged("Img2Path");
                }
            }
        }
        #endregion

        #region Result Image
        //Przechowuje scieżke dostępu do wynikowego pliku graficznego
        private string resultImage;
        public string ResultImage     
        {                             
            get
            {

                return resultImage;
            }
            set
            {
                if (resultImage != value)
                {
                    resultImage = value;
                    RaisePropertyChanged("ResultImage");
                }

            }

        }
        #endregion

        #region Functions
        //zwraca liczbę wolnych procesorów logicznych
        private static int NumberOfFreeLogicalProcessor()
        {
            return Environment.ProcessorCount-1;
        }
        #endregion

        #region INotifyPropertyChanged members
        void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(prop)); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region IDataErrorInfo members
        public string Error
        {
            get;
            private set;
        }

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "Img1Path": Error = String.IsNullOrWhiteSpace(Img1Path) ? "Err Path" : null;
                        break;
                    case "Img2Path": Error = String.IsNullOrWhiteSpace(Img2Path) ? "Err Path" : null;
                        break;
                }


                return Error;
            }

        }
        #endregion
    }
}
