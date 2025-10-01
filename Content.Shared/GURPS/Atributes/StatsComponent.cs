using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.GURPS.Atributes
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class StatsComponent : Component
    {
        [DataField("attributes")]
        public Dictionary<EntProtoId, int> Attributes { get; set; }
    }
}
