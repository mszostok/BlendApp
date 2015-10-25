namespace BlendApp.Commands
{
    using BlendApp.ViewModels;
    using System.Windows.Input;

    class BaseCommand : ICommand
    {
        #region Members
        protected MainWindowViewModel viewModel;
        #endregion

        #region Constructors
        public BaseCommand(MainWindowViewModel view)
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

        public virtual bool CanExecute(object parameter)
        {
            return true;
        }

        public virtual void Execute(object parameter)
        {
            throw new System.NotImplementedException();
        }
        #endregion

    }
}
