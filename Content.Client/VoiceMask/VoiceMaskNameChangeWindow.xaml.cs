using Content.Client.UserInterface.Controls;
using Content.Shared.Speech;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;

namespace Content.Client.VoiceMask;

[GenerateTypedNameReferences]
public sealed partial class VoiceMaskNameChangeWindow : FancyWindow
{
    public Action<string>? OnNameChange;
    public Action<string?>? OnVerbChange;

    private List<(string, string)> _verbs = new();

    private string? _verb;

    public VoiceMaskNameChangeWindow(IPrototypeManager proto)
    {
        RobustXamlLoader.Load(this);

        NameSelectorSet.OnPressed += _ =>
        {
            OnNameChange?.Invoke(NameSelector.Text);
        };

        SpeechVerbSelector.OnItemSelected += args =>
        {
            OnVerbChange?.Invoke((string?) args.Button.GetItemMetadata(args.Id));
            SpeechVerbSelector.SelectId(args.Id);
        };

        ReloadVerbs(proto);

        AddVerbs();
    }

    private void ReloadVerbs(IPrototypeManager proto)
    {
        foreach (var verb in proto.EnumeratePrototypes<SpeechVerbPrototype>())
        {
            _verbs.Add((Loc.GetString(verb.Name), verb.ID));
        }
        _verbs.Sort((a, b) => a.Item1.CompareTo(b.Item1));
    }

    private void AddVerbs()
    {
        SpeechVerbSelector.Clear();

        AddVerb(Loc.GetString("chat-speech-verb-name-none"), null);
        foreach (var (name, id) in _verbs)
        {
            AddVerb(name, id);
        }
    }

    private void AddVerb(string name, string? verb)
    {
        var id = SpeechVerbSelector.ItemCount;
        SpeechVerbSelector.AddItem(name);
        if (verb is {} metadata)
            SpeechVerbSelector.SetItemMetadata(id, metadata);

        if (verb == _verb)
            SpeechVerbSelector.SelectId(id);
    }

    public void UpdateState(string name, string? verb)
    {
        NameSelector.Text = name;
        _verb = verb;

        for (int id = 0; id < SpeechVerbSelector.ItemCount; id++)
        {
            if (string.Equals(verb, SpeechVerbSelector.GetItemMetadata(id)))
            {
                SpeechVerbSelector.SelectId(id);
                break;
            }
        }
    }
}
