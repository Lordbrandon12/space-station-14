using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;

namespace Content.Shared.GURPS.Atributes
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class WeaponAttributesComponent : Component
    {
        [DataField("governingAttributes")]
        public Dictionary<EntProtoId, int> GoverningAttributes { get; set; }

        [DataField("multiplyingAttributes")]
        public List<EntProtoId> MultiplyingAttributes { get; set; }
    }
}
