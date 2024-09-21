using System.Xml.Serialization;

namespace MobilePackageGen.XmlDsm
{
    [XmlRoot(ElementName = "FileEntry", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
    public class FileEntry
    {
        [XmlElement(ElementName = "FileType", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string FileType { get; set; }
        [XmlElement(ElementName = "DevicePath", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string DevicePath { get; set; }
        [XmlElement(ElementName = "CabPath", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string CabPath { get; set; }
        [XmlElement(ElementName = "Attributes", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string Attributes { get; set; }
        [XmlElement(ElementName = "SourcePackage", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string SourcePackage { get; set; }
        [XmlElement(ElementName = "FileSize", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string FileSize { get; set; }
        [XmlElement(ElementName = "CompressedFileSize", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string CompressedFileSize { get; set; }
        [XmlElement(ElementName = "StagedFileSize", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string StagedFileSize { get; set; }
    }
}
