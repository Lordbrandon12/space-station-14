using System.Linq;
using Content.Shared.Mind;
using Content.Shared.Random.Helpers;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Toolshed.Commands.Values;

namespace Content.Shared.GURPS.Atributes
{
    public sealed partial class SharedAttributeSystem : EntitySystem
    {
        [Dependency] private readonly SharedJobSystem _jobs = default!;
        [Dependency] private readonly SharedMindSystem _minds = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly IEntityManager _entityMan = default!;
        [Dependency] private readonly IGameTiming _timing = default!;

        private const string SkillKeyword = "skill";

        private const int MinDiceRoll = 3;
        private const int MaxDiceRoll = 18;

        public void InitializeSkills()
        {
            SubscribeLocalEvent<MindComponent, RoleAddedEvent>(OnRoleAdded);
            SubscribeLocalEvent<MindComponent, MindCreatedEvent>(OnMindCreated);

            SubscribeLocalEvent<WeaponAttributesComponent, GetMeleeDiceRollEvent>(OnGetMeleeDiceRoll);
        }

        private void OnGetMeleeDiceRoll(Entity<WeaponAttributesComponent> ent, ref GetMeleeDiceRollEvent args)
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

        public bool TryGetGoverningAttributes(WeaponAttributesComponent comp,
                                              Dictionary<EntProtoId, int> stats,
                                              out IEnumerable<KeyValuePair<EntProtoId, int>> attributes)
        {
            List<KeyValuePair<EntProtoId, int>> governing = new();

            foreach (var (key, bonus) in comp.GoverningAttributes)
            {
                if (stats.TryGetValue(key, out var value))
                    governing.Add(new KeyValuePair<EntProtoId, int>(key, value + bonus));
            }

            attributes = governing;
            return governing.Count > 0;
        }
    }
}
