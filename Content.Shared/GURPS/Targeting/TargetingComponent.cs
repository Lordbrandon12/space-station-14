using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.GURPS.Targeting
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class TargetingComponent : Component
    {
        [DataField]
        public TargetBodyPart BodyPart { get; set; } = TargetBodyPart.Torso;
    }

    [Serializable, NetSerializable]
    public sealed class RequestSetTargetBodyPartEvent : EntityEventArgs
    {
        public TargetBodyPart BodyPart { get; }

        public RequestSetTargetBodyPartEvent(TargetBodyPart bodyPart)
        {
            BodyPart = bodyPart;
        }
    }
}
