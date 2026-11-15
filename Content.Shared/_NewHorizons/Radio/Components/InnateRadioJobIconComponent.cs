using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._NewHorizons.Radio.Components;

/// <summary>
/// Gives an entity an innate job icon for radio chat messages,
/// regardless of equipped ID cards.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class InnateRadioJobIconComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public ProtoId<JobIconPrototype> Icon = "JobIconUnknown";
}
