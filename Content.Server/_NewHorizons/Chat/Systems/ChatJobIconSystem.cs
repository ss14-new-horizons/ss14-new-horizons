using Content.Server.Access.Systems;
using Content.Shared._NewHorizons.Radio.EntitySystems;
using Content.Shared.Chat;
using Content.Shared.Mobs.Components;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;

namespace Content.Server._NewHorizons.Chat.Systems;

/// <summary>
/// Handles job icon assignment for entities talking in radio chat.
/// Respects InnateRadioJobIcon, falls back to ID card via IdCardSystem,
/// and sets JobIconNoId for mobs without an ID.
/// </summary>
public sealed partial class ChatJobIconSystem : EntitySystem
{
    [Dependency] private IdCardSystem _idCardSystem = default!;
    [Dependency] private IPrototypeManager _prototype = default!;

    private static readonly ProtoId<JobIconPrototype> JobIconNoId = "JobIconNoId";

    public override void Initialize()
    {
        base.Initialize();

        // Runs after InnateRadioJobIconSystem so innate radio icons (e.g. borgs, station AI) have priority.
        SubscribeLocalEvent<MobStateComponent, TransformSpeakerNameEvent>(OnTransformSpeakerName, after: [typeof(InnateRadioJobIconSystem)]);
    }

    private void OnTransformSpeakerName(Entity<MobStateComponent> ent, ref TransformSpeakerNameEvent args)
    {
        // If an innate icon (or other override) is already set, keep it.
        if (args.JobIcon != null)
            return;

        // Try finding ID card (the original method: checks inventory slot, PDA container, etc.)
        if (_idCardSystem.TryFindIdCard(ent.Owner, out var idCard) &&
            !string.IsNullOrEmpty(idCard.Comp.JobIcon.Id) &&
            idCard.Comp.JobIcon != "JobIconNoId" &&
            _prototype.HasIndex(idCard.Comp.JobIcon))
        {
            args.JobIcon = idCard.Comp.JobIcon;
            return;
        }

        // Living mobs without an ID card get JobIconNoId.
        args.JobIcon = JobIconNoId;
    }
}
