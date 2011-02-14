using System.Xml.Serialization;
using MEFedMVVM.Common;

namespace YouDown.Services
{
    [XmlRoot("Settings")]
    public class SettingsData : NotifyPropertyChangedBase
    {
        public bool CheckUpdatesOnLaunch { get; set; }
        public string TargetPath { get; set; }
        public int MaxQuality { get; set; }
        public string UpdateLocation { get; set; }

        public SettingsData Clone()
        {
            return (SettingsData)MemberwiseClone();
        }
    }
}
