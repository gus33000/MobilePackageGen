using System.Xml.Serialization;

namespace CabSorter.UpdateHistory
{
    [XmlRoot(ElementName = "Packages", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
    public class Packages
    {
        [XmlElement(ElementName = "Package", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public List<Package> Package
        {
            get; set;
        }
    }
}