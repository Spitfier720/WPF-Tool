using EasyMockLib;
using EasyMockLib.Models;

using CommandLine;
using System.Xml.Serialization;
using CommandLine.Text;

namespace MockTxtToXmlConverter
{
    public class Options
    {
        [Option('i', "input", Required = true, HelpText = "Input file path.")]
        public string InputFilePath { get; set; }

        [Option('o', "output", Required = false, HelpText = "Output file path.")]
        public string OutputFilePath { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options => {
                    string txtFolder = options.InputFilePath;
                    string xmlFolder = options.OutputFilePath;
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
                })
                .WithNotParsed(errors =>
                {
                    foreach(var error in errors)
                    {
                        if(error is HelpRequestedError || error is VersionRequestedError)
                        {
                            Console.WriteLine("Use -i for input file and -o for output file.");
                        }
                        else
                        {
                            Console.Error.WriteLine($"Error: {error.Tag} - {error.ToString()}");
                        }   
                    }
                });
        }
    }
}