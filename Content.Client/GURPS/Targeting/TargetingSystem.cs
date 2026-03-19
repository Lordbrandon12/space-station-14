using Content.Shared.GURPS.Targeting;

namespace Content.Client.GURPS.Targeting
{
    public sealed partial class TargetingSystem : SharedTargetingSystem
    {
        public override void Initialize()
        {
            base.Initialize();
        }

        public void UIBodyPartChanged(TargetBodyPart bodyPart)
        {
            RaisePredictiveEvent(new RequestSetTargetBodyPartEvent(bodyPart));
        }
    }
}
