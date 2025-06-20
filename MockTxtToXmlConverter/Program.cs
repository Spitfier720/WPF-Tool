using EasyMockLib;
using EasyMockLib.Models;
using System.Xml.Serialization;

namespace MockTxtToXmlConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            string txtFolder = @".\TestMockFiles";
            string xmlFolder = @".\TestMockFiles";
            var parser = new MockFileParser();

            foreach (var txtFile in Directory.EnumerateFiles(txtFolder, "*.txt"))
            {
                var mockNodes = parser.Parse(txtFile);
                string xmlFile = Path.Combine(xmlFolder, Path.GetFileNameWithoutExtension(txtFile) + ".xml");

                var serializer = new XmlSerializer(typeof(List<MockNode>));
                using (var writer = new StreamWriter(xmlFile))
                {
                    serializer.Serialize(writer, mockNodes.Nodes);
                }

                Console.WriteLine($"Converted {txtFile} to {xmlFile}");
            }
        }
    }
}