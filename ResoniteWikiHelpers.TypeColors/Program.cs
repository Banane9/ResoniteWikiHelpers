using Elements.Core;
using Elements.Quantity;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Security;
using ProtoFlux.Runtimes.Execution.Nodes.Math;
using System.Globalization;
using TwitchLib.Client.Enums;

namespace ResoniteWikiHelpers.TypeColors
{
    internal class Program
    {
        private static readonly string _cssTemplate =
@".PFN-{0} {{
  stroke: rgb({1});
  fill: rgb({1}, 0.174);
  background-image: linear-gradient(rgba({1}, 0.7), rgba({1}, 0.7));
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
            var rgb = color.rgb * 1.5f;
            var max = MathX.Max(rgb.x, rgb.y, rgb.z, 1f);
            rgb = MathX.RoundToInt(255 * (rgb / max));

            return $"{rgb.x}, {rgb.y}, {rgb.z}";
        }

        private static void Main(string[] args)
        {
            var types = GenericTypesAttribute.GetTypes(GenericTypesAttribute.Group.EnginePrimitivesAndEnums)
                .Concat(new[]
                {
                    typeof(object),
                    typeof(Nullable), typeof(Nullable<>),
                    typeof(Guid), typeof(RefID),
                    typeof(IValue), typeof(IValue<>), typeof(IVariable<,>),
                    typeof(IField), typeof(IField<>),
                    typeof(ISyncRef), typeof(ISyncRef<>),
                    typeof(TangentPointFloat),
                    typeof(bobool3ol), typeof(half),
                    typeof(ColorProfile),
                    typeof(CultureInfo), typeof(IFormatProvider),
                    typeof(DateTimeKind), typeof(DayOfWeek),
                    typeof(StringComparison),
                    typeof(User), typeof(UserRef),
                    typeof(UserRoot), typeof(BodyNode),
                    typeof(FingerType), typeof(FingerSegmentType), typeof(IFingerPoseSource),
                    typeof(CharacterController), typeof(IAvatarAnchor),
                    typeof(JoinRequestHandle), typeof(Platform),
                    typeof(IWorldElement), typeof(Slot), typeof(Component), typeof(IComponent),
                    typeof(ProtoFluxNode),
                    typeof(ICollider), typeof(BoundingBox),
                    typeof(Rect), typeof(BoundingBox2D),
                    typeof(ReflectionProbe),
                    typeof(ComponentHandling),
                    typeof(IFocusable),
                    typeof(UsersAssetLoadProgress<>), typeof(AssetLoadState),
                    typeof(IPlayable), typeof(AudioOutput), typeof(SyncPlayback),
                    typeof(AudioRolloffMode), typeof(AudioDistanceSpace), typeof(AudioDistanceSpace), typeof(AudioTypeGroup),
                    typeof(RawDataTool), typeof(ITool),
                    typeof(IWorldLink), typeof(Userspace.WorldRelation),
                    typeof(LocaleResource),
                    typeof(IAssetProvider), typeof(IAssetProvider<>),
                    typeof(Camera), typeof(Elements.Assets.TextureFormat),
                    typeof(TextureWrapMode), typeof(Elements.Assets.WrapMode),
                    typeof(StaticAudioClip), typeof(StaticMesh),
                    typeof(SpriteProvider),typeof(Texture2D), typeof(StaticTexture2D),
                    typeof(Texture3D),typeof(StaticTexture3D),
                    typeof(Grabber), typeof(IGrabbable),
                    typeof(ILocomotionModule),
                    typeof(Animation),
                    typeof(ITouchable), typeof(EventState), typeof(TouchType), typeof(TouchEventType),
                    typeof(CurvePreset),
                    typeof(BadgeColor), typeof(SubscriptionPlan),
                    typeof(WebsocketClient),
                    });

            using var output = new FileStream("protofluxTypes.css", FileMode.OpenOrCreate);
            output.SetLength(0);

            using var writer = new StreamWriter(output);

            writer.Write(_notice);

            foreach (var impulseType in Enum.GetValues<ImpulseType>())
            {
                var color = impulseType.GetImpulseColor();
                writer.Write(_cssTemplate, impulseType.ToString(), ColorXToString(color));
            }

            var syncOpColor = DatatypeColorHelper.GetOperationColor(false);
            var asyncOpColor = DatatypeColorHelper.GetOperationColor(true);

            writer.Write(_cssTemplate, "SyncOperation", ColorXToString(syncOpColor));
            writer.Write(_cssTemplate, "AsyncOperation", ColorXToString(asyncOpColor));
            writer.Write(_cssTemplate, "MixedOperation", ColorXToString(MathX.Lerp(syncOpColor, asyncOpColor, .5f)));

            foreach (var type in types)
            {
                var color = type.GetTypeColor();
                writer.Write(_cssTemplate, type.Name.Replace('`', '_'), ColorXToString(color));
            }

            var dummyColor = typeof(dummy).GetTypeColor();
            writer.Write(_cssTemplate, "T", ColorXToString(dummyColor));
            writer.Write(_cssTemplate, "I", ColorXToString(dummyColor));
            writer.Write(_cssTemplate, "O", ColorXToString(dummyColor));

            var enumColor = typeof(Enum).GetTypeColor();
            writer.Write(_cssTemplate, "E", ColorXToString(enumColor));

            var unitColor = typeof(IQuantity<>).GetTypeColor();
            writer.Write(_cssTemplate, "U", ColorXToString(unitColor));

            var assetColor = typeof(IAsset).GetTypeColor();
            writer.Write(_cssTemplate, "A", ColorXToString(unitColor));

            writer.Flush();
            writer.Close();
        }
    }
}