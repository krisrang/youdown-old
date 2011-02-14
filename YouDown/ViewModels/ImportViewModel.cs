using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google.YouTube;
using Lemon;
using MEFedMVVM.Services.Contracts;
using MEFedMVVM.ViewModelLocator;
using YouDown.Models;
using YouDown.Properties;
using Video = YouDown.Models.Video;

namespace YouDown.ViewModels
{
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [ExportViewModel("ImportViewModel")]
    public class ImportViewModel : ViewModelBase
    {
        #region Properties
        private string _targetPath;
        public string TargetPath
        {
            get { return _targetPath; }
            set { _targetPath = value; NotifyOfPropertyChange(() => TargetPath); }
        }

        private string _username;
        public string Username
        {
            get { return _username; }
            set { _username = value; NotifyOfPropertyChange(() => Username); }
        }

        private bool _fileImport;
        public bool FileImport
        {
            get { return _fileImport; }
            set { _fileImport = value; NotifyOfPropertyChange(() => FileImport); }
        }

        private bool _favImport;
        public bool FavImport
        {
            get { return _favImport; }
            set { _favImport = value; NotifyOfPropertyChange(() => FavImport); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; NotifyOfPropertyChange(() => IsBusy); }
        }

        private bool _done;
        public bool Done
        {
            get { return _done; }
            set { _done = value; NotifyOfPropertyChange(() => Done); }
        }

        private string _status;
        public string Status
        {
            get { return _status; }
            set { _status = value; NotifyOfPropertyChange(() => Status); }
        }

        public List<VideoQuality> QualityList { get { return VideoQualities.DefaultQualityList; } }

        private VideoQuality _selectedQuality;
        public VideoQuality SelectedQuality
        {
            get { return _selectedQuality; }
            set
            {
                _selectedQuality = value;
                NotifyOfPropertyChange(() => SelectedQuality);
            }
        }
        #endregion

        public ImportCollection Videos { get; set; }

        public DelegateCommand NextCommand { get; set; }
        public DelegateCommand TargetCommand { get; set; }
        public DelegateCommand AddCommand { get; set; }

        private readonly IVisualStateManager _visualStateManager;
        private readonly IMediator _mediator;

        [ImportingConstructor]
        public ImportViewModel(IVisualStateManager visualStateManager, IContainerStatus containerStatus, IMediator mediator)
        {
            _visualStateManager = visualStateManager;
            _mediator = mediator;

            Videos = new ImportCollection();

            NextCommand = new DelegateCommand(ExecuteNextCommand);
            TargetCommand = new DelegateCommand(ExecuteTargetCommand);
            AddCommand = new DelegateCommand(ExecuteAddCommand);

            SelectedQuality = QualityList[0];
            FileImport = true;

            containerStatus.ContainerUnloaded += ContainerClosed;
        }

        private void ContainerClosed()
        {
            
        }

        public void ExecuteTargetCommand(object args)
        {
            var dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.Multiselect = false;
            dialog.Title = Resources.DialogSelectList;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                TargetPath = dialog.FileName;
            }
        }

        private void ExecuteNextCommand(object args)
        {
            IsBusy = true;
            Status = "Loading...";

            Videos.ImportFinished += () =>
                                           {
                                               IsBusy = false;
                                               Execute.OnUIThread(() => _visualStateManager.GoToState("ImportList"));
                                           };
            Videos.ProcessingFinished += () =>
                                             {
                                                 Status = "Done";
                                                 Done = true;
                                             };

            Videos.StartImport(FileImport ? TargetPath : Username, FileImport, SelectedQuality);
        }

        private void ExecuteAddCommand(object args)
        {
            _mediator.NotifyColleagues(MediatorMessages.ImportVideos, Videos);
            Close();
        }
    }
}
