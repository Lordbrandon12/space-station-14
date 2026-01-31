using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.Gameplay;
using Content.Shared.GURPS.Targeting;
using System.Linq;
using Content.Client.GURPS.Targeting;

namespace Content.Client.UserInterface.Systems.GURPS.Targeting;

public sealed class TargetingDollUIController : UIController
{
    [UISystemDependency] private readonly TargetingSystem _targetingSystem = default!;

    public TargetBodyPart SelectedBodyPart { get; private set; } = TargetBodyPart.Torso;

    private TargetingDoll? _targetingDoll;
    private bool _initialized;

    public override void Initialize()
    {
        base.Initialize();

        var gameplayStateLoad = UIManager.GetUIController<GameplayStateLoadController>();
        gameplayStateLoad.OnScreenLoad += OnScreenLoad;
    }

    private void OnScreenLoad()
    {
        if (_initialized)
            return;

        if (UIManager.ActiveScreen?.GetWidget<TargetingDoll>() is not { } doll)
            return;

        _targetingDoll = doll;
        _targetingDoll.Setup();

        foreach (var button in doll.BodyParts.Values)
            button.OnPressed += OnTextureButtonPressed;

        if (doll.BodyParts.TryGetValue(SelectedBodyPart, out var startButton))
            doll.SetActive(startButton);

        _initialized = true;
    }

    private void OnTextureButtonPressed(BaseButton.ButtonEventArgs args)
    {
        if (_targetingDoll == null)
            return;

        _targetingDoll.SetActive(args.Button);

        SelectedBodyPart = _targetingDoll.BodyParts.FirstOrDefault(x => x.Value == args.Button).Key;
        _targetingSystem.UIBodyPartChanged(SelectedBodyPart);
    }
}
