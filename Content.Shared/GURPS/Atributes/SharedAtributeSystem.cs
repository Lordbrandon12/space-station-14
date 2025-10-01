using System.Linq;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Prototypes;

namespace Content.Shared.GURPS.Atributes
{
    public sealed partial class SharedAttributeSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototypes = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<WeaponAttributesComponent, GetMeleeDamageEvent>(OnGetMeleeDamage);

            InitializeSkills();
        }

        private void OnGetMeleeDamage(Entity<WeaponAttributesComponent> ent, ref GetMeleeDamageEvent args)
        {
            if (!TryComp<WeaponAttributesComponent>(ent, out var weaponAttributes))
                return;

            if (!TryComp<StatsComponent>(args.User, out var statsComp))
                return;

            if (!TryGetAtributeMultipliers(weaponAttributes, statsComp, out var stats))
                return;

            foreach (var keyValue in stats)
                args.Damage *= keyValue.Value / GetStatBaseValue(keyValue.Key);
        }

        private bool TryGetAtributeMultipliers(WeaponAttributesComponent weaponAttributesComp,
                                              StatsComponent statsComp,
                                              out IEnumerable<KeyValuePair<EntProtoId, int>> stats)
        {
            var weaponMultipliers = weaponAttributesComp.MultiplyingAttributes;
            var multiplierIds = new HashSet<string>(weaponMultipliers.Select(x => x.Id));
            stats = statsComp.Attributes.Where(stat => multiplierIds.Contains(stat.Key.Id));

            if (!stats.Any())
                return false;

            return true;
        }


        private int GetStatBaseValue(EntProtoId key)
        {
            var prototype = _prototypes.EnumeratePrototypes<AttributePrototype>().Where(x => x.ID == key.Id);

            return prototype.FirstOrDefault()?.BaseValue ?? 1;
        }
    }
}
