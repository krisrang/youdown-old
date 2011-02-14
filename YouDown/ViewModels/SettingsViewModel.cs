using System;
using System.ComponentModel.Composition;
using System.Deployment.Application;
using Lemon;
using MEFedMVVM.Services.Contracts;
using MEFedMVVM.ViewModelLocator;
using YouDown.Services;

namespace YouDown.ViewModels
{
    [ExportViewModel("SettingsViewModel")]
    public class SettingsViewModel : ViewModelBase
    {
        #region Properties
        public bool CheckUpdatesOnLaunch
        {
            get { return _settingsManager.Settings.CheckUpdatesOnLaunch; }
            set
            {
                _settingsManager.Settings.CheckUpdatesOnLaunch = value;
                NotifyOfPropertyChange(() => CheckUpdatesOnLaunch);
            }
        }

        private Version _version;
        public string Version
        {
            get
            {
                if (_version == null)
                {
                    _version = Lemon.Utils.App.GetVersion();
                }

                return String.Format("{0}.{1}.{2}.{3}", _version.Major, _version.Minor, _version.Build, _version.Revision);
            }
            set {}
        }
        #endregion

        public DelegateCommand UpdateCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand CloseCommand { get; set; }

        private readonly IMediator _mediator;
        private readonly SettingsManager _settingsManager;
        private readonly SettingsData _oldSettings;

        [ImportingConstructor]
        public SettingsViewModel(IMediator mediator, SettingsManager settingsManager)
        {
            _mediator = mediator;
            _settingsManager = settingsManager;
            _oldSettings = _settingsManager.Settings.Clone();

            UpdateCommand = new DelegateCommand(ExecuteUpdateCommand);
            SaveCommand = new DelegateCommand(ExecuteSaveCommand);
            CloseCommand = new DelegateCommand(ExecuteCloseCommand);
        }

        private void ExecuteUpdateCommand(object args)
        {
            //_mediator.NotifyColleagues(MediatorMessages.CheckUpdates, false);
        }

        private void ExecuteSaveCommand(object args)
        {
            _settingsManager.Save();
            OpenScreen("Download");
        }

        private void ExecuteCloseCommand(object args)
        {
            _settingsManager.Settings = _oldSettings;
            OpenScreen("Download");
        }
    }
}
