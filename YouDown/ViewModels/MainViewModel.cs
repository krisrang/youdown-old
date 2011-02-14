using System;
using System.ComponentModel.Composition;
using System.Deployment.Application;
using System.Windows.Shell;
using Lemon;
using Lemon.Services.Contracts;
using MEFedMVVM.Services.CommonServices;
using MEFedMVVM.Services.Contracts;
using MEFedMVVM.ViewModelLocator;
using YouDown.Services;

namespace YouDown.ViewModels
{
    [ExportViewModel("MainViewModel")]
    public class MainViewModel : ViewModelBase, IDesignTimeAware
    {
        #region Properties
        private TaskbarItemProgressState _progressState;
        public TaskbarItemProgressState ProgressState
        {
            get { return _progressState; }
            set { _progressState = value; NotifyOfPropertyChange(() => ProgressState); }
        }

        private double smallProgress;
        public double SmallProgress
        {
            get { return smallProgress; }
            set { smallProgress = value; NotifyOfPropertyChange(() => SmallProgress); }
        }

        private double _totalProgress;
        public double TotalProgress
        {
            get { return _totalProgress; }
            set { _totalProgress = value; NotifyOfPropertyChange(() => TotalProgress); }
        }

        private bool _screenChangeVisible;
        public bool ScreenChangeVisible
        {
            get { return _screenChangeVisible; }
            set { _screenChangeVisible = value; NotifyOfPropertyChange(() => ScreenChangeVisible); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; NotifyOfPropertyChange(() => IsBusy); }
        }

        private string _busyContent;
        public string BusyContent
        {
            get { return _busyContent; }
            set { _busyContent = value; NotifyOfPropertyChange(() => BusyContent); }
        }

        private double _busyProgress;
        public double BusyProgress
        {
            get { return _busyProgress; }
            set { _busyProgress = value; NotifyOfPropertyChange(() => BusyProgress); }
        }
        #endregion

        public delegate void TotalProgressChangeDelegate(double progress);

        public DelegateCommand SettingsCommand { get; set; }

        private readonly SettingsManager _settingsManager;
        private string _currentScreen;
            
        [ImportingConstructor]
        public MainViewModel(IContainerStatus containerStatus, IMediator mediator, SettingsManager settingsManager)
        {
            _settingsManager = settingsManager;

            mediator.Register(this);
            SettingsCommand = new DelegateCommand(ExecuteSettingsCommand);

            if (_settingsManager.Settings.CheckUpdatesOnLaunch)
                containerStatus.ContainerLoaded += () => UpdateCheck(true);

            ScreenChangeVisible = true;
        }

        public void DesignTimeInitialization()
        {
            TotalProgress = 67;
            ScreenChangeVisible = true;
        }

        [MediatorMessageSink(MediatorMessages.CheckUpdates, ParameterType = typeof(bool))]
        public void UpdateCheck(bool launchCheck)
        {
            
        }

        [MediatorMessageSink(MediatorMessages.TotalProgressChange, ParameterType = typeof(double))]
        private void TotalProgressChanged(double progress)
        {
            TotalProgress = progress;
            SmallProgress = TotalProgress / 100d;
        }

        [MediatorMessageSink(MediatorMessages.TaskbarProgressChange, ParameterType = typeof(TaskbarItemProgressState))]
        private void TaskbarProgressChanged(TaskbarItemProgressState state)
        {
            ProgressState = state;
        }

        [MediatorMessageSink(MediatorMessages.ScreenChange, ParameterType = typeof(string))]
        private void ScreenChanged(string screen)
        {
            _currentScreen = screen;
            ScreenChangeAllow(true);
        }

        [MediatorMessageSink(MediatorMessages.ScreenChangeAllow, ParameterType = typeof(bool))]
        private void ScreenChangeAllow(bool allow)
        {
            ScreenChangeVisible = allow && _currentScreen != "Settings";
        }

        private void ExecuteSettingsCommand(object args)
        {
            OpenScreen("Settings");
        }
    }
}
