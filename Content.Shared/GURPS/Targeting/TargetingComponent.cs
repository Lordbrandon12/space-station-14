using Robust.Shared.GameStates;

namespace Content.Shared.GURPS.Targeting
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class TargetingComponent : Component
    {
        public TargetBodyPart BodyPart { get; set; } = TargetBodyPart.Torso;
    }

    public sealed class RequestSetTargetedBodyPartEvent : EntityEventArgs
    {
        public TargetBodyPart BodyPart { get; }

        public RequestSetTargetedBodyPartEvent(TargetBodyPart bodyPart)
        {
            BodyPart = bodyPart;
        }
    }
}
