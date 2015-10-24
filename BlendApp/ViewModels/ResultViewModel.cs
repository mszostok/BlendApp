namespace BlendApp.ViewModels
{
    using System.ComponentModel;

    internal class ResultViewModel : INotifyPropertyChanged
    {
        #region Members
        private string imgPath;
        #endregion

        #region Constructors
        public ResultViewModel()
        {
            
        }
        #endregion

        #region Properties
        public string ImgPath
        {
            get
            {
                return imgPath;
            }
            set
            {
                imgPath = value;
                RaisePropertyChanged("ImgPath");
            }
        }
        #endregion

        #region Functions

        #endregion

        #region Commands
       
        #endregion

        #region INotifyPropertyChanged members
        void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(prop)); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
