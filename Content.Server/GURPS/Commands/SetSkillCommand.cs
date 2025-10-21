using System.Linq;
using System.Runtime.CompilerServices;
using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.GURPS.Atributes;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;
using DependencyAttribute = Robust.Shared.IoC.DependencyAttribute;

namespace Content.Server.GURPS.Commands
{
    [AdminCommand(AdminFlags.Debug)]
    public sealed class SetSkillCommand : IConsoleCommand
    {
        [Dependency] private readonly IEntityManager _entities = default!;
        [Dependency] private readonly IPrototypeManager _prototypes = default!;

        public string Command => "setskill";

        public string Description => "Sets the entity's skill";

        public string Help => $"{Command} <EntityId> <SkillId> <value>";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length < 1 || args.Length > 3)
            {
                shell.WriteLine(Help);
                return;
            }

            if (!NetEntity.TryParse(args[0], out var eNet) || !_entities.TryGetEntity(eNet, out var entUid))
            {
                shell.WriteError($"Failed to parse euid '{args[0]}'.");
                return;
            }

            if (!_entities.TryGetComponent<MindContainerComponent>(entUid, out var mindComp))
            {
                shell.WriteError("Entity does not have a mind container.");
                return;
            }

            if (!_entities.TryGetComponent<MindComponent>(mindComp.Mind, out var mind))
            {
                shell.WriteError("Entity does not have a mind.");
                return;
            }

            if (!int.TryParse(args[2], out var value))
            {
                shell.WriteError("Failed to parse value to int.");
            }

            var attributes = _prototypes.EnumeratePrototypes<AttributePrototype>();
            if (attributes.Where(x => x.ID == args[1]).Count() < 1)
            {
                shell.WriteError($"{args[1]} is not a valid skill id");
                return;
            }

            mind.Skills[args[1]] = value;
            _entities.Dirty(mindComp.Mind.Value, mind);
            shell.WriteLine($"{args[1]} set to {value}");
        }
    }
}
