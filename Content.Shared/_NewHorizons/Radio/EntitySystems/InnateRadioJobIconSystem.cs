using Content.Shared._NewHorizons.Radio.Components;
using Content.Shared.Chat;

namespace Content.Shared._NewHorizons.Radio.EntitySystems;

public sealed class InnateRadioJobIconSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InnateRadioJobIconComponent, TransformSpeakerNameEvent>(OnTransformSpeakerName);
    }

    private void OnTransformSpeakerName(Entity<InnateRadioJobIconComponent> ent, ref TransformSpeakerNameEvent args)
    {
        args.JobIcon = ent.Comp.Icon;
    }
}
