using Content.Shared.Extinguisher;

namespace Content.Server.Extinguisher;

[RegisterComponent]
[Access(typeof(FireExtinguisherSystem))]
public sealed partial class FireExtinguisherComponent : SharedFireExtinguisherComponent
{
}
