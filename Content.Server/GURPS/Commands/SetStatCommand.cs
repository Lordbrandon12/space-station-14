using System.Linq;
using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.GURPS.Atributes;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server.GURPS.Commands
{
    [AdminCommand(AdminFlags.Debug)]
    public sealed class SetStatCommand : IConsoleCommand
    {
        [Dependency] private readonly IEntityManager _entities = default!;
        [Dependency] private readonly IPrototypeManager _prototypes = default!;

        public string Command => "setstat";

        public string Description => "sets the entity stat";

        public string Help => $"{Command} <entityUid> <statId> <value>";

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

            if (!_entities.TryGetComponent<StatsComponent>(entUid, out var statsComp))
            {
                shell.WriteError("Entity does not have a stats component");
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

            statsComp.Attributes[args[1]] = value;
            _entities.Dirty(entUid.Value, statsComp);
            shell.WriteLine($"{args[1]} set to {value}");
        }
    }
}
