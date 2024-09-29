using System.Xml.Serialization;

namespace MobilePackageGen.UpdateHistory
{
    [XmlRoot(ElementName = "Disks", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
    public class Disks
    {
        [XmlElement(ElementName = "Disk", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public List<Disk> Disk
        {
            get; set;
        }
    }
}