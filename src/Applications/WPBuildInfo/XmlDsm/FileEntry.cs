using System.Xml.Serialization;

namespace MobilePackageGen.XmlDsm
{
    [XmlRoot(ElementName = "FileEntry", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
    public class FileEntry
    {
        // Late Only
        [XmlElement(ElementName = "FileType", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string FileType
        {
            get; set;
        }
        [XmlElement(ElementName = "DevicePath", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string DevicePath
        {
            get; set;
        }
        [XmlElement(ElementName = "CabPath", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string CabPath
        {
            get; set;
        }
        [XmlElement(ElementName = "Attributes", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string Attributes
        {
            get; set;
        }
        // Late Only
        [XmlElement(ElementName = "SourcePackage", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string SourcePackage
        {
            get; set;
        }
        // Late Only
        [XmlElement(ElementName = "FileSize", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string FileSize
        {
            get; set;
        }
        // Late Only
        [XmlElement(ElementName = "CompressedFileSize", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string CompressedFileSize
        {
            get; set;
        }
        // Late Only
        [XmlElement(ElementName = "StagedFileSize", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string StagedFileSize
        {
            get; set;
        }

        // Early Only
        [XmlElement(ElementName = "Hash", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string Hash
        {
            get; set;
        }
        // Early Only
        [XmlElement(ElementName = "Type", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string Type
        {
            get; set;
        }
    }
}
