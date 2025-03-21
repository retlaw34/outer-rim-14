﻿using Content.Server.Visible;
using Content.Shared.Revenant.Components;
using Content.Shared.Revenant.EntitySystems;
using Robust.Server.GameObjects;

namespace Content.Server.Revenant.EntitySystems;

public sealed class CorporealSystem : SharedCorporealSystem
{
    [Dependency] private readonly VisibilitySystem _visibilitySystem = default!;

    public override void OnStartup(EntityUid uid, CorporealComponent component, ComponentStartup args)
    {
        base.OnStartup(uid, component, args);

        if (TryComp<VisibilityComponent>(uid, out var visibility))
        {
            _visibilitySystem.RemoveLayer(visibility, (int) VisibilityFlags.Ghost, false);
            _visibilitySystem.AddLayer(visibility, (int) VisibilityFlags.Normal, false);
            _visibilitySystem.RefreshVisibility(visibility);
        }
    }

    public override void OnShutdown(EntityUid uid, CorporealComponent component, ComponentShutdown args)
    {
        base.OnShutdown(uid, component, args);

        if (TryComp<VisibilityComponent>(uid, out var visibility))
        {
            _visibilitySystem.AddLayer(visibility, (int) VisibilityFlags.Ghost, false);
            _visibilitySystem.RemoveLayer(visibility, (int) VisibilityFlags.Normal, false);
            _visibilitySystem.RefreshVisibility(visibility);
        }
    }
}
