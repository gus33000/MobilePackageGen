using System.Xml.Serialization;

namespace MobilePackageGen.UpdateHistory
{
    [XmlRoot(ElementName = "Id", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
    public class Id
    {
        [XmlElement(ElementName = "Guid", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string Guid
        {
            get; set;
        }
    }
}