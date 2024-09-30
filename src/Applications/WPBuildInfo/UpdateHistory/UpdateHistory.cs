using System.Xml.Serialization;

namespace MobilePackageGen.UpdateHistory
{

    [XmlRoot(ElementName = "UpdateHistory", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
    public class UpdateHistory
    {
        [XmlElement(ElementName = "UpdateEvents", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public UpdateEvents UpdateEvents
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns
        {
            get; set;
        }
    }
}
