using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lemon;
using MEFedMVVM.Services.Contracts;

namespace YouDown.Models
{
    /// <summary>
    /// First in, last out video download queue
    /// </summary>
    public class VideoQueue : ObservableCollection<Video>
    {
        public event QueueStateChangedEventHandler QueueStateChanged = delegate { };
        public event TotalProgressChangedEventHandler TotalProgressChanged = delegate { };
        public event VideoFinishedEventHandler VideoFinished = delegate { };

        private double smallProgress;
        public double SmallProgress
        {
            get { return smallProgress; }
            set { smallProgress = value; NotifyOfPropertyChange(() => SmallProgress); }
        }

        private double totalProgress;
        public double TotalProgress
        {
            get { return totalProgress; }
            set
            {
                totalProgress = value; 
                NotifyOfPropertyChange(() => TotalProgress);
                InvokeTotalProgressChanged(value);
            }
        }

        private bool inProgress;
        public bool InProgress
        {
            get { return inProgress; }
            set { inProgress = value; NotifyOfPropertyChange(() => InProgress); }
        }

        private int startedCount;
        private int currentCount;

        private Video currentlyDownloading;
        private string targetPath;

        public VideoQueue()
        {
            TotalProgress = 0d;
            InProgress = false;
        }

        public void AddVideo(Video item)
        {
            AddVideo(item, true);
        }

        public void AddVideo(Video item, bool animate)
        {
            item.PropertyChanged += VideoPropertyChanged;

            if (animate)
                Insert(0, item);
            else
                Add(item);

            if (item.State == VideoState.None)
                item.BeginProcessing();
        }

        public void RemoveVideo(Video item)
        {
            if (currentlyDownloading == item)
            {
                currentlyDownloading.StopDownload();
                currentlyDownloading = null;
            }

            item.PropertyChanged -= VideoPropertyChanged;
            Remove(item);

            DownloadNext();
        }

        public void Start(string path)
        {
            InProgress = true;
            targetPath = path;
            startedCount = Count;
            InvokeQueueStateChanged();

            DownloadNext();
        }

        public void Stop()
        {
            if (currentlyDownloading != null && currentlyDownloading.State == VideoState.Downloading)
            {
                currentlyDownloading.StopDownload();
                currentlyDownloading.State = VideoState.Ready;
            }

            ResetState();
        }

        private void DownloadNext()
        {
            if (InProgress && (currentlyDownloading == null || currentlyDownloading.State != VideoState.Downloading))
            {
                var nextVid = this.FirstOrDefault(v => v.State == VideoState.Ready);

                if (nextVid != null)
                {
                    currentlyDownloading = nextVid;
                    nextVid.StartDownload(targetPath);
                }
                else
                {
                    ResetState();
                }
            }
        }

        private void ResetState()
        {
            startedCount = currentCount = 0;
            TotalProgress = 0d;
            InProgress = false;
            InvokeQueueStateChanged();
        }

        private void VideoPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var video = (Video)sender;

            if (e.PropertyName == "State")
            {
                if (video.State == VideoState.Downloaded)
                    currentCount++;

                if (video.State == VideoState.Downloaded || video.State == VideoState.Error || video.State == VideoState.Cancelled)
                {
                    InvokeVideoFinished(new VideoFinishedEventArgs(video));
                    RemoveVideo(video);
                }
            }
            else if (e.PropertyName == "Progress" && video.State == VideoState.Downloading)
            {
                TotalProgress = (video.Progress + currentCount*100)/startedCount;
                SmallProgress = TotalProgress/100d;

                InvokeTotalProgressChanged(TotalProgress);
            }
        }

        private void InvokeQueueStateChanged()
        {
            if (QueueStateChanged != null)
                QueueStateChanged(this, new QueueStateChangedEventArgs());
        }

        private void InvokeTotalProgressChanged(double progress)
        {
            if (TotalProgressChanged != null)
                TotalProgressChanged(this, new TotalProgressChangedEventArgs(progress));
        }

        private void InvokeVideoFinished(VideoFinishedEventArgs e)
        {
            if (VideoFinished != null)
                VideoFinished(this, e);
        }

        public void NotifyOfPropertyChange(string propertyName)
        {
            Execute.OnUIThread(() => OnPropertyChanged(new PropertyChangedEventArgs(propertyName)));
        }

        public void NotifyOfPropertyChange<TProperty>(Expression<Func<TProperty>> property)
        {
            var lambda = (LambdaExpression)property;

            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression)
            {
                var unaryExpression = (UnaryExpression)lambda.Body;
                memberExpression = (MemberExpression)unaryExpression.Operand;
            }
            else memberExpression = (MemberExpression)lambda.Body;

            NotifyOfPropertyChange(memberExpression.Member.Name);
        }
    }

    public delegate void QueueStateChangedEventHandler(object sender, QueueStateChangedEventArgs e);
    public delegate void TotalProgressChangedEventHandler(object sender, TotalProgressChangedEventArgs e);
    public delegate void VideoFinishedEventHandler(object sender, VideoFinishedEventArgs e);

    public class QueueStateChangedEventArgs : EventArgs {}

    public class TotalProgressChangedEventArgs : EventArgs
    {
        public double Value { get; set; }

        public TotalProgressChangedEventArgs(double progress)
        {
            Value = progress;
        }
    }

    public class VideoFinishedEventArgs : EventArgs
    {
        public VideoState NewState { get; set; }
        public Video Video { get; set; }

        public VideoFinishedEventArgs(Video item)
        {
            Video = item;
            NewState = item.State;
        }
    }
}
