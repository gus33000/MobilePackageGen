using System.Xml.Serialization;

namespace CabSorter.UpdateHistory
{
    [XmlRoot(ElementName = "UpdateEvents", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
    public class UpdateEvents
    {
        [XmlElement(ElementName = "UpdateEvent", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
        public UpdateEvent[] UpdateEvent
        {
            get; set;
        }
    }
}