using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ToSPKG
{
    public static class XmlDsm
    {
        [XmlRoot(ElementName = "Version", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class Version
        {
            [XmlAttribute(AttributeName = "Major")]
            public string Major { get; set; }
            [XmlAttribute(AttributeName = "Minor")]
            public string Minor { get; set; }
            [XmlAttribute(AttributeName = "QFE")]
            public string QFE { get; set; }
            [XmlAttribute(AttributeName = "Build")]
            public string Build { get; set; }
        }

        [XmlRoot(ElementName = "Identity", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class Identity
        {
            [XmlElement(ElementName = "Owner", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public string Owner { get; set; }
            [XmlElement(ElementName = "Component", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public string Component { get; set; }
            [XmlElement(ElementName = "SubComponent", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public string SubComponent { get; set; }
            [XmlElement(ElementName = "Version", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public Version Version { get; set; }
        }

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

        [XmlRoot(ElementName = "Files", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public class Files
        {
            [XmlElement(ElementName = "FileEntry", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
            public List<FileEntry> FileEntry { get; set; }
        }

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
}
