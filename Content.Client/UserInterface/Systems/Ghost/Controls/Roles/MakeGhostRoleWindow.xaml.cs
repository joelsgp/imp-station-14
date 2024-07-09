﻿using System.Numerics;
using Content.Server.Ghost.Roles.Raffles;
using Content.Shared.Ghost.Roles.Raffles;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;
using static Robust.Client.UserInterface.Controls.BaseButton;

namespace Content.Client.UserInterface.Systems.Ghost.Controls.Roles
{
    [GenerateTypedNameReferences]
    public sealed partial class MakeGhostRoleWindow : DefaultWindow
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        private readonly List<GhostRoleRaffleSettingsPrototype> _rafflePrototypes = [];

        private const int RaffleDontRaffleId = -1;
        private const int RaffleCustomRaffleId = -2;
        private int _raffleSettingId = RaffleDontRaffleId;

        private NetEntity? Entity { get; set; }

        public event MakeRole? OnMake;

        public MakeGhostRoleWindow()
        {
            IoCManager.InjectDependencies(this);
            RobustXamlLoader.Load(this);

            MakeSentientLabel.MinSize = new Vector2(150, 0);
            RoleEntityLabel.MinSize = new Vector2(150, 0);
            RoleNameLabel.MinSize = new Vector2(150, 0);
            RoleName.MinSize = new Vector2(300, 0);
            RoleDescriptionLabel.MinSize = new Vector2(150, 0);
            RoleDescription.MinSize = new Vector2(300, 0);
            RoleRulesLabel.MinSize = new Vector2(150, 0);
            RoleRules.MinSize = new Vector2(300, 0);
            RaffleLabel.MinSize = new Vector2(150, 0);
            RaffleButton.MinSize = new Vector2(300, 0);
            RaffleInitialDurationLabel.MinSize = new Vector2(150, 0);
            RaffleInitialDuration.MinSize = new Vector2(300, 0);
            RaffleJoinExtendsDurationByLabel.MinSize = new Vector2(150, 0);
            RaffleJoinExtendsDurationBy.MinSize = new Vector2(270, 0);
            RaffleMaxDurationLabel.MinSize = new Vector2(150, 0);
            RaffleMaxDuration.MinSize = new Vector2(270, 0);

            RaffleInitialDuration.OverrideValue(30);
            RaffleJoinExtendsDurationBy.OverrideValue(5);
            RaffleMaxDuration.OverrideValue(60);

            RaffleInitialDuration.SetButtons(new List<int> { -30, -10 }, new List<int> { 10, 30 });
            RaffleJoinExtendsDurationBy.SetButtons(new List<int> { -10, -5 }, new List<int> { 5, 10 });
            RaffleMaxDuration.SetButtons(new List<int> { -30, -10 }, new List<int> { 10, 30 });

            RaffleInitialDuration.IsValid = duration => duration > 0;
            RaffleJoinExtendsDurationBy.IsValid = duration => duration >= 0;
            RaffleMaxDuration.IsValid = duration => duration > 0;

            RaffleInitialDuration.ValueChanged += OnRaffleDurationChanged;
            RaffleJoinExtendsDurationBy.ValueChanged += OnRaffleDurationChanged;
            RaffleMaxDuration.ValueChanged += OnRaffleDurationChanged;


            RaffleButton.AddItem("Don't raffle", RaffleDontRaffleId);
            RaffleButton.AddItem("Custom settings", RaffleCustomRaffleId);

            var raffleProtos =
                _prototypeManager.EnumeratePrototypes<GhostRoleRaffleSettingsPrototype>();

            var idx = 0;
            foreach (var raffleProto in raffleProtos)
            {
                _rafflePrototypes.Add(raffleProto);
                var s = raffleProto.Settings;
                var label =
                    $"{raffleProto.ID} (initial {s.InitialDuration}s, max {s.MaxDuration}s, join adds {s.JoinExtendsDurationBy}s)";
                RaffleButton.AddItem(label, idx++);
            }

            MakeButton.OnPressed += OnMakeButtonPressed;
            RaffleButton.OnItemSelected += OnRaffleButtonItemSelected;
        }

        private void OnRaffleDurationChanged(ValueChangedEventArgs args)
        {
            ValidateRaffleDurations();
        }

        private void ValidateRaffleDurations()
        {
            if (RaffleInitialDuration.Value > RaffleMaxDuration.Value)
            {
                MakeButton.Disabled = true;
                MakeButton.ToolTip = "The initial duration must not exceed the maximum duration.";
            }
            else
            {
                MakeButton.Disabled = false;
                MakeButton.ToolTip = null;
            }
        }

        private void OnRaffleButtonItemSelected(OptionButton.ItemSelectedEventArgs args)
        {
            _raffleSettingId = args.Id;
            args.Button.SelectId(args.Id);
            if (args.Id != RaffleCustomRaffleId)
            {
                RaffleCustomSettingsContainer.Visible = false;
                MakeButton.ToolTip = null;
                MakeButton.Disabled = false;
            }
            else
            {
                RaffleCustomSettingsContainer.Visible = true;
                ValidateRaffleDurations();
            }
        }

        public void SetEntity(IEntityManager entManager, NetEntity entity)
        {
            Entity = entity;
            RoleName.Text = entManager.GetComponent<MetaDataComponent>(entManager.GetEntity(entity)).EntityName;
            RoleEntity.Text = $"{entity}";
        }

        private void OnMakeButtonPressed(ButtonEventArgs args)
        {
            if (Entity == null)
            {
                return;
            }

            GhostRoleRaffleSettings? raffleSettings = null;

            if (_raffleSettingId == RaffleCustomRaffleId)
            {
                raffleSettings = new GhostRoleRaffleSettings()
                {
                    InitialDuration = (uint) RaffleInitialDuration.Value,
                    JoinExtendsDurationBy = (uint) RaffleJoinExtendsDurationBy.Value,
                    MaxDuration = (uint) RaffleMaxDuration.Value
                };
            }
            else if (_raffleSettingId != RaffleDontRaffleId)
            {
                raffleSettings = _rafflePrototypes[_raffleSettingId].Settings;
            }

            OnMake?.Invoke(Entity.Value, RoleName.Text, RoleDescription.Text, RoleRules.Text, MakeSentientCheckbox.Pressed, raffleSettings);
        }

        public delegate void MakeRole(NetEntity uid, string name, string description, string rules, bool makeSentient, GhostRoleRaffleSettings? settings);
    }
}
