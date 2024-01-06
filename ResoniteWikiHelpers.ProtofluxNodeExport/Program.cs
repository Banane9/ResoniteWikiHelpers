using Elements.Core;
using FrooxEngine;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution.Nodes.Actions;
using ProtoFlux.Runtimes.Execution.Nodes.Operators;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ResoniteWikiHelpers.ProtofluxNodeExport
{
    internal class Program
    {
        private static readonly string _allNodesNotice =
@"<!-----------------------------------------------------------------------+
 ! This file has been generated using a script. Do not edit it manually. !
 +----------------------------------------------------------------------->

[All Nodes Header](./README_prefix.md ':include')
";

        private static readonly string _includeTemplate = "[Category {0}](./{1}/README.md ':include')";
        private static readonly string[] _nodeAssemblies = new[] { "FrooxEngine", "ProtoFlux.Core", "ProtoFlux.Nodes.Core", "ProtoFlux.Nodes.FrooxEngine" };

        private static readonly Type _nodeType = typeof(INode);

        private static readonly string _notice =
@"<!-----------------------------------------------------------------------+
 ! This file has been generated using a script. Do not edit it manually. !
 ! Edit the individual node pages instead.                               !
 +----------------------------------------------------------------------->

## {0}

";

        private static readonly Regex _rtfRegex = new(@"</?.*?/?>");

        private static readonly string _tableFooterTemplate =
@"| {0} | {1} |  |
<!-- ProtofluxNode:end -->

";

        private static readonly string _tableHeaderTemplate =
@"### {0}

<!-- ProtofluxNode:start -->
| {0} | Type | Label |
| --- | ---- | ----- |";

        private static readonly string _tableRowTemplate = "| {0} | {1} | {2} |";

        private static string FormatRow(string element, string type, string name)
            => string.Format(_tableRowTemplate, element, type.Replace('`', '_'), name.Replace("|", "\\|"))
                .Replace("`", "\\`");

        private static string GetOperationName(bool isSync, bool isAsync)
            => isSync ?
                (isAsync ? "MixedOperation" : "SyncOperation")
                : (isAsync ? "AsyncOperation" : "NoneOperation");

        private static void Main(string[] args)
        {
            if (Directory.Exists("export"))
                Directory.Delete("export", true);

            Directory.CreateDirectory("export");

            using var allNodesFile = new FileStream(Path.Combine("export", "README.md"), FileMode.OpenOrCreate);
            using var allNodesWriter = new StreamWriter(allNodesFile);

            allNodesWriter.WriteLine(_allNodesNotice);

            var nodeCategories = _nodeAssemblies.Select(name => Assembly.LoadFrom($"./{name}.dll"))
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && type.IsPublic && type.IsAssignableTo(_nodeType))
                .Select(nodeType =>
                {
                    var metadata = NodeMetadataHelper.GetMetadata(nodeType);
                    var path = NullConcat("Root/", nodeType.GetCustomAttributes<NodeCategoryAttribute>(true).FirstOrDefault()?.Name) ?? "Root";
                    var name = StringHelper.BeautifyName(metadata.Name ?? Path.GetExtension(metadata.Overload) ?? nodeType.Name);

                    return (Type: nodeType, Metadata: metadata, Path: path, Name: name);
                })
                .Where(node => node.Path is not null)
                .GroupBy(node => node.Metadata.Overload ?? node.Name)
                .Select(nodeGroup => nodeGroup.First()) // Select by something else?
                .GroupBy(node => node.Path)
                .OrderBy(nodeCategory => nodeCategory.Key);

            foreach (var nodeCategory in nodeCategories)
            {
                var path = Path.Combine("export", nodeCategory.Key);
                Directory.CreateDirectory(path);

                allNodesWriter.WriteLine(_includeTemplate, nodeCategory.Key, nodeCategory.Key.Replace(" ", "%20"));
                allNodesWriter.WriteLine();

                using var outputFile = new FileStream(Path.Combine(path, "README.md"), FileMode.OpenOrCreate);
                using var writer = new StreamWriter(outputFile);

                writer.Write(_notice, nodeCategory.Key);

                foreach (var node in nodeCategory.OrderBy(node => node.Metadata.Overload ?? node.Name))
                {
                    writer.WriteLine(string.Format(_tableHeaderTemplate, _rtfRegex.Replace(node.Name.Replace("`", "\\`").Replace("|", "\\|"), "")));

                    var inputElements = node.Metadata.FixedOperations
                        .Concat<IElementMetadata>(node.Metadata.DynamicOperations)
                        .Concat(node.Metadata.FixedInputs)
                        .Concat(node.Metadata.DynamicInputs);

                    var outputElements = node.Metadata.FixedImpulses
                        .Concat<IElementMetadata>(node.Metadata.DynamicImpulses)
                        .Concat(node.Metadata.FixedOutputs)
                        .Concat(node.Metadata.DynamicOutputs);

                    foreach (var element in inputElements.Interleave(outputElements))
                    {
                        switch (element)
                        {
                            case OperationMetadata operation:
                                writer.WriteLine(FormatRow("input", GetOperationName(!operation.IsAsync, operation.IsAsync), operation.Name));
                                break;

                            case OperationListMetadata operationList:
                                writer.WriteLine(FormatRow("inputlist", GetOperationName(operationList.SupportsSync, operationList.SupportsAsync), operationList.Name));
                                break;

                            case InputMetadata input:
                                writer.WriteLine(FormatRow("input", input.Field.FieldType.IsConstructedGenericType ? input.Field.FieldType.GenericTypeArguments[0].Name : input.Field.FieldType.Name, input.Name));
                                break;

                            case InputListMetadata input:
                                writer.WriteLine(FormatRow("inputlist", input.Field.FieldType.IsConstructedGenericType ? input.Field.FieldType.GenericTypeArguments[0].Name : input.Field.FieldType.Name, input.Name));
                                break;

                            case ImpulseMetadata impulse:
                                writer.WriteLine(FormatRow("output", impulse.Type.ToString(), impulse.Name));
                                break;

                            case ImpulseListMetadata impulseList:
                                writer.WriteLine(FormatRow("outputlist", impulseList.Type?.ToString() ?? "null", impulseList.Name));
                                break;

                            case OutputMetadata output:
                                writer.WriteLine(FormatRow("output", output.OutputType.Name, output.Name));
                                break;

                            case OutputListMetadata outputList:
                                writer.WriteLine(FormatRow("outputlist", outputList.TypeConstraint?.Name ?? "*", outputList.Name));
                                break;

                            default:
                                Console.WriteLine(element);
                                break;
                        }
                    }

                    writer.WriteLine(string.Format(_tableFooterTemplate, node.Path, node.Type.FullName!.Replace("`", "\\`")));
                }

                writer.Flush();
                writer.Close();
            }
        }

        private static string? NullConcat(string? a, string? b)
                    => a is null || b is null ? null : a + b;
    }
}