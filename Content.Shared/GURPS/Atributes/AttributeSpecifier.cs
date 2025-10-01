using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.GURPS.Atributes
{
    [DataDefinition, Serializable, NetSerializable]
    public sealed partial class AttributeSpecifier
    {
        [DataField("governingAtributes")]
        public Dictionary<EntProtoId, int> GoverningAttributes { get; set; } = new();

        [DataField("multiplyingAtributes")]
        public List<EntProtoId> MultiplyingAttributes { get; set; } = new();
    }
}
