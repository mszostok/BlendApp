namespace BlendApp.Commands
{
    using BlendApp.ViewModels;
    using System;
    using System.Windows.Input;

    class ImgSelectAppSettingsCommand : ICommand
    {
         #region Members
        private MainWindowViewModel viewModel;
        #endregion

        #region Constructors
        public ImgSelectAppSettingsCommand(MainWindowViewModel view)
        {
            viewModel = view;
        }
        #endregion

        #region Properties
       
        #endregion

        #region Functions

        #endregion

        #region Commands
       
        #endregion

        #region ICommand Members
        public event System.EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }



        public void Execute(object parameter)
        {
            switch(((string)parameter))
            {
                case "Img1": viewModel.Img1Select();
                    break;
                case "Img2": viewModel.Img2Select();
                    break;
            }

        }
        #endregion
    }
}
