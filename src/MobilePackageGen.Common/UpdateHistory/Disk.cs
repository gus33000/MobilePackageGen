using System.Xml.Serialization;

namespace MobilePackageGen.UpdateHistory
{
    [XmlRoot(ElementName = "Disk", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
    public class Disk
    {
        [XmlElement(ElementName = "StoreId", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public StoreId StoreId
        {
            get; set;
        }
        [XmlElement(ElementName = "Partitions", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public Partitions Partitions
        {
            get; set;
        }
    }
}