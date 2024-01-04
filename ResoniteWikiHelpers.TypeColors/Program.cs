using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;

namespace ResoniteWikiHelpers.TypeColors
{
    internal class Program
    {
        private static readonly string _cssTemplate =
@".PFN-{0} {{
  stroke: rgb({1});
  fill: rgb({1}, 0.2);
  background-image: linear-gradient(rgba({1}, 0.3), rgba({1}, 0.3));
}}

";

        private static readonly string _notice =
@"/*************************************************************************
 * This file has been generated using a script. Do not edit it manually. *
 * Edit the protoflux.css file instead.                                  *
 *************************************************************************/

";

        private static string ColorXToString(colorX color)
        {
            var rgb = MathX.RoundToInt(color.rgb.Normalized * 255);
            return $"{rgb.x}, {rgb.y}, {rgb.z}";
        }

        private static void Main(string[] args)
        {
            var types = GenericTypesAttribute.GetTypes(GenericTypesAttribute.Group.EnginePrimitivesAndEnums);
            using var output = new FileStream("PFN-TypeColors.css", FileMode.OpenOrCreate);
            output.SetLength(0);

            using var writer = new StreamWriter(output);

            writer.Write(_notice);

            foreach (var impulseType in Enum.GetValues<ImpulseType>())
            {
                var color = impulseType.GetImpulseColor().MulRGB(1.5f);
                writer.Write(string.Format(_cssTemplate, impulseType.ToString(), ColorXToString(color)));
            }

            writer.Write(string.Format(_cssTemplate, "SyncOperation", ColorXToString(DatatypeColorHelper.GetOperationColor(false).MulRGB(1.5f))));
            writer.Write(string.Format(_cssTemplate, "AsyncOperation", ColorXToString(DatatypeColorHelper.GetOperationColor(true).MulRGB(1.5f))));

            foreach (var type in types)
            {
                var color = type.GetTypeColor().MulRGB(1.5f);
                writer.Write(string.Format(_cssTemplate, type.Name, ColorXToString(color)));
            }

            writer.Flush();
            writer.Close();
        }
    }
}