// Copyright (c) 2018, Gustave M. - gus33000.me - @gus33000
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System.Xml.Serialization;

namespace CbsToReg.Mum
{
    [XmlRoot(ElementName = "package", Namespace = "urn:schemas-microsoft-com:asm.v3")]
    public class Package
    {
        [XmlElement(ElementName = "customInformation", Namespace = "urn:schemas-microsoft-com:asm.v3")]
        public CustomInformation CustomInformation
        {
            get; set;
        }
        [XmlElement(ElementName = "update", Namespace = "urn:schemas-microsoft-com:asm.v3")]
        public Update Update
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "identifier")]
        public string Identifier
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "releaseType")]
        public string ReleaseType
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "restart")]
        public string Restart
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "targetPartition")]
        public string TargetPartition
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "binaryPartition")]
        public string BinaryPartition
        {
            get; set;
        }
    }
}