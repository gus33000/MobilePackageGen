using System.Xml.Serialization;

namespace MobilePackageGen.UpdateHistory
{
    [XmlRoot(ElementName = "Version", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
    public class Version
    {
        [XmlAttribute(AttributeName = "Major")]
        public string Major
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "Minor")]
        public string Minor
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "QFE")]
        public string QFE
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "Build")]
        public string Build
        {
            get; set;
        }
    }
}