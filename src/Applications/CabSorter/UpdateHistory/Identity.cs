using System.Xml.Serialization;

namespace CabSorter.UpdateHistory
{
    [XmlRoot(ElementName = "Identity", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
    public class Identity
    {
        [XmlElement(ElementName = "Owner", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string Owner
        {
            get; set;
        }
        [XmlElement(ElementName = "Component", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string Component
        {
            get; set;
        }
        [XmlElement(ElementName = "Version", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public Version Version
        {
            get; set;
        }
        [XmlElement(ElementName = "SubComponent", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string SubComponent
        {
            get; set;
        }
    }
}