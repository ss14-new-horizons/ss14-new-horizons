using Content.Shared.Administration;
using Content.Shared.CCVar;
using Content.Shared.CCVar.CVarAccess;
using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared._NewHorizons.CCVar;

/// <summary>
/// Contains all the CVars exclusive to New Horizons fork.
/// </summary>
[CVarDefs]
public sealed partial class NewHorizonsCCVars : CVars
{
    /*
     * Death Blackout
     */

    /// <summary>
    /// How long existing sounds continue echoing and fade out after death in seconds.
    /// </summary>
    public static readonly CVarDef<float> DeathBlackoutEchoDuration =
        CVarDef.Create("nh.death_blackout.echo_duration", 3.5f, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);

    /// <summary>
    /// How long the death blackout takes to smoothly fade out in seconds.
    /// </summary>
    public static readonly CVarDef<float> DeathBlackoutFadeOutDuration =
        CVarDef.Create("nh.death_blackout.fade_out_duration", 2.0f, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);
}
