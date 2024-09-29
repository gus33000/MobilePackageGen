using System.Xml.Serialization;

namespace MobilePackageGen.UpdateHistory
{
    [XmlRoot(ElementName = "Package", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
    public class Package
    {
        [XmlElement(ElementName = "Identity", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public Identity Identity
        {
            get; set;
        }
        [XmlElement(ElementName = "ReleaseType", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string ReleaseType
        {
            get; set;
        }
        [XmlElement(ElementName = "OwnerType", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string OwnerType
        {
            get; set;
        }
        [XmlElement(ElementName = "BuildType", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string BuildType
        {
            get; set;
        }
        [XmlElement(ElementName = "CpuType", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string CpuType
        {
            get; set;
        }
        [XmlElement(ElementName = "Partition", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string Partition
        {
            get; set;
        }
        [XmlElement(ElementName = "InstalledSize", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string InstalledSize
        {
            get; set;
        }
        [XmlElement(ElementName = "IsRemoval", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string IsRemoval
        {
            get; set;
        }
        [XmlElement(ElementName = "IsBinaryPartition", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string IsBinaryPartition
        {
            get; set;
        }
        [XmlElement(ElementName = "PackageFile", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string PackageFile
        {
            get; set;
        }
        [XmlElement(ElementName = "DriverPackageIdentity", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string DriverPackageIdentity
        {
            get; set;
        }
        [XmlElement(ElementName = "PackageType", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string PackageType
        {
            get; set;
        }
        [XmlElement(ElementName = "PackageIdentity", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string PackageIdentity
        {
            get; set;
        }
        [XmlElement(ElementName = "GroupingKey", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string GroupingKey
        {
            get; set;
        }
        [XmlElement(ElementName = "Culture", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string Culture
        {
            get; set;
        }
        [XmlElement(ElementName = "IsSelfUpdate", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string IsSelfUpdate
        {
            get; set;
        }
        [XmlElement(ElementName = "Result", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string Result
        {
            get; set;
        }
        [XmlElement(ElementName = "Platform", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string Platform
        {
            get; set;
        }
        [XmlElement(ElementName = "Resolution", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string Resolution
        {
            get; set;
        }

        public override string ToString()
        {
            return Path.GetFileName(PackageFile);
        }
    }
}