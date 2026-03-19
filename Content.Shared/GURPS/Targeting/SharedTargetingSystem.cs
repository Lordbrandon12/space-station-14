namespace Content.Shared.GURPS.Targeting
{
    public abstract partial class SharedTargetingSystem : EntitySystem
    {
        public override void Initialize()
        {
            SubscribeAllEvent<RequestSetTargetBodyPartEvent>(HandleSetTargetBodyPart);

            base.Initialize();
        }

        private void HandleSetTargetBodyPart(RequestSetTargetBodyPartEvent ev, EntitySessionEventArgs eventArgs)
        {
            if (eventArgs.SenderSession.AttachedEntity == null)
                return;

            TrySetActiveBodyPart(ev.BodyPart, eventArgs.SenderSession.AttachedEntity.Value);
        }

        private void TrySetActiveBodyPart(TargetBodyPart bodyPart, EntityUid ent)
        {
            if (!TryComp<TargetingComponent>(ent, out var comp))
                return;

            if (comp.BodyPart == bodyPart)
                return;

            comp.BodyPart = bodyPart;
            Dirty(ent, comp);
        }
    }
}
