using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lemon.Analytics;

namespace YouDown.Models
{
    public static class Analytics
    {
        public static string Domain = "youdown.apps.kristjanrang.eu";

        public static void AppStart()
        {
            var view = new GoogleView("AppStart", Domain, "start");
            GoogleTracking.TrackView(view);
        }

        public static void DownloadStart(int count)
        {
            var evnt = new GoogleEvent(Domain, "Download", "Start", "Queue", count);
            GoogleTracking.TrackEvent(evnt);
        }

        public static void DownloadFinish(int count)
        {
            var evnt = new GoogleEvent(Domain, "Download", "Finish", "Queue", count);
            GoogleTracking.TrackEvent(evnt);
        }
    }
}
