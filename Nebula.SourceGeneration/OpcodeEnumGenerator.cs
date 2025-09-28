using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Nebula.SourceGeneration.Properties;
using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Xml.Linq;

namespace Nebula.SourceGeneration
{
    [Generator]
    public class OpcodeEnumGenerator
        : IIncrementalGenerator
    {
        private static readonly Assembly s_sourceAssembly = Assembly.GetAssembly(typeof(OpcodeEnumGenerator));

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterSourceOutput(context.CompilationProvider, (spc, compilation) =>
            {
                XDocument opcodeSource = XDocument.Parse(Resources.Opcodes);

                StringWriter fileContents = new StringWriter();
                IndentedTextWriter writer = new IndentedTextWriter(fileContents);

                writer.WriteLine("namespace Nebula.Interop.Enumerators");
                writer.WriteLine('{');
                writer.Indent++;

                writer.WriteLine("public enum InstructionOpcode");
                writer.WriteLine("{");
                writer.Indent++;

                WriteOpcodeValues(opcodeSource, writer);

                writer.Indent--;
                writer.WriteLine("}");
                writer.Indent--;
                writer.WriteLine("}");


                string contents = fileContents.ToString();
                var sourceText = SourceText.From(contents, Encoding.UTF8);
                spc.AddSource("InstructionOpcode.g.cs", sourceText);
            });
        }

        private static void WriteOpcodeValues(XDocument opcodeSource, IndentedTextWriter writer)
        {
            XElement opcodeList = opcodeSource.Element("Opcodes");
            foreach (var opcode in opcodeList.Elements())
            {
                string opcodeName = opcode.Attribute("Type").Value;
                string description = (opcode.Element("Description").Value ?? string.Empty).Trim();

                if (!string.IsNullOrEmpty(description))
                {
                    WriteSummaryComment(writer, description);
                }

                writer.Write(opcodeName);
                writer.WriteLine(',');
            }
        }

        private static void WriteSummaryComment(IndentedTextWriter writer, string description)
        {
            string[] descriptionLines = description.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            if (descriptionLines.Length == 1)
            {
                writer.Write("///<summary>");
                writer.Write($" {description} ");
                writer.WriteLine("</summary>");
                return;
            }

            writer.WriteLine("///<summary>");
            foreach (var line in descriptionLines)
            {
                writer.Write("/// ");
                writer.WriteLine(line);
            }

            writer.WriteLine("///</summary>");
        }
    }
}
