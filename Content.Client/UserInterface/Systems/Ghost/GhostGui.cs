using Content.Client.Ghost;
using Content.Client.Stylesheets;
using Content.Client.UserInterface.Systems.Ghost.Controls;
using Content.Shared.CCVar;
using Content.Shared.Ghost;
using Robust.Client.Console;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Localization;
using Robust.Shared.Timing;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client.UserInterface.Systems.Ghost
{
    public sealed class GhostGui : Control
    {
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly IConfigurationManager _configurationManager = default!;
        private readonly Button _returnToBody = new() {Text = Loc.GetString("ghost-gui-return-to-body-button") };
        private readonly Button _ghostWarp = new() {Text = Loc.GetString("ghost-gui-ghost-warp-button") };
        private readonly Button _ghostRoles = new();
        private readonly Button _respawn = new() {Text = Loc.GetString("ghost-gui-respawn-button")};
        private readonly GhostComponent _owner;
        private readonly GhostSystem _system;

        public GhostTargetWindow? TargetWindow { get; }

        public GhostGui(GhostComponent owner, GhostSystem system, IEntityNetworkManager eventBus)
        {
            IoCManager.InjectDependencies(this);

            _owner = owner;
            _system = system;

            TargetWindow = new GhostTargetWindow(eventBus);

            MouseFilter = MouseFilterMode.Ignore;

            _ghostWarp.OnPressed += _ =>
            {
                eventBus.SendSystemNetworkMessage(new GhostWarpsRequestEvent());
                TargetWindow.Populate();
                TargetWindow.OpenCentered();
            };
            _returnToBody.OnPressed += _ =>
            {
                var msg = new GhostReturnToBodyRequest();
                eventBus.SendSystemNetworkMessage(msg);
            };
            _ghostRoles.OnPressed += _ => IoCManager.Resolve<IClientConsoleHost>()
                .RemoteExecuteCommand(null, "ghostroles");

            _respawn.OnPressed += _ => IoCManager.Resolve<IClientConsoleHost>()
                .RemoteExecuteCommand(null, "ghostrespawn");

            var container = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                Children =
                {
                    _returnToBody,
                    _ghostWarp,
                    _ghostRoles,
                }
            };
            AddChild(container);

            if (_configurationManager.GetCVar(CCVars.AllowRespawns))
            {
                container.AddChild(_respawn);
            }
        }

        public void UpdateRespawn()
        {
            var time = ( _gameTiming.CurTime - _owner.TimeOfDeath);
            var respawnTime = _configurationManager.GetCVar(CCVars.RespawnTime);
            _respawn.Disabled =  respawnTime > time.TotalSeconds;
            _respawn.Text = _respawn.Disabled
                ? Loc.GetString("ghost-gui-respawn-button-denied", ("time", (int)(respawnTime - time.TotalSeconds)))
                : Loc.GetString("ghost-gui-respawn-button-allowed");
        }

        public void Update()
        {
            _returnToBody.Disabled = !_owner.CanReturnToBody;
            _ghostRoles.Text = Loc.GetString("ghost-gui-ghost-roles-button", ("count", _system.AvailableGhostRoleCount));

            if (_system.AvailableGhostRoleCount != 0)
            {
                _ghostRoles.StyleClasses.Add(StyleBase.ButtonCaution);
            }
            else
            {
                _ghostRoles.StyleClasses.Remove(StyleBase.ButtonCaution);
            }

            TargetWindow?.Populate();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                TargetWindow?.Dispose();
            }
        }
    }
}
