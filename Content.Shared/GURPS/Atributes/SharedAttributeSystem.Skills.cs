using System.Linq;
using Content.Shared.Mind;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Robust.Shared.Prototypes;

namespace Content.Shared.GURPS.Atributes
{
    public sealed partial class SharedAttributeSystem : EntitySystem
    {
        [Dependency] private readonly SharedJobSystem _jobs = default!;

        private const string SkillKeyword = "skill";
        public void InitializeSkills()
        {
            SubscribeLocalEvent<MindComponent, RoleAddedEvent>(OnRoleAdded);
            SubscribeLocalEvent<MindComponent, MindCreatedEvent>(OnMindCreated);
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
    }
}
