using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Nebula.SourceGeneration
{
    [Generator]
    public class OpcodeEnumGenerator
        : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var opcodeSourceFile = context.AdditionalTextsProvider.Where(file => Path.GetFileName(file.Path) == "Opcodes.xml");
            var fileContents = opcodeSourceFile.Select((text, cancellationToken) => text.GetText(cancellationToken)?.ToString());
            var provider = fileContents.Collect();

            context.RegisterSourceOutput(provider, (spc, opcodeDefinitionFiles) =>
            {
                int counter = 0;
                foreach (var content in opcodeDefinitionFiles)
                {
                    XDocument opcodeSource = XDocument.Parse(content);
                    StringWriter streamWriter = new StringWriter();
                    IndentedTextWriter writer = new IndentedTextWriter(streamWriter);

                    writer.WriteLine("namespace Nebula.Shared.Enumerators");
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


                    string contents = streamWriter.ToString();
                    var sourceText = SourceText.From(contents, Encoding.UTF8);
                    spc.AddSource($"InstructionOpcode_{counter++}.g.cs", sourceText);
                }
            });
        }

        private static void WriteOpcodeValues(XDocument opcodeSource, IndentedTextWriter writer)
        {
            XElement opcodeList = opcodeSource.Element("Opcodes");
            foreach (var opcode in opcodeList.Elements())
            {
                string opcodeName = opcode.Attribute("Type").Value;
                string description = string.Empty;
                var descriptionElement = opcode.Element("Description");
                if (descriptionElement != null)
                {
                    description = descriptionElement.Attribute("Text")?.Value ?? string.Empty;
                    description.Trim();
                }

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
            string[] descriptionLines = CalculateDescriptionLines(description);

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

        private static string[] CalculateDescriptionLines(string description)
        {
            const int maxLength = 120;
            if (string.IsNullOrWhiteSpace(description))
                return Array.Empty<string>();

            var result = new List<string>();

            // Split into sentences (keeps punctuation)
            var sentences = Regex.Split(description, @"(?<=[.!?])\s+");

            foreach (var sentence in sentences)
            {
                if (sentence.Length <= maxLength)
                {
                    result.Add(sentence.Trim());
                }
                else
                {
                    // Split long sentences by words
                    var words = sentence.Split(' ');
                    var current = "";

                    foreach (var word in words)
                    {
                        if ((current + word).Length > maxLength)
                        {
                            result.Add(current.Trim());
                            current = word + " ";
                        }
                        else
                        {
                            current += word + " ";
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(current))
                        result.Add(current.Trim());
                }
            }

            return result.ToArray();
        }
    }
}
