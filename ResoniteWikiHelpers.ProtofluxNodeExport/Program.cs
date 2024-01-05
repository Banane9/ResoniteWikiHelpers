using Elements.Core;
using FrooxEngine;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution.Nodes.Actions;
using ProtoFlux.Runtimes.Execution.Nodes.Operators;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ResoniteWikiHelpers.ProtofluxNodeExport
{
    internal class Program
    {
        private static readonly string[] _nodeAssemblies = new[] { "FrooxEngine", "ProtoFlux.Core", "ProtoFlux.Nodes.Core", "ProtoFlux.Nodes.FrooxEngine" };

        private static readonly Type _nodeType = typeof(INode);

        private static readonly string _notice =
@"<!---------------------------------------------------------------------!
 ! This file has been generated using a script. Do not edit it manually. !
 ! Edit the protoflux.css file instead.                                  !
 !-----------------------------------------------------------------------!
";

        private static readonly Regex _rtfRegex = new(@"</?.*?/?>");

        private static readonly string _tableFooterTemplate =
@"| {0} |  |  |
<!-- ProtofluxNode:end -->

";

        private static readonly string _tableHeaderTemplate =
@"<!-- panels:start -->
<!-- div:title-panel -->
### {0}
<!-- div:left-panel -->

<!-- div:right-panel -->
<!-- ProtofluxNode:start -->
| {0} | Type | Label |
| --- | ---- | ----- |";

        private static readonly string _tableRowTemplate = "| {0} | {1} | {2} |";

        private static string FormatRow(bool input, string type, string name)
            => string.Format(_tableRowTemplate, input ? "input" : "output", type, name.Replace("|", "\\|"))
                .Replace("`", "\\`");

        private static string GetAsyncOp(bool isAsync)
            => isAsync ? "AsyncOperation" : "SyncOperation";

        private static void Main(string[] args)
        {
            using var outputFile = new FileStream("ProtofluxNodes.md", FileMode.OpenOrCreate);
            outputFile.SetLength(0);

            using var writer = new StreamWriter(outputFile);

            writer.Write(_notice);

            var nodes = _nodeAssemblies.Select(name => Assembly.LoadFrom($"./{name}.dll"))
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && type.IsPublic && type.IsAssignableTo(_nodeType))
                .Select(nodeType => (Type: nodeType, Metadata: NodeMetadataHelper.GetMetadata(nodeType), Path: nodeType.GetCustomAttributes<NodeCategoryAttribute>(false).FirstOrDefault()?.Name))
                .Where(node => node.Path is not null && node.Metadata.Name is not null)
                .DistinctBy(node => string.IsNullOrWhiteSpace(node.Metadata.Overload) ? node.Type.FullName : node.Metadata.Overload)
                .OrderBy(node => node.Metadata.Name);

            foreach (var node in nodes)
            {
                writer.WriteLine(string.Format(_tableHeaderTemplate, _rtfRegex.Replace(node.Metadata.Name.Replace("|", "\\|"), "")));

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
                            writer.WriteLine(FormatRow(true, GetAsyncOp(operation.IsAsync), operation.Name));
                            break;

                        case OperationListMetadata operationList:
                            writer.WriteLine(FormatRow(true, GetAsyncOp(operationList.SupportsAsync), operationList.Name));
                            break;

                        case InputMetadataBase input:
                            writer.WriteLine(FormatRow(true, input.Field.FieldType.IsConstructedGenericType ? input.Field.FieldType.GenericTypeArguments[0].Name : input.Field.FieldType.Name, input.Name));
                            break;

                        case ImpulseMetadata impulse:
                            writer.WriteLine(FormatRow(false, impulse.Type.ToString(), impulse.Name));
                            break;

                        case ImpulseListMetadata impulseList:
                            writer.WriteLine(FormatRow(false, impulseList.Type?.ToString() ?? "null", impulseList.Name));
                            break;

                        case OutputMetadata output:
                            writer.WriteLine(FormatRow(false, output.OutputType.Name, output.Name));
                            break;

                        case OutputListMetadata outputList:
                            writer.WriteLine(FormatRow(false, outputList.TypeConstraint?.Name ?? "*", outputList.Name));
                            break;

                        default:
                            Console.WriteLine(element);
                            break;
                    }
                }

                writer.WriteLine(string.Format(_tableFooterTemplate, Path.GetFileName(node.Path) ?? ""));
            }

            writer.Flush();
            writer.Close();
        }
    }
}