namespace BlendApp.Models
{
    using System;
    using System.ComponentModel;

    public class AppSettings : INotifyPropertyChanged, IDataErrorInfo
    {

        #region TreadNumberRecomended
        private int treadNumberRecomended = 4;   // Liczba wątków ustalona przez użytkownika - domyślnie 1
        public int TreadNumerRecomended
        {
            get
            {
                return treadNumberRecomended;
            }
            set
            {
                if (treadNumberRecomended != value)
                {
                    treadNumberRecomended = value;
                    RaisePropertyChanged("TreadNumberRecomended");
                }
            }
        }
        #endregion

        #region TreadCount
        private int treadNumber = 4;   // Liczba wątków ustalona przez użytkownika - domyślnie 1
        public int TreadNumer
        {
            get
            {
                return treadNumber;
            }
            set
            {
                if (treadNumber != value)
                {
                    treadNumber = value;
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
        string img1Path;
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

        string img2Path;
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
