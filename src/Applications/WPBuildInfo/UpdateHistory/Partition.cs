using System.Xml.Serialization;

namespace MobilePackageGen.UpdateHistory
{
    [XmlRoot(ElementName = "Partition", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
    public class Partition
    {
        [XmlElement(ElementName = "Style", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string Style
        {
            get; set;
        }
        [XmlElement(ElementName = "Offset", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string Offset
        {
            get; set;
        }
        [XmlElement(ElementName = "Type", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public Type Type
        {
            get; set;
        }
        [XmlElement(ElementName = "Id", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public Id Id
        {
            get; set;
        }
        [XmlElement(ElementName = "Name", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string Name
        {
            get; set;
        }
    }
}