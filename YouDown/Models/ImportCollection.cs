using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.GData.YouTube;
using Google.YouTube;
using Lemon;

namespace YouDown.Models
{
    public class ImportCollection : ObservableCollection<Video>
    {
        public event Action ImportFinished;
        public event Action ProcessingFinished;

        public ImportCollection()
        {
        }

        public void StartImport(string target, bool isFileImport, VideoQuality maxQuality)
        {
            Task.Factory.StartNew(() =>
            {
                if (isFileImport)
                    ImportFile(target, maxQuality);
                else
                    ImportFavs(target, maxQuality);
            });
        }

        private void ImportFile(string target, VideoQuality maxQuality)
        {
            using (var stream = File.OpenRead(target))
            {
                using (var reader = new StreamReader(stream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (Video.VerifyAddress(line.Trim()))
                        {
                            var vid = new Video(line.Trim(), maxQuality);

                            if (!Contains(vid))
                            {
                                vid.PropertyChanged += VideoChanged;
                                Execute.OnUIThread(() => Insert(0, vid));
                                vid.BeginProcessing();
                            }
                        }
                    }
                }
            }

            InvokeImportFinished();
        }

        private void ImportFavs(string target, VideoQuality maxQuality)
        {
            var req =
                    new YouTubeRequest(new YouTubeRequestSettings("KRYDwn",
                                                                  "AI39si7NaaziR7b4XO290VzZ8i4ICOyh6nFAQwzsj1G90GoAjYYpWVsPCgUo3odTP3w65ahc69Av0BinLHTbrxVEtGYbY5gUDw"));

            var query = new YouTubeQuery(YouTubeQuery.CreateFavoritesUri(target));
            var feed = req.Get<Google.YouTube.Video>(query);

            var fetched = 0;

            var total = feed.TotalResults;
            var start = feed.StartIndex;
            var perpage = feed.PageSize;

            while (fetched < total)
            {
                var pageQuery = new YouTubeQuery(YouTubeQuery.CreateFavoritesUri(target));
                pageQuery.StartIndex = fetched;
                var pageFeed = req.Get<Google.YouTube.Video>(pageQuery);

                foreach (var entry in pageFeed.Entries)
                {
                    if (entry.WatchPage != null)
                    {
                        var vid = new Video(entry.WatchPage.ToString(), maxQuality);

                        if (!Contains(vid))
                        {
                            vid.PropertyChanged += VideoChanged;
                            Execute.OnUIThread(() => Insert(0, vid));
                            vid.BeginProcessing();
                        }
                    }

                    fetched++;
                }
            }

            InvokeImportFinished();
        }

        private void VideoChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "State")
            {
                var vid = sender as Video;

                if (vid == null)
                    return;

                if (vid.State == VideoState.Cancelled || vid.State == VideoState.Error)
                    Execute.OnUIThread(() => Remove(vid));

                if (this.Count(v => v.State == VideoState.None) == 0 && this.Count(v => v.State == VideoState.Processing) == 0)
                    InvokeProcessingFinished();
            }
        }

        private void InvokeImportFinished()
        {
            if (ImportFinished != null)
                ImportFinished();
        }

        private void InvokeProcessingFinished()
        {
            if (ProcessingFinished != null)
                ProcessingFinished();
        }
    }
}
