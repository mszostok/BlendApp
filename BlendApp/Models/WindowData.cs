
namespace BlendApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;


    public class WindowData : INotifyPropertyChanged
    {
        #region Title
        private string title;   
        
        public string Title
        {
            get
            {
                if(IsExpanded == true)
                {
                    title = "Ustawienia programu";
                }
                else{
                    title = "Podgląd połączonych obrazów";
                }
                return title;
            }
            set
            {
                if (title != value)
                {
                    title = value;
                    RaisePropertyChanged("Title");
                }
            }
        }
        #endregion

            
        #region IsExpanded
        private bool isExpanded = true;   
        
        public bool IsExpanded
        {
            get
            {
                return isExpanded;
            }
            set
            {
                if (isExpanded != value)
                {
                    isExpanded = value;
                    RaisePropertyChanged("Title");
                    RaisePropertyChanged("IsExpanded");
                }
            }
        }
        #endregion

        #region WaitingScreen
        private bool waitingScreen = false;

        public object WaitingScreen
        {
            get
            {
                return waitingScreen ? Visibility.Visible : Visibility.Collapsed;
            }
            set
            {
                waitingScreen = (bool)value;
                RaisePropertyChanged("WaitingScreen");
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
    }
}
