using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Shell;
using Lemon;
using Lemon.Services.Contracts;
using MEFedMVVM.Services.CommonServices;
using MEFedMVVM.Services.Contracts;
using MEFedMVVM.ViewModelLocator;
using YouDown.Models;
using YouDown.Services;

namespace YouDown.ViewModels
{
    [ExportViewModel("DownloadViewModel")]
    public class DownloadViewModel : ViewModelBase, IDesignTimeAware
    {
        #region Properties
        private bool _uiEnabled;
        public bool UIEnabled
        {
            get { return _uiEnabled; }
            set { _uiEnabled = value; NotifyOfPropertyChange(() => UIEnabled); }
        }

        private bool _doneVisible;
        public bool DoneVisible
        {
            get { return _doneVisible; }
            set { _doneVisible = value; NotifyOfPropertyChange(() => DoneVisible); }
        }

        private string _addressText;
        public string AddressText
        {
            get { return _addressText; }
            set { _addressText = value; NotifyOfPropertyChange(() => AddressText); }
        }

        private string _targetPath;
        public string TargetPath
        {
            get { return _targetPath; }
            set 
            { 
                _targetPath = value;
                _settingsManager.Settings.TargetPath = value;
                _settingsManager.Save();
                NotifyOfPropertyChange(() => TargetPath); 
            }
        }

        private TaskbarItemProgressState _progressState;
        public TaskbarItemProgressState ProgressState
        {
            get { return _progressState; }
            set 
            { 
                _progressState = value; 
                NotifyOfPropertyChange(() => ProgressState); 
                _mediator.NotifyColleagues(MediatorMessages.TaskbarProgressChange, value);
            }
        }

        public List<VideoQuality> QualityList { get { return VideoQualities.DefaultQualityList; } }

        private VideoQuality _selectedQuality;
        public VideoQuality SelectedQuality
        {
            get { return _selectedQuality; }
            set
            {
                _selectedQuality = value;
                _settingsManager.Settings.MaxQuality = value.Id;
                _settingsManager.Save();
                NotifyOfPropertyChange(() => SelectedQuality);
            }
        }
        #endregion

        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand TargetCommand { get; set; }
        public DelegateCommand GoCommand { get; set; }
        public DelegateCommand StopCommand { get; set; }
        public DelegateCommand ImportCommand { get; set; }

        public VideoQueue Queue { get; set; }
        public ObservableCollection<Video> FinishedVideos { get; set; }

        private readonly IMessageBox _messageBox;
        private readonly IMediator _mediator;
        private readonly SettingsManager _settingsManager;
            
        [ImportingConstructor]
        public DownloadViewModel(IMessageBox messageBox, IMediator mediator, SettingsManager settingsManager)
        {
            _messageBox = messageBox;
            _mediator = mediator;
            _settingsManager = settingsManager;

            _mediator.Register(this);

            UIEnabled = true;

            SelectedQuality = QualityList.FirstOrDefault(q => q.Id == _settingsManager.Settings.MaxQuality) ?? QualityList[0];
            TargetPath = _settingsManager.Settings.TargetPath;

            AddCommand = new DelegateCommand(ExecuteAddCommand);
            TargetCommand = new DelegateCommand(ExecuteTargetCommand);
            GoCommand = new DelegateCommand(ExecuteGoCommand);
            StopCommand = new DelegateCommand(ExecuteStopCommand);

            ImportCommand = new DelegateCommand(ExecuteImportCommand);

            Queue = new VideoQueue();
            Queue.TotalProgressChanged += TotalProgressChanged;
            Queue.QueueStateChanged += QueueStateChanged;
            Queue.VideoFinished += VideoFinished;

            FinishedVideos = new ObservableCollection<Video>();
        }

        public void DesignTimeInitialization()
        {
            Queue.InProgress = true;
            DoneVisible = false;
        }

        public void ExecuteImportCommand(object args)
        {
            GetOverlay("Import").Show();
        }

        public void ExecuteAddCommand(object args)
        {
            if (!String.IsNullOrWhiteSpace(AddressText))
            {
                if (!Video.VerifyAddress(AddressText))
                {
                    _messageBox.Show(
                        "The specified address does not seem to be a valid YouTube video address. Please check the address for errors.",
                        "Invalid address", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    
                    return;
                }

                var newVid = new Video(AddressText, SelectedQuality);

                if (!Queue.Contains(newVid))
                    Queue.AddVideo(newVid);

                AddressText = String.Empty;
            }
        }

        public void ExecuteTargetCommand(object args)
        {
            var dialog = new FolderBrowserDialog { ShowNewFolderButton = true };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                TargetPath = dialog.SelectedPath;
            }
        }

        public void ExecuteGoCommand(object args)
        {
            //ExecuteAddCommand(null);

            if (Queue.Count > 0)
            {
                Queue.Start(TargetPath);
                Analytics.DownloadStart(Queue.Count);
            }
        }

        public void ExecuteStopCommand(object args)
        {
            Queue.Stop();
        }

        private void QueueStateChanged(object sender, QueueStateChangedEventArgs e)
        {
            if (Queue.InProgress)
            {
                UIEnabled = false;
                FinishedVideos.Clear();
                ProgressState = TaskbarItemProgressState.Normal;
            }
            else
            {
                UIEnabled = true;
                DoneVisible = false;
                ProgressState = TaskbarItemProgressState.None;

                if (FinishedVideos.Count > 0)
                {
                    var summaryView = GetOverlay("Summary");
                    summaryView.DataContext = this;
                    summaryView.Show();
                }
            }
        }

        private void TotalProgressChanged(object sender, TotalProgressChangedEventArgs e)
        {
            _mediator.NotifyColleagues(MediatorMessages.TotalProgressChange, e.Value);
        }

        private void VideoFinished(object sender, VideoFinishedEventArgs e)
        {
            FinishedVideos.Insert(0, e.Video);

            if (FinishedVideos.Count > 0 && Queue.InProgress)
            {
                DoneVisible = true;
                Analytics.DownloadFinish(FinishedVideos.Count(v => v.State == VideoState.Downloaded));
            }
        }

        [MediatorMessageSink(MediatorMessages.ImportVideos, ParameterType = typeof(IEnumerable<Video>))]
        private void ImportVideos(IEnumerable<Video> import)
        {
            foreach (var video in import)
            {
                Queue.AddVideo(video, true);
            }
        }
    }
}
