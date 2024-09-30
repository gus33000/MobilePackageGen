using System.Xml.Serialization;

namespace MobilePackageGen.XmlDsm
{
    [XmlRoot(ElementName = "Files", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
    public class Files
    {
        [XmlElement(ElementName = "FileEntry", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public List<FileEntry> FileEntry
        {
            get; set;
        }
    }
}
