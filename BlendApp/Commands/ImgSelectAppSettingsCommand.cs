namespace BlendApp.Commands
{
    using BlendApp.ViewModels;
    using System;
    using System.Windows.Input;

    class ImgSelectAppSettingsCommand : BaseCommand
    {

        #region Constructors
        public ImgSelectAppSettingsCommand(MainWindowViewModel view) : base(view) { }
        #endregion

        #region ICommand Members
        public override void Execute(object parameter)
        {
            viewModel.OpenDialogForImage((string)parameter);
        }
        #endregion
    }
}
