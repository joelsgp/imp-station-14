using Content.Client.Atmos.Monitor.UI.Widgets;
using Content.Client.Message;
using Content.Client.Stylesheets;
using Content.Client.UserInterface.Controls;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Monitor;
using Content.Shared.Atmos.Monitor.Components;
using Content.Shared.Atmos.Piping.Unary.Components;
using Content.Shared.Temperature;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.Atmos.Monitor.UI;

[GenerateTypedNameReferences]
public sealed partial class AirAlarmWindow : FancyWindow
{
    public event Action<string, IAtmosDeviceData>? AtmosDeviceDataChanged;
	public event Action<IAtmosDeviceData>? AtmosDeviceDataCopied;
    public event Action<string, AtmosMonitorThresholdType, AtmosAlarmThreshold, Gas?>? AtmosAlarmThresholdChanged;
    public event Action<AirAlarmMode>? AirAlarmModeChanged;
    public event Action<bool>? AutoModeChanged;
    public event Action? ResyncAllRequested;
    public event Action<AirAlarmTab>? AirAlarmTabChange;

    private RichTextLabel _address => CDeviceAddress;
    private RichTextLabel _deviceTotal => CDeviceTotal;
    private RichTextLabel _pressure => CPressureLabel;
    private RichTextLabel _temperature => CTemperatureLabel;
    private RichTextLabel _alarmState => CStatusLabel;

    private TabContainer _tabContainer => CTabContainer;
    private BoxContainer _ventDevices => CVentContainer;
    private BoxContainer _scrubberDevices => CScrubberContainer;

    private Dictionary<string, PumpControl> _pumps = new();
    private Dictionary<string, ScrubberControl> _scrubbers = new();
    private Dictionary<string, SensorInfo> _sensors = new();
    private Button _resyncDevices => CResyncButton;


    private Dictionary<Gas, Label> _gasLabels = new();

    private OptionButton _modes => CModeButton;

    private CheckBox _autoMode => AutoModeCheckBox;

    public AirAlarmWindow(BoundUserInterface owner)
    {
        RobustXamlLoader.Load(this);

        foreach (var mode in Enum.GetValues<AirAlarmMode>())
        {
            var text = mode switch
            {
                AirAlarmMode.Filtering => "air-alarm-ui-mode-filtering",
                AirAlarmMode.WideFiltering => "air-alarm-ui-mode-wide-filtering",
                AirAlarmMode.Fill => "air-alarm-ui-mode-fill",
                AirAlarmMode.Panic => "air-alarm-ui-mode-panic",
                AirAlarmMode.None => "air-alarm-ui-mode-none",
                _ => "error"
            };
            _modes.AddItem(Loc.GetString(text));
        }

        _modes.OnItemSelected += args =>
        {
            _modes.SelectId(args.Id);
            AirAlarmModeChanged!.Invoke((AirAlarmMode) args.Id);
        };

        _autoMode.OnToggled += args =>
        {
            AutoModeChanged!.Invoke(_autoMode.Pressed);
        };

        _tabContainer.SetTabTitle(0, Loc.GetString("air-alarm-ui-window-tab-vents"));
        _tabContainer.SetTabTitle(1, Loc.GetString("air-alarm-ui-window-tab-scrubbers"));
        _tabContainer.SetTabTitle(2, Loc.GetString("air-alarm-ui-window-tab-sensors"));

        _tabContainer.OnTabChanged += idx =>
        {
            AirAlarmTabChange!((AirAlarmTab) idx);
        };

        _resyncDevices.OnPressed += _ =>
        {
            _ventDevices.RemoveAllChildren();
            _pumps.Clear();
            _scrubberDevices.RemoveAllChildren();
            _scrubbers.Clear();
            CSensorContainer.RemoveAllChildren();
            _sensors.Clear();
            ResyncAllRequested!.Invoke();
        };

        EntityView.SetEntity(owner.Owner);
    }

    public void UpdateState(AirAlarmUIState state)
    {
        _address.SetMarkup(state.Address);
        _deviceTotal.SetMarkup($"{state.DeviceCount}");
        _pressure.SetMarkup(Loc.GetString("air-alarm-ui-window-pressure", ("pressure", $"{state.PressureAverage:0.##}")));
        _temperature.SetMarkup(Loc.GetString("air-alarm-ui-window-temperature", ("tempC", $"{TemperatureHelpers.KelvinToCelsius(state.TemperatureAverage):0.#}"), ("temperature", $"{state.TemperatureAverage:0.##}")));
        _alarmState.SetMarkup(Loc.GetString("air-alarm-ui-window-alarm-state",
                    ("color", ColorForAlarm(state.AlarmType)),
                    ("state", $"{state.AlarmType}")));
        UpdateModeSelector(state.Mode);
        UpdateAutoMode(state.AutoMode);
        foreach (var (addr, dev) in state.DeviceData)
        {
            UpdateDeviceData(addr, dev);
        }

        _tabContainer.CurrentTab = (int) state.Tab;
    }

    public void UpdateModeSelector(AirAlarmMode mode)
    {
        _modes.SelectId((int) mode);
    }

    public void UpdateAutoMode(bool enabled)
    {
        _autoMode.Pressed = enabled;
    }

    public void UpdateDeviceData(string addr, IAtmosDeviceData device)
    {
        switch (device)
        {
            case GasVentPumpData pump:
                if (!_pumps.TryGetValue(addr, out var pumpControl))
                {
                    var control= new PumpControl(pump, addr);
                    control.PumpDataChanged += AtmosDeviceDataChanged!.Invoke;
					control.PumpDataCopied += AtmosDeviceDataCopied!.Invoke;
                    _pumps.Add(addr, control);
                    CVentContainer.AddChild(control);
                }
                else
                {
                    pumpControl.ChangeData(pump);
                }

                break;
            case GasVentScrubberData scrubber:
                if (!_scrubbers.TryGetValue(addr, out var scrubberControl))
                {
                    var control = new ScrubberControl(scrubber, addr);
                    control.ScrubberDataChanged += AtmosDeviceDataChanged!.Invoke;
					control.ScrubberDataCopied += AtmosDeviceDataCopied!.Invoke;
                    _scrubbers.Add(addr, control);
                    CScrubberContainer.AddChild(control);
                }
                else
                {
                    scrubberControl.ChangeData(scrubber);
                }

                break;
            case AtmosSensorData sensor:
                if (!_sensors.TryGetValue(addr, out var sensorControl))
                {
                    var control = new SensorInfo(sensor, addr);
                    control.OnThresholdUpdate += AtmosAlarmThresholdChanged;
                    _sensors.Add(addr, control);
                    CSensorContainer.AddChild(control);
                }
                else
                {
                    sensorControl.ChangeData(sensor);
                }

                break;
        }
    }

    public static Color ColorForThreshold(float amount, AtmosAlarmThreshold threshold)
    {
        threshold.CheckThreshold(amount, out AtmosAlarmType curAlarm);
        return ColorForAlarm(curAlarm);
    }

    public static Color ColorForAlarm(AtmosAlarmType curAlarm)
    {
        if(curAlarm == AtmosAlarmType.Danger)
        {
            return StyleNano.DangerousRedFore;
        }
        else if(curAlarm == AtmosAlarmType.Warning)
        {
            return StyleNano.ConcerningOrangeFore;
        }

        return StyleNano.GoodGreenFore;
    }


}
