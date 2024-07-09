using Content.Shared.Disposal.Components;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;
using static Content.Shared.Disposal.Components.SharedDisposalRouterComponent;

namespace Content.Client.Disposal.UI
{
    /// <summary>
    /// Client-side UI used to control a <see cref="SharedDisposalRouterComponent"/>
    /// </summary>
    [GenerateTypedNameReferences]
    public sealed partial class DisposalRouterWindow : DefaultWindow
    {
        public DisposalRouterWindow()
        {
            RobustXamlLoader.Load(this);

            TagInput.IsValid = tags => TagRegex.IsMatch(tags);
        }


        public void UpdateState(DisposalRouterUserInterfaceState state)
        {
            TagInput.Text = state.Tags;
        }
    }
}
