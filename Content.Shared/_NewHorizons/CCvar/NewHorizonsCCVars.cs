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
    /// <summary>
    ///     Toggles displaying job icons in chat.
    /// </summary>
    public static readonly CVarDef<bool> ChatShowJobIcons =
        CVarDef.Create("chat.show_job_icons", true, CVar.CLIENTONLY | CVar.ARCHIVE);
}
