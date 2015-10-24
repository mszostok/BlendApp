using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlendApp.Models
{
    class WindowData : INotifyPropertyChanged
    {
        #region Title
        private string title;   // Liczba wątków ustalona przez użytkownika - domyślnie 1
        
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
        private bool isExpanded =true;   // Liczba wątków ustalona przez użytkownika - domyślnie 1
        
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
    }
}
