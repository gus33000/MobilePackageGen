using System.Xml.Serialization;

namespace MobilePackageGen.UpdateHistory
{
    [XmlRoot(ElementName = "UpdateEvent", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
    public class UpdateEvent
    {
        [XmlElement(ElementName = "Sequence", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string Sequence
        {
            get; set;
        }
        [XmlElement(ElementName = "DateTime", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string DateTime
        {
            get; set;
        }
        [XmlElement(ElementName = "UpdateOSOutput", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public UpdateOSOutput UpdateOSOutput
        {
            get; set;
        }
    }
}