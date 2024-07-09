using Content.Client.UserInterface.Fragments;
using Content.Shared.Mech;
using Content.Shared.Mech.Components;
using JetBrains.Annotations;

namespace Content.Client.Mech.Ui;

[UsedImplicitly]
public sealed class MechBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private MechMenu? _menu;

    public MechBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = new(Owner);

        _menu.OnClose += Close;
        _menu.OpenCenteredLeft();

        _menu.OnRemoveButtonPressed += uid =>
        {
            SendMessage(new MechEquipmentRemoveMessage(EntMan.GetNetEntity(uid)));
        };
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not MechBoundUiState msg)
            return;
        UpdateEquipmentControls(msg);
        _menu?.UpdateMechStats();
        _menu?.UpdateEquipmentView();
    }

    public void UpdateEquipmentControls(MechBoundUiState state)
    {
        if (!EntMan.TryGetComponent<MechComponent>(Owner, out var mechComp))
            return;

        foreach (var ent in mechComp.EquipmentContainer.ContainedEntities)
        {
            var ui = GetEquipmentUi(ent);
            if (ui == null)
                continue;
            foreach (var (attached, estate) in state.EquipmentStates)
            {
                if (ent == EntMan.GetEntity(attached))
                    ui.UpdateState(estate);
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        _menu?.Close();
    }

    public UIFragment? GetEquipmentUi(EntityUid? uid)
    {
        var component = EntMan.GetComponentOrNull<UIFragmentComponent>(uid);
        component?.Ui?.Setup(this, uid);
        return component?.Ui;
    }
}

