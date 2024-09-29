using System.Xml.Serialization;

namespace MobilePackageGen.UpdateHistory
{
    [XmlRoot(ElementName = "Type", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
    public class Type
    {
        [XmlElement(ElementName = "Guid", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string Guid
        {
            get; set;
        }
    }
}