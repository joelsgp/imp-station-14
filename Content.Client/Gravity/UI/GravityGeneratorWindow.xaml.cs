﻿using Content.Shared.Gravity;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using FancyWindow = Content.Client.UserInterface.Controls.FancyWindow;

namespace Content.Client.Gravity.UI
{
    [GenerateTypedNameReferences]
    public sealed partial class GravityGeneratorWindow : FancyWindow
    {
        private readonly ButtonGroup _buttonGroup = new();

        private readonly GravityGeneratorBoundUserInterface _owner;

        public GravityGeneratorWindow(GravityGeneratorBoundUserInterface owner)
        {
            RobustXamlLoader.Load(this);
            IoCManager.InjectDependencies(this);

            _owner = owner;

            OnButton.Group = _buttonGroup;
            OffButton.Group = _buttonGroup;

            OnButton.OnPressed += _ => _owner.SetPowerSwitch(true);
            OffButton.OnPressed += _ => _owner.SetPowerSwitch(false);

            EntityView.SetEntity(owner.Owner);
        }

        public void UpdateState(SharedGravityGeneratorComponent.GeneratorState state)
        {
            if (state.On)
                OnButton.Pressed = true;
            else
                OffButton.Pressed = true;

            PowerLabel.Text = Loc.GetString(
                "gravity-generator-window-power-label",
                ("draw", state.PowerDraw),
                ("max", state.PowerDrawMax));

            PowerLabel.SetOnlyStyleClass(MathHelper.CloseTo(state.PowerDraw, state.PowerDrawMax) ? "Good" : "Caution");

            ChargeBar.Value = state.Charge;
            ChargeText.Text = (state.Charge / 255f).ToString("P0");
            StatusLabel.Text = Loc.GetString(state.PowerStatus switch
            {
                GravityGeneratorPowerStatus.Off => "gravity-generator-window-status-off",
                GravityGeneratorPowerStatus.Discharging => "gravity-generator-window-status-discharging",
                GravityGeneratorPowerStatus.Charging => "gravity-generator-window-status-charging",
                GravityGeneratorPowerStatus.FullyCharged => "gravity-generator-window-status-fully-charged",
                _ => throw new ArgumentOutOfRangeException()
            });

            StatusLabel.SetOnlyStyleClass(state.PowerStatus switch
            {
                GravityGeneratorPowerStatus.Off => "Danger",
                GravityGeneratorPowerStatus.Discharging => "Caution",
                GravityGeneratorPowerStatus.Charging => "Caution",
                GravityGeneratorPowerStatus.FullyCharged => "Good",
                _ => throw new ArgumentOutOfRangeException()
            });

            EtaLabel.Text = state.EtaSeconds >= 0
                ? Loc.GetString("gravity-generator-window-eta-value", ("left", TimeSpan.FromSeconds(state.EtaSeconds)))
                : Loc.GetString("gravity-generator-window-eta-none");

            EtaLabel.SetOnlyStyleClass(state.EtaSeconds >= 0 ? "Caution" : "Disabled");
        }
    }
}
