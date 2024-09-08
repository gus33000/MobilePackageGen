using System.Xml.Serialization;

namespace ToCBS.XmlMum
{
    [XmlRoot(ElementName = "component", Namespace = "urn:schemas-microsoft-com:asm.v3")]
    public class Component
    {
        [XmlElement(ElementName = "assemblyIdentity", Namespace = "urn:schemas-microsoft-com:asm.v3")]
        public AssemblyIdentity AssemblyIdentity
        {
            get; set;
        }
    }
}
