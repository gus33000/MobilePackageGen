using System.Xml.Serialization;

namespace CabSorter.UpdateHistory
{
    [XmlRoot(ElementName = "StoreId", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
    public class StoreId
    {
        [XmlElement(ElementName = "Guid", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public string Guid
        {
            get; set;
        }
    }
}