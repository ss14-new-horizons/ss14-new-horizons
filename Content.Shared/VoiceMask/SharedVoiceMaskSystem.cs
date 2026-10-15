using Content.Shared.Inventory;
using Content.Shared.StatusIcon; // NH-Add (Radio job icons)
using Robust.Shared.Prototypes; // NH-Add (Radio job icons)
using Robust.Shared.Serialization;

namespace Content.Shared.VoiceMask;

[Serializable, NetSerializable]
public enum VoiceMaskUIKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class VoiceMaskBuiState : BoundUserInterfaceState
{
    public readonly string Name;
    public readonly string? Verb;
    public readonly bool Active;
    public readonly bool AccentHide;
    public readonly LocId TitleText;
    public readonly string TTSVoice; // Corvax-TTS
    public readonly ProtoId<JobIconPrototype>? JobIcon; // NH-Add (Radio job icons)

    public VoiceMaskBuiState(string name, string? verb, bool active, bool accentHide, LocId titleText, string voice, ProtoId<JobIconPrototype>? jobIcon = null) // Corvax-TTS // NH-Edit (Radio job icons)
    {
        Name = name;
        Verb = verb;
        Active = active;
        AccentHide = accentHide;
        TitleText = titleText;
        TTSVoice = voice;  // Corvax-TTS
        JobIcon = jobIcon; // NH-Add (Radio job icons)
    }
}

[Serializable, NetSerializable]
public sealed class VoiceMaskChangeNameMessage : BoundUserInterfaceMessage
{
    public readonly string Name;

    public VoiceMaskChangeNameMessage(string name)
    {
        Name = name;
    }
}

/// <summary>
/// Change the speech verb prototype to override, or null to use the user's verb.
/// </summary>
[Serializable, NetSerializable]
public sealed class VoiceMaskChangeVerbMessage : BoundUserInterfaceMessage
{
    public readonly string? Verb;

    public VoiceMaskChangeVerbMessage(string? verb)
    {
        Verb = verb;
    }
}

// NH-Add-Start (Radio job icons)
/// <summary>
/// Change the job icon prototype to override, or null to reset/not override.
/// </summary>
[Serializable, NetSerializable]
public sealed class VoiceMaskChangeJobIconMessage(ProtoId<JobIconPrototype>? icon) : BoundUserInterfaceMessage
{
    public readonly ProtoId<JobIconPrototype>? Icon = icon;
}
// NH-Add-End

/// <summary>
///     Toggle the effects of the voice mask.
/// </summary>
[Serializable, NetSerializable]
public sealed class VoiceMaskToggleMessage : BoundUserInterfaceMessage;

/// <summary>
///     Toggle the effects of accent negation.
/// </summary>
[Serializable, NetSerializable]
public sealed class VoiceMaskAccentToggleMessage : BoundUserInterfaceMessage;

/// <summary>
///  Fired when a voice mask is turned on.
/// </summary>
/// <param name=="Mask">The voice mask that was turned on</param>
/// <param name=="Source">The entity that owns the voice mask</param>
/// <param name=="Active">The new value of the voice mask</param>
public sealed class VoiceMaskToggledEvent(EntityUid mask, EntityUid source, bool active) : IInventoryRelayEvent
{
    public EntityUid Mask = mask;
    public EntityUid Source = source;

    public bool Active = active;

    SlotFlags IInventoryRelayEvent.TargetSlots => SlotFlags.WITHOUT_POCKET;
}
