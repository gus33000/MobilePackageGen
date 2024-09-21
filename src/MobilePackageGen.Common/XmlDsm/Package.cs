using System.Xml.Serialization;

namespace MobilePackageGen.XmlDsm
{
    [XmlRoot(ElementName = "Package", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
    public class Package
    {
        [XmlElement(ElementName = "Identity", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public Identity Identity { get; set; }
        [XmlElement(ElementName = "ReleaseType", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string ReleaseType { get; set; }
        [XmlElement(ElementName = "OwnerType", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string OwnerType { get; set; }
        [XmlElement(ElementName = "BuildType", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string BuildType { get; set; }
        [XmlElement(ElementName = "CpuType", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string CpuType { get; set; }
        [XmlElement(ElementName = "Culture", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string Culture { get; set; }
        [XmlElement(ElementName = "Partition", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string Partition { get; set; }
        [XmlElement(ElementName = "GroupingKey", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string GroupingKey { get; set; }
        [XmlElement(ElementName = "IsRemoval", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string IsRemoval { get; set; }
        [XmlElement(ElementName = "Files", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public Files Files { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }
}
