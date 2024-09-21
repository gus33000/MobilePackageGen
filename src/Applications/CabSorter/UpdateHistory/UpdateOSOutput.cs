using System.Xml.Serialization;

namespace CabSorter.UpdateHistory
{
    [XmlRoot(ElementName = "UpdateOSOutput", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
    public class UpdateOSOutput
    {
        [XmlElement(ElementName = "Description", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string Description
        {
            get; set;
        }
        [XmlElement(ElementName = "OverallResult", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string OverallResult
        {
            get; set;
        }
        [XmlElement(ElementName = "UpdateState", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string UpdateState
        {
            get; set;
        }
        [XmlElement(ElementName = "Packages", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public Packages Packages
        {
            get; set;
        }
        [XmlElement(ElementName = "Disks", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public Disks Disks
        {
            get; set;
        }
    }
}