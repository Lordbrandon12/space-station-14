using System.Numerics;
using Content.Client.Message;
using Content.Client.Paper.UI;
using Content.Shared.CCVar;
using Content.Shared.Movement.Components;
using Content.Shared.Tips;
using Robust.Client.GameObjects;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Client.Audio;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Timing;
using static Content.Client.Tips.TippyUI;
using Content.Client.Tips;
using Content.Client.UserInterface.Systems.Gameplay;

namespace Content.Client.UserInterface.Systems.Targeting;

public sealed class TargetingDollUIController : UIController
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IResourceCache _resCache = default!;
    [UISystemDependency] private readonly AudioSystem _audio = default!;
    [UISystemDependency] private readonly SpriteSystem _sprite = default!;

    private EntityUid _entity;

    public override void Initialize()
    {
        base.Initialize();

        var gameplayStateLoad = UIManager.GetUIController<GameplayStateLoadController>();
        gameplayStateLoad.OnScreenLoad += OnScreemLoad;
    }

    private void OnScreemLoad()
    {
        if (UIManager.ActiveScreen?.GetWidget<TargetingDoll>() is { } targetDoll)
        {
            targetDoll.Setup();
        }
    }

    public void Setup()
    {

    }

}
