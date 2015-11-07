namespace BlendApp.Commands
{
    using BlendApp.ViewModels;
    using System.Windows.Input;

    public abstract class BaseCommand : ICommand
    {
        #region Members
        protected MainWindowViewModel viewModel;
        #endregion

        #region Constructors
        protected BaseCommand(MainWindowViewModel view)
        {
            this.viewModel = view;
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

        public abstract void Execute(object parameter);
        #endregion

    }
}
