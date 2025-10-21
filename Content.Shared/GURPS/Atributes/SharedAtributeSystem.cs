using System.Linq;
using Content.Shared.Mind;
using Content.Shared.Random.Helpers;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared.GURPS.Atributes
{
    public sealed partial class SharedAttributeSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototypes = default!;
        [Dependency] private readonly SharedJobSystem _jobs = default!;
        [Dependency] private readonly SharedMindSystem _minds = default!;
        [Dependency] private readonly IEntityManager _entityMan = default!;
        [Dependency] private readonly IGameTiming _timing = default!;

        private const string SkillKeyword = "skill";

        private const int MinDiceRoll = 3;
        private const int MaxDiceRoll = 18;


        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<AttributesComponent, GetMeleeDamageEvent>(OnGetDiceRoll);
            SubscribeLocalEvent<MindComponent, RoleAddedEvent>(OnRoleAdded);
            SubscribeLocalEvent<MindComponent, MindCreatedEvent>(OnMindCreated);
            SubscribeLocalEvent<AttributesComponent, GetDiceRollEvent>(OnGetMeleeDiceRoll);
        }

        private void OnGetDiceRoll(Entity<AttributesComponent> ent, ref GetMeleeDamageEvent args)
        {
            if (!TryComp<AttributesComponent>(ent, out var weaponAttributes))
                return;

            if (!TryComp<StatsComponent>(args.User, out var statsComp))
                return;

            if (!TryGetAtributeMultipliers(weaponAttributes, statsComp, out var stats))
                return;

            foreach (var keyValue in stats)
                args.Damage *= keyValue.Value / GetStatBaseValue(keyValue.Key);
        }

        private bool TryGetAtributeMultipliers(AttributesComponent weaponAttributesComp,
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

        public bool TryGetEntityStats(EntityUid entUid, out Dictionary<EntProtoId, int> stats)
        {
            stats = new();
            if (!TryComp<StatsComponent>(entUid, out var statsComp))
                return false;
            stats = statsComp.Attributes;
            if (stats.Count < 1)
                return false;
            return true;
        }

        private void OnGetMeleeDiceRoll(Entity<AttributesComponent> ent, ref GetDiceRollEvent args)
        {
            IEnumerable<KeyValuePair<EntProtoId, int>>? attributes;
            args.Missed = false;

            if (!_entityMan.TryGetNetEntity(ent, out var netEnt))
                return;

            if (!_minds.TryGetMind(args.User, out var mindId, out var mind))
                return;

            if (!TryComp<StatsComponent>(args.User, out var statsComp))
                return;

            //it's necessary to get the entity's stats aswell so, incase someone lacks the necessary skill, it can fallback
            //to their dexterity.
            //it automatically applies the modifiers defined in the WeaponAttributesComponent
            if (!TryGetGoverningAttributes(ent.Comp, mind.Skills.Concat(statsComp.Attributes).ToDictionary(), out attributes))
                return;

            // TODO: Replace with RandomPredicted once the engine PR is merged
            var seed = SharedRandomExtensions.HashCodeCombine(new() { (int)_timing.CurTick.Value, GetNetEntity(ent).Id });
            int result = Roll3D6(seed); // this is based on GURPS so it'll away roll a 3D6
            int governingStat = attributes.Max(x => x.Value);
            args.Missed = result > governingStat; //TODO:probably rename this to result when I get around to add the DiceRollResultEnum
        }

        private int Roll3D6(int seed)
        {
            var rand = new System.Random(seed);
            return rand.Next(MinDiceRoll, MaxDiceRoll);
        }

        private void OnMindCreated(Entity<MindComponent> ent, ref MindCreatedEvent args)
        {
            var skills = _prototypes.EnumeratePrototypes<AttributePrototype>().Where(x => x.ID.Contains(SkillKeyword));

            if (!skills.Any())
                return;

            foreach (var skill in skills)
                ent.Comp.Skills.Add(skill.ID, skill.BaseValue);
        }

        private void OnRoleAdded(Entity<MindComponent> ent, ref RoleAddedEvent args)
        {
            if (!_jobs.MindTryGetJob(ent, out var job))
                return;

            if (job?.Skills is null)
                return;

            foreach (var skill in job.Skills)
                ent.Comp.Skills[skill.Key] = skill.Value;
        }

        public bool TryGetGoverningAttributes(AttributesComponent comp,
                                              Dictionary<EntProtoId, int> stats,
                                              out IEnumerable<KeyValuePair<EntProtoId, int>> attributes)
        {
            List<KeyValuePair<EntProtoId, int>> governing = new();

            foreach (var (key, bonus) in comp.GoverningAttributes)
            {
                if (stats.TryGetValue(key, out var value))
                {
                    var attribute = value + bonus;
                    governing.Add(new KeyValuePair<EntProtoId, int>(key, Math.Clamp(attribute, MinDiceRoll, MaxDiceRoll)));
                }
            }

            attributes = governing;
            return governing.Count > 0;
        }

        public bool TryGetEntitySkills(EntityUid entUid, out Dictionary<EntProtoId, int> skills)
        {
            skills = new();
            if (!_minds.TryGetMind(entUid, out var mindId, out var mind))
                return false;
            skills = mind.Skills;
            if (skills.Count < 1)
                return false;
            return true;
        }
    }

    //TODO: I have to change this to use a enum representing the different outcomes of the diceroll
    [ByRefEvent]
    public record struct GetDiceRollEvent(EntityUid User, bool Missed = false);
}
