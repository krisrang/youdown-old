using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Lemon;
using Lemon.Utils;

namespace YouDown.Models
{
    public class Video : NotifyPropertyChangedBase, IEquatable<Video>
    {
        #region Properties
        private Uri address;
        public Uri Address
        {
            get { return address; }
            set
            {
                address = value;
                NotifyOfPropertyChange(() => Address);
            }
        }

        private string videoId;
        public string VideoId
        {
            get { return videoId; }
            set
            {
                videoId = value;
                NotifyOfPropertyChange(() => VideoId);
            }
        }

        private string error;
        public string Error
        {
            get { return error; }
            set
            {
                error = value;
                NotifyOfPropertyChange(() => Error);
                NotifyOfPropertyChange(() => HasError);
            }
        }

        public bool HasError { get { return !String.IsNullOrWhiteSpace(Error); } }

        private string title;
        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                NotifyOfPropertyChange(() => Title);
            }
        }

        private string targetPath;
        public string TargetPath
        {
            get { return targetPath; }
            set
            {
                targetPath = value;
                NotifyOfPropertyChange(() => TargetPath);
            }
        }

        private Uri thumbnail;
        public Uri Thumbnail
        {
            get { return thumbnail; }
            set
            {
                thumbnail = value;
                NotifyOfPropertyChange(() => Thumbnail);
            }
        }

        private double progress;
        public double Progress
        {
            get { return progress; }
            set
            {
                progress = value;
                NotifyOfPropertyChange(() => Progress);
            }
        }

        private string progressText;
        public string ProgressText
        {
            get { return progressText; }
            set
            {
                progressText = value;
                NotifyOfPropertyChange(() => ProgressText);
            }
        }

        public string SummaryText
        {
             get
             {
                 switch (State)
                 {
                     case VideoState.Downloaded:
                         return "Downloaded to: " + TargetPath;
                     case VideoState.Error:
                         return "Error: " + Error;
                     case VideoState.Cancelled:
                         return "Cancelled";
                     default:
                         return String.Empty;
                 }
             }
        }
        public bool Downloading { get; set; }

        private VideoState state;
        public VideoState State
        {
            get { return state; }
            set
            {
                state = value;
                Downloading = state == VideoState.Downloading;
                NotifyOfPropertyChange(() => State);
                NotifyOfPropertyChange(() => SummaryText);
                NotifyOfPropertyChange(() => Downloading);
            }
        }

        private VideoQuality downloadQuality;
        public VideoQuality DownloadQuality
        {
            get { return downloadQuality; }
            set
            {
                downloadQuality = value;
                NotifyOfPropertyChange(() => DownloadQuality);
            }
        }

        private VideoQuality maxQuality;
        public VideoQuality MaxQuality
        {
            get { return maxQuality; }
            set
            {
                maxQuality = value;
                NotifyOfPropertyChange(() => MaxQuality);
            }
        }
        #endregion

        public List<VideoQuality> AvailableQualities { get; set; }

        public Uri RealThumbnail { get { return new Uri("http://i1.ytimg.com/vi/" + VideoId + "/default.jpg"); } }
        public const string RegexAddress = @"^.*youtube.com\/[w\/|watch].*v=([a-zA-Z0-9_-]+).*";

        private IDictionary<string, string> _urlParams;
        private IDictionary<string, string> _flashVars;
        private IDictionary<int, string> _fmtMap;
        private string _page;

        private Stream _strResponse;
        private Stream _strLocal;
        private HttpWebRequest _webRequest;
        private HttpWebResponse _webResponse;

        private int _total;
        private int _start;
        private int _end;
        private int _bytes;

        const int Limit = 1572864;

        private WebClient _downloadClient;

        public static bool VerifyAddress(string address)
        {
            return Regex.IsMatch(address, RegexAddress, RegexOptions.IgnoreCase);
        }

        public Video(string address, VideoQuality selectedQuality)
        {
            Thumbnail = new Uri("pack://application:,,,/Assets/default.jpg", UriKind.Absolute);
            AvailableQualities = new List<VideoQuality>();

            MaxQuality = selectedQuality;

            Address = address.StartsWith("http://") ? new Uri(address) : new Uri(new Uri("http://www.youtube.com"), address.Split(new[] { "/" }, StringSplitOptions.None)[1]);

            if (Address.Query == null)
            {
                throw new ArgumentException("Address does not appear to be a valid YouTube video address");
            }

            _urlParams = ParseParams(Address.Query);
            VideoId = _urlParams["v"];
            Title = "Processing " + VideoId + "...";
        }

        public void BeginProcessing()
        {
            Task.Factory.StartNew(ProcessVideo);
        }

        public void StartDownload(string path)
        {
            if (State != VideoState.Downloading)
            {
                State = VideoState.Downloading;

                var uri = new Uri(_fmtMap[DownloadQuality.Id]);
                TargetPath = Path.Combine(path, SanitizeTitleForPath(Title) + DownloadQuality.Extension);

                //if (File.Exists(TargetPath))
                //    TargetPath = Path.Combine(path, SanitizeTitleForPath(Title) + DateTime.UtcNow.ToShortTimeString() + DownloadQuality.Extension);

                _webResponse = (HttpWebResponse)((HttpWebRequest)WebRequest.Create(uri)).GetResponse();
                _total = (int)_webResponse.ContentLength;
                _webResponse.Close();

                _bytes = 0;
                _start = 0;
                _end = Limit;

                ThreadPool.QueueUserWorkItem(s =>
                                                 {
                                                    //_strLocal = new FileStream(TargetPath, FileMode.Create, FileAccess.Write, FileShare.Read);

                                                    while (_start < _total && State == VideoState.Downloading)
                                                    {
                                                        Download(uri.ToString(), TargetPath, _start, _end);

                                                        _start = _end + 1;
                                                        _end = Math.Min(_end + Limit, _total);
                                                    }

                                                     //_strLocal.Close();

                                                     if (State == VideoState.Downloading)
                                                        State = VideoState.Downloaded;
                                                 });
            }
        }

        public void StopDownload()
        {
        }

        private void Download(string url, string target, int start, int end)
        {
            try
            {
                // Create a request to the file we are downloading
                _webRequest = (HttpWebRequest)WebRequest.Create(url);
                // Set the starting point of the request
                _webRequest.AddRange(start, end);

                // Set default authentication for retrieving the file
                _webRequest.Credentials = CredentialCache.DefaultCredentials;
                // Retrieve the response from the server
                _webResponse = (HttpWebResponse)_webRequest.GetResponse();
                // Ask the server for the file size and store it
                Int64 fileSize = _webResponse.ContentLength;

                // Open the URL for download
                _strResponse = _webResponse.GetResponseStream();

                if (_strResponse != null)
                {
                    // It will store the current number of bytes we retrieved from the server
                    var bytesSize = 0;
                    // A buffer for storing and writing the data retrieved from the server
                    var downBuffer = new byte[Limit / 2];

                    _strLocal = start == 0
                        ? new FileStream(target, FileMode.Create, FileAccess.Write, FileShare.Read)
                        : new FileStream(target, FileMode.Append, FileAccess.Write, FileShare.Read);

                    // Loop through the buffer until the buffer is empty
                    while ((bytesSize = _strResponse.Read(downBuffer, 0, downBuffer.Length)) > 0)
                    {
                        // Write the data from the buffer to the local hard drive
                        _strLocal.Write(downBuffer, 0, bytesSize);
                        // Invoke the method that updates the form's label and progress bar

                        _bytes = _bytes + bytesSize;

                        Progress = _bytes / (_total / 100d);

                        var num = (_bytes > 1024 * 1024)
                                      ? (_bytes / 1024d / 1024d)
                                      : (_bytes / 1024d);
                        var str = (_bytes > 1024 * 1024)
                                      ? "MB"
                                      : "KB";

                        ProgressText = String.Format("{0} {1} / {2} MB",
                                                     Math.Round(num, 1), str,
                                                     Math.Round(
                                                         (_total /
                                                          1024d) / 1024d, 1));

                        if (State != VideoState.Downloading)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    State = VideoState.Error;
                    Error = "Unknown error: " + _webResponse.StatusDescription;
                }
            }
            catch (WebException ex)
            {
                State = VideoState.Error;
                Error = "Error: " + ex.Message;
            }
            finally
            {
                if (_strResponse != null)
                    _strResponse.Close();

                if (_strLocal != null)
                    _strLocal.Close();

                if (State == VideoState.Cancelled)
                    Error = "Cancelled";

                if (State != VideoState.Downloading && File.Exists(TargetPath))
                    File.Delete(TargetPath);
            }
        }

        public void OldStartDownload(string path)
        {
            State = VideoState.Downloading;

            var uri = new Uri(_fmtMap[DownloadQuality.Id]);
            TargetPath = Path.Combine(path, SanitizeTitleForPath(Title) + DownloadQuality.Extension);

            //if (File.Exists(TargetPath))
            //    TargetPath = Path.Combine(path, SanitizeTitleForPath(Title) + DateTime.UtcNow.ToShortTimeString() + DownloadQuality.Extension);

            if (_downloadClient == null)
            {
                _downloadClient = new WebClient();
                _downloadClient.Proxy = null;
                _downloadClient.DownloadProgressChanged += (s, a) =>
                                                                {
                                                                    Progress =
                                                                        a.BytesReceived / (a.TotalBytesToReceive / 100d);

                                                                    var num =
                                                                        (a.BytesReceived > 1024 * 1024)
                                                                            ? (a.BytesReceived / 1024d / 1024d)
                                                                            : (a.BytesReceived / 1024d);
                                                                    var str =
                                                                        (a.BytesReceived > 1024 * 1024)
                                                                            ? "MB"
                                                                            : "KB";

                                                                    ProgressText =
                                                                        String.Format("{0} {1} / {2} MB", Math.Round(num, 1), str, Math.Round((a.TotalBytesToReceive / 1024d) / 1024d, 1));
                                                                };
                _downloadClient.DownloadFileCompleted += (s, a) =>
                                                            {
                                                                if (!a.Cancelled)
                                                                    State = VideoState.Downloaded;

                                                                if (a.Cancelled && File.Exists(TargetPath))
                                                                {
                                                                    Error = "Cancelled";
                                                                    File.Delete(TargetPath);
                                                                }

                                                                _downloadClient.Dispose();
                                                            };
            }

            _downloadClient.DownloadFileAsync(uri, TargetPath);
        }

        public void OldStopDownload()
        {
            if (_downloadClient != null && _downloadClient.IsBusy)
            {
                _downloadClient.CancelAsync();
            }
        }

        public void Cancel()
        {
            State = VideoState.Cancelled;
        }

        public string SanitizeTitleForPath(string titleDirty)
        {
            var builder = new StringBuilder();
            var invalid = System.IO.Path.GetInvalidFileNameChars();

            foreach (var cur in titleDirty)
            {
                if (!invalid.Contains(cur))
                    builder.Append(cur);
            }

            return builder.ToString();
        }

        #region Processing
        private void ProcessVideo()
        {
            State = VideoState.Processing;

            try
            {
                var cl = new WebClient();
                cl.Proxy = null;
                cl.Headers["UserAgent"] = "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US; rv:1.9.2.3) Gecko/20100401 Firefox/3.6.3 ( .NET CLR 3.5.30729; .NET4.0E)";
                _page = cl.DownloadString(new Uri("http://www.youtube.com/watch?v=" + VideoId));
            }
            catch (WebException ex)
            {
                Error = ex.Message;
                State = VideoState.Error;
                return;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(_page);

            var errorBox = doc.GetElementbyId("error-box");

            if (errorBox != null)
            {
                var elements =
                    errorBox.Descendants().Where(
                        n => n.Attributes.Contains("class") && n.Attributes["class"].Value.Contains("yt-alert-content"));
                if (elements.Count() > 0)
                {
                    Error = elements.First().InnerHtml;
                    State = VideoState.Error;
                }

                return;
            }

            if (_page.Contains("ServiceLoginAuth"))
            {
                Error = "Video is private";
                State = VideoState.Error;
                return;
            }

            var docTitle = doc.DocumentNode.Descendants("meta").Where(n => n.Attributes["name"].Value == "title");

            if (docTitle.Count() > 0)
            {
                Title = WebUtility.HtmlDecode(docTitle.First().Attributes["content"].Value);
            }

            Thumbnail = RealThumbnail;

            var vars = "name=\\\\\"flashvars\\\\\" value=\\\\\"(.*)\\\\\"";
            var match = Regex.Match(_page, vars, RegexOptions.Singleline);

            if (match.Success && match.Groups.Count >= 2)
            {
                var flashVarsString = match.Groups[1].Value;
                _flashVars = ParseVars(flashVarsString);
            }

            var fmtMapEncoded = HttpUtility.UrlDecode(_flashVars["fmt_url_map"]);
            _fmtMap = ParseFmtMap(fmtMapEncoded);

            foreach (var defaultQuality in VideoQualities.DefaultQualityList)
            {
                if (_fmtMap.ContainsKey(defaultQuality.Id))
                    AvailableQualities.Add(defaultQuality);
            }
            NotifyOfPropertyChange(() => AvailableQualities);

            if (!AvailableQualities.Contains(MaxQuality))
            {
                var defaultList = VideoQualities.DefaultQualityList;
                var index = defaultList.IndexOf(MaxQuality);

                DownloadQuality = null;

                for (int i = index + 1; i < defaultList.Count; i++)
                {
                    if (AvailableQualities.Contains(defaultList[i]))
                    {
                        DownloadQuality = defaultList[i];
                        break;
                    }
                }

                if (DownloadQuality == null) // Shouldn't actually get in here
                    DownloadQuality = AvailableQualities.Last();
            }
            else
            {
                DownloadQuality = MaxQuality;
            }

            _urlParams = null;
            _flashVars = null;
            _page = String.Empty;

            State = VideoState.Ready;
        }

        private IDictionary<int, string> ParseFmtMap(string paramsString)
        {
            if (string.IsNullOrEmpty(paramsString))
                throw new ArgumentNullException("paramsString");

            // convert to dictionary
            var dict = new Dictionary<int, string>();

            var formats = paramsString.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var format in formats)
            {
                var pos = format.IndexOf("|");

                if (pos == -1)
                    continue;

                var key = Int32.Parse(format.Substring(0, pos));
                var value = format.Substring(pos + 1);

                if (!dict.ContainsKey(key))
                    dict.Add(key, value);
            }

            return dict;
        }

        private IDictionary<string, string> ParseVars(string paramsString)
        {
            if (string.IsNullOrEmpty(paramsString))
                throw new ArgumentNullException("paramsString");

            // convert to dictionary
            var dict = new Dictionary<string, string>();

            var vars = paramsString.Split(new[] { "&" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var flashvar in vars)
            {
                var pos = flashvar.IndexOf("=");

                if (pos == -1)
                    continue;

                var key = flashvar.Substring(0, pos);
                var value = flashvar.Substring(pos + 1);

                if (!dict.ContainsKey(key))
                    dict.Add(key, value);
            }

            return dict;
        }

        private IDictionary<string, string> ParseParams(string paramsString)
        {
            if (string.IsNullOrEmpty(paramsString))
                throw new ArgumentNullException("paramsString");

            // convert to dictionary
            var dict = new Dictionary<string, string>();

            // remove the leading ?
            if (paramsString.StartsWith("?"))
                paramsString = paramsString.Substring(1);

            var length = paramsString.Length;

            for (var i = 0; i < length; i++)
            {
                var startIndex = i;
                var pivotIndex = -1;

                while (i < length)
                {
                    char ch = paramsString[i];
                    if (ch == '=')
                    {
                        if (pivotIndex < 0)
                        {
                            pivotIndex = i;
                        }
                    }
                    else if (ch == '&')
                    {
                        break;
                    }
                    i++;
                }

                string name;
                string value;
                if (pivotIndex >= 0)
                {
                    name = paramsString.Substring(startIndex, pivotIndex - startIndex);
                    value = paramsString.Substring(pivotIndex + 1, (i - pivotIndex) - 1);
                }
                else
                {
                    name = paramsString.Substring(startIndex, i - startIndex);
                    value = null;
                }

                var keyName = HttpUtility.UrlDecode(name);

                if (!dict.ContainsKey(keyName))
                    dict.Add(HttpUtility.UrlDecode(name), HttpUtility.UrlDecode(value));

                // if string ends with ampersand, add another empty token
                if ((i == (length - 1)) && (paramsString[i] == '&'))
                    dict.Add(null, string.Empty);
            }
            
            return dict;
        }
        #endregion

        public bool Equals(Video other)
        {
            return other.VideoId == this.VideoId;
        }
    }

    public class VideoQuality
    {
        public string Description { get; set; }
        public string Extension { get; set; }
        public int Id { get; set; }
    }

    public static class VideoQualities
    {
        /// <summary>
        /// List of supported qualities ordered highest to lowest
        /// </summary>
        public static List<VideoQuality> DefaultQualityList =
            new List<VideoQuality>
                {
                    new VideoQuality {Description = "1080p HD MP4", Id = 37, Extension = ".mp4"},
                    new VideoQuality {Description = "720p HD MP4", Id = 22, Extension = ".mp4"},
                    new VideoQuality {Description = "480p FLV", Id = 35, Extension = ".flv"},
                    new VideoQuality {Description = "360p FLV", Id = 34, Extension = ".flv"},
                    new VideoQuality {Description = "360p MP4", Id = 18, Extension = ".mp4"},
                    new VideoQuality {Description = "360p FLV (old)", Id = 6, Extension = ".flv"},
                    new VideoQuality {Description = "240p FLV", Id = 5, Extension = ".flv"},
                };
    }

    public enum VideoState
    {
        None,
        Processing,
        Ready,
        Downloading,
        Downloaded,
        Error,
        Cancelled
    }
}
