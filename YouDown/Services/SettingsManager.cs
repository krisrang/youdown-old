using System;
using System.IO;
using System.Xml.Serialization;
using Lemon.Utils;
using MEFedMVVM.Common;
using MEFedMVVM.ViewModelLocator;

namespace YouDown.Services
{
    [ExportService(ServiceType.Both, typeof(SettingsManager))]
    public class SettingsManager : NotifyPropertyChangedBase
    {
        public bool Dirty { get; set; }
        public SettingsData Settings { get; set; }

        public SettingsManager()
        {
            Load();
        }

        public SettingsData DefaultSettings()
        {
            var defSettings = new SettingsData();
            defSettings.CheckUpdatesOnLaunch = true;
            defSettings.MaxQuality = 0;
            defSettings.UpdateLocation = "http://apps.kristjanrang.eu/youdown/";

            defSettings.TargetPath = DefaultTargetPath();

            return defSettings;
        }

        private string DefaultTargetPath()
        {
            string target;

            var vistaDownloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                                                     "Downloads");

            var xpDownloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                                              "Downloads");

            if (Features.VistaOrLater && Directory.Exists(vistaDownloadPath))
                target = vistaDownloadPath;
            else if (Directory.Exists(xpDownloadPath))
                target = xpDownloadPath;
            else
                target = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            return target;
        }

        public void Save()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(SettingsData));
                TextWriter textWriter = new StreamWriter(SettingsPath());
                serializer.Serialize(textWriter, Settings);
                textWriter.Close();
            }
            catch
            {
            }
        }

        private void Load()
        {
            if (File.Exists(SettingsPath()))
            {
                var serializer = new XmlSerializer(typeof (SettingsData));
                TextReader textReader = new StreamReader(SettingsPath());

                try
                {
                    Settings = (SettingsData) serializer.Deserialize(textReader);
                }
                catch
                {
                    Settings = DefaultSettings();
                }
                finally
                {
                    textReader.Close();
                }

                if (!Directory.Exists(Settings.TargetPath))
                {
                    Settings.TargetPath = DefaultTargetPath();
                    Save();
                }
            }

            if (Settings == null)
            {
                Settings = DefaultSettings();
            }
        }

        private static string SettingsPath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            return Path.Combine(appData, "YouDown.usersettings");
        }
    }
}
