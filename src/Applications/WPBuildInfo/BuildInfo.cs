using System.Xml.Serialization;

namespace WPBuildInfo
{
    public class BuildInfo
    {
        [XmlRoot(ElementName = "build-information")]
        public class Buildinformation
        {
            [XmlElement(ElementName = "release-label")]
            public string Releaselabel
            {
                get; set;
            }
            [XmlElement(ElementName = "build-time")]
            public string Buildtime
            {
                get; set;
            }
            [XmlElement(ElementName = "parent-branch-build")]
            public string Parentbranchbuild
            {
                get; set;
            }
            [XmlElement(ElementName = "builder")]
            public string Builder
            {
                get; set;
            }
            [XmlElement(ElementName = "major-version")]
            public string Majorversion
            {
                get; set;
            }
            [XmlElement(ElementName = "minor-version")]
            public string Minorversion
            {
                get; set;
            }
            [XmlElement(ElementName = "qfe-level")]
            public string Qfelevel
            {
                get; set;
            }
            [XmlElement(ElementName = "build-type")]
            public string Buildtype
            {
                get; set;
            }
            [XmlElement(ElementName = "target-cpu")]
            public string Targetcpu
            {
                get; set;
            }
            [XmlElement(ElementName = "target-os")]
            public string Targetos
            {
                get; set;
            }
            [XmlElement(ElementName = "winphone-root")]
            public string Winphoneroot
            {
                get; set;
            }
            [XmlElement(ElementName = "ntrazzlemajorversion")]
            public string Ntrazzlemajorversion
            {
                get; set;
            }
            [XmlElement(ElementName = "ntrazzleminorversion")]
            public string Ntrazzleminorversion
            {
                get; set;
            }
            [XmlElement(ElementName = "ntrazzlebuildnumber")]
            public string Ntrazzlebuildnumber
            {
                get; set;
            }
            [XmlElement(ElementName = "ntrazzlerevisionnumber")]
            public string Ntrazzlerevisionnumber
            {
                get; set;
            }
        }
    }
}
