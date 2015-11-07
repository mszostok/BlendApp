namespace BlendApp.Commands
{
    using System;
    using System.Windows.Input;
    using BlendApp.ViewModels;

    public class BlendAppSettingsCommand : BaseCommand
    {

        #region Constructors
        public BlendAppSettingsCommand(MainWindowViewModel view) : base(view) { }
        #endregion

        #region ICommand Members  
        public override bool CanExecute(object parameter)
        {
            //jeśli jest jakiś bład należy zablokować przycisk
            return String.IsNullOrWhiteSpace(viewModel.AppSettings.Error);
        }

        public override void Execute(object parameter)
        {
            viewModel.BlendImages();
        }
        #endregion
    }
}
