using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;

namespace Nebula.SourceGeneration
{
    [Generator]
    public class ReportMessagesGenerator
        : IIncrementalGenerator
    {
        public const string EnumeratorNamespace = "Nebula.Shared.Enumerators";
        public const string ProviderNamespace = "Nebula.Commons.Reporting.Strings";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var opcodeSourceFile = context.AdditionalTextsProvider.Where(file => Path.GetExtension(file.Path) == ".resx");
            var fileContents = opcodeSourceFile.Select((text, cancellationToken) => (Path.GetFileNameWithoutExtension(text.Path), text.GetText(cancellationToken)?.ToString()));
            var provider = fileContents.Collect();

            context.RegisterSourceOutput(provider, (spc, files) =>
            {
                foreach ((string fileName, string content) in files)
                {
                    XDocument xmlSource = XDocument.Parse(content);
                    var root = xmlSource.Element("root");

                    var entries = root.Elements("data");
                    WriteEnumClass(spc, fileName, entries, out var enumName);
                    WriteProviderClass(spc, fileName, enumName, entries);
                }
            });
        }

        private void WriteProviderClass(SourceProductionContext spc, string fileName, string enumName, IEnumerable<XElement> entries)
        {
            StringWriter writer = new StringWriter();
            IndentedTextWriter indentedWriter = new IndentedTextWriter(writer);

            indentedWriter.WriteLine($"using {EnumeratorNamespace};");
            indentedWriter.WriteLine($"using Nebula.Commons.Reporting.Strings;");
            indentedWriter.WriteLine();

            OpenNamespace(indentedWriter, ProviderNamespace);
            OpenClassScope(indentedWriter, $"{fileName}Provider", isStatic: true);

            int counter = 0;
            foreach (var dataPoint in entries)
            {
                var name = dataPoint.Attribute("name");
                var comment = dataPoint.Element("comment")?.Value ?? string.Empty;
                string valueName = BuildEnumValueName(fileName, counter++, comment);
                indentedWriter.WriteLine($"public static ({enumName} Code, string MessageTemplate) {name.Value} = ({enumName}.{valueName}, {fileName}.{name.Value});");
            }

            CloseScope(indentedWriter);
            CloseScope(indentedWriter);


            string providerContents = writer.ToString();
            SourceText sourceText = SourceText.From(providerContents, Encoding.UTF8);
            spc.AddSource($"{fileName}.Provider.g.cs", sourceText);
        }

        private void WriteEnumClass(SourceProductionContext spc, string fileName, IEnumerable<XElement> entries, out string enumName)
        {
            StringWriter writer = new StringWriter();
            IndentedTextWriter indentedWriter = new IndentedTextWriter(writer);

            enumName = $"E{fileName}";

            OpenNamespace(indentedWriter, EnumeratorNamespace);
            OpenEnumScope(indentedWriter, enumName);

            long counter = 0;
            foreach (var dataPoint in entries)
            {
                var name = dataPoint.Attribute("name");
                var comment = dataPoint.Element("comment")?.Value ?? string.Empty;
                string valueName = BuildEnumValueName(fileName, counter, comment);
                indentedWriter.WriteLine($"{valueName} = {counter++},");
            }

            CloseScope(indentedWriter);
            CloseScope(indentedWriter);

            string enumContents = writer.ToString();
            SourceText sourceText = SourceText.From(enumContents, Encoding.UTF8);
            spc.AddSource($"{fileName}.Enumerator.g.cs", sourceText);
        }

        private void OpenClassScope(IndentedTextWriter indentedWriter, string className, bool isStatic)
        {
            if (isStatic)
            {
                indentedWriter.WriteLine($"public static class {className}");
            }
            else
            {
                indentedWriter.WriteLine($"public class {className}");
            }

            indentedWriter.WriteLine("{");
            indentedWriter.Indent++;
        }

        private void OpenEnumScope(IndentedTextWriter indentedWriter, string enumName)
        {
            indentedWriter.WriteLine($"public enum {enumName}");
            indentedWriter.WriteLine("{");
            indentedWriter.Indent++;
        }

        private static string BuildEnumValueName(string fileName, long counter, string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return $"{fileName[0]}{counter:x4}".ToUpper();
            }

            return $"{fileName[0]}{prefix}{counter:x4}".ToUpper();
        }

        private static void CloseScope(IndentedTextWriter indentedEnumwriter)
        {
            indentedEnumwriter.Indent--;
            indentedEnumwriter.WriteLine("}");
        }

        private static void OpenNamespace(IndentedTextWriter writer, string ns)
        {
            writer.WriteLine($"namespace {ns}");
            writer.WriteLine('{');
            writer.Indent++;
        }
    }
}
