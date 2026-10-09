using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Shared.StatusIcon;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client._NewHorizons.UserInterface.RichText;

public sealed partial class JobIconTag : IMarkupTagHandler
{
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private IEntityManager _entity = default!;

    public string Name => "jobicon";

    public bool TryCreateControl(MarkupNode node, [NotNullWhen(true)] out Control? control)
    {
        control = null;
        if (node.Closing)
            return false;

        if (!node.Value.TryGetString(out var iconId) || string.IsNullOrWhiteSpace(iconId))
            return false;

        if (!_prototype.TryIndex<JobIconPrototype>(iconId, out var jobIconProto))
            return false;

        var sprite = _entity.SystemOrNull<SpriteSystem>();
        if (sprite == null)
            return false;

        Texture texture;
        try
        {
            texture = sprite.Frame0(jobIconProto.Icon);
        }
        catch
        {
            return false;
        }

        var textureRect = new TextureRect
        {
            Texture = texture,
            TextureScale = new Vector2(2.5f, 2.5f),
            Stretch = TextureRect.StretchMode.KeepCentered,
            VerticalAlignment = Control.VAlignment.Center,
            Margin = new Thickness(0, 2f, 3f, 0),
            ToolTip = jobIconProto.LocalizedJobName,
        };

        control = textureRect;
        return true;
    }
}
