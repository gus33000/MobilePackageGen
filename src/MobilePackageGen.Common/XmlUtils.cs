using System.Xml.Serialization;

namespace MobilePackageGen
{
    internal static class XmlUtils
    {
        internal static T GetObjectFromXML<T>(this Stream XMLStringStream)
        {
            XmlSerializer serializer = new(typeof(T));
            return (T)serializer.Deserialize(XMLStringStream)!;
        }
    }
}