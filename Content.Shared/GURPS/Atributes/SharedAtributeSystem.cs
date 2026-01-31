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

        private const int MinDiceRoll = 1;
        private const int MaxDiceRoll = 6;

        private const int MaxPossibleRoll = 18;
        private const int MinnPossibleRoll = 3;

        private const int MasteryThreshold = 16;
        private const int CriticalFailureDifference = 10;

        private readonly int[] _criticalSucessRange = [3, 4];
        private readonly int[] _criticalSucessMasteryRange = [3, 4, 6, 7];
        private readonly int[] _cricitalFailureRange = [17, 18];
        private readonly int[] _criticalFailureMasteryRange = [18];

        private readonly Dictionary<string, int> _attributeBaseValues = new();

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<AttributesComponent, GetMeleeDamageEvent>(OnGetDiceRoll);
            SubscribeLocalEvent<MindComponent, RoleAddedEvent>(OnRoleAdded);
            SubscribeLocalEvent<MindComponent, MindCreatedEvent>(OnMindCreated);
            SubscribeLocalEvent<AttributesComponent, GetDiceRollEvent>(OnGetDiceRoll);

            foreach(var prototype in _prototypes.EnumeratePrototypes<AttributePrototype>())
            {
                _attributeBaseValues[prototype.ID] = prototype.BaseValue;
            }
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
                args.Damage *= keyValue.Value / _attributeBaseValues[keyValue.Key];
        }

        private bool TryGetAtributeMultipliers(AttributesComponent weaponAttributesComp,
                                              StatsComponent statsComp,
                                              out List<KeyValuePair<EntProtoId, int>> result)
        {
            result = new();

            foreach (var stat in statsComp.Attributes)
            {
                foreach (var attribute in weaponAttributesComp.MultiplyingAttributes)
                {
                    if (stat.Key == attribute.Id)
                    {
                        result.Add(stat);
                        break;
                    }
                }
            }

            return result.Count > 0;
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

        private void OnGetDiceRoll(Entity<AttributesComponent> ent, ref GetDiceRollEvent args)
        {
            //TODO: in the future it'll take into account status effects, like if the player is low on stamina or
            // high on crack
            IEnumerable<KeyValuePair<EntProtoId, int>>? attributes;

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
            var seed = SharedRandomExtensions.HashCodeCombine([ (int)_timing.CurTick.Value, GetNetEntity(ent).Id ]);
            int result = Roll3D6(seed); // this is based on GURPS so it'll always roll a 3D6
            int governingStat = attributes.Max(x => x.Value);
            args.SkillLevel = governingStat;
            args.Result = ResolveDiceResult(result, governingStat);
        }

        private DiceResult ResolveDiceResult(int roll, int skill)
        {
            bool mastery = skill >= MasteryThreshold;

            var critFailure = mastery ? _criticalFailureMasteryRange :
                                        _criticalSucessRange;

            var critSucess = mastery ? _criticalSucessMasteryRange :
                                       _criticalSucessRange;

            if (critFailure.Contains(roll) || roll >= skill + CriticalFailureDifference)
                return DiceResult.CriticalFailure;

            if (critSucess.Contains(roll))
                return DiceResult.CriticalSucess;

            if (roll <= skill)
                return DiceResult.Sucess;

            return DiceResult.Failure;
        }

        private int Roll3D6(int seed)
        {
            var rand = new System.Random(seed);
            return rand.Next(MinDiceRoll, MaxDiceRoll) + rand.Next(MinDiceRoll, MaxDiceRoll) + rand.Next(MinDiceRoll, MaxDiceRoll);
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
                    governing.Add(new KeyValuePair<EntProtoId, int>(key, Math.Clamp(attribute, MinnPossibleRoll, MaxPossibleRoll)));
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


    [ByRefEvent]
    public record struct GetDiceRollEvent(EntityUid User, DiceResult Result = DiceResult.Failure, int SkillLevel = 0);
}
