namespace BlendApp.Commands
{
    using BlendApp.ViewModels;
    using System;
    using System.Windows.Input;

    class BlendAppSettingsCommand : ICommand
    {
        #region Members
        private MainWindowViewModel viewModel;
        #endregion

        #region Constructors
        public BlendAppSettingsCommand(MainWindowViewModel view)
        {
            viewModel = view;
        }
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
            return String.IsNullOrWhiteSpace(viewModel.AppSettings.Error);
        }


        public void Execute(object parameter)
        {
            viewModel.BlendImages();
        }
        #endregion
    }
}
