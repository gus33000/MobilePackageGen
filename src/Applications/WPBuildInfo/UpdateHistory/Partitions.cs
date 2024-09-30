using System.Xml.Serialization;

namespace MobilePackageGen.UpdateHistory
{
    [XmlRoot(ElementName = "Partitions", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
    public class Partitions
    {
        [XmlElement(ElementName = "Partition", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public Partition Partition
        {
            get; set;
        }
    }
}