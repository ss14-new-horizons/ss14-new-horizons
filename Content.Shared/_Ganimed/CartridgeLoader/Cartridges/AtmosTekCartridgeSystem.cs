// SPDX-FileCopyrightText: 2024 ArchRBX <5040911+ArchRBX@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 Hyper B <137433177+HyperB1@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos.Components;
using Content.Shared.CartridgeLoader;

namespace Content.Shared._Ganimed.CartridgeLoader.Cartridges;

/// <summary>
/// Works the same way as the <see cref="MedTekCartridgeSystem"/>.
/// The program cannot be activated in the PDA UI.
/// You have to click on another object for the UI to pop-up separately.
/// </summary>
public sealed partial class AtmosTekCartridgeSystem : EntitySystem
{
    [Dependency] private CartridgeLoaderSystem _cartridgeLoaderSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AtmosTekCartridgeComponent, CartridgeAddedEvent>(OnCartridgeAdded);
        SubscribeLocalEvent<AtmosTekCartridgeComponent, CartridgeRemovedEvent>(OnCartridgeRemoved);
    }

    private void OnCartridgeAdded(Entity<AtmosTekCartridgeComponent> ent, ref CartridgeAddedEvent args)
    {
        EnsureComp<GasAnalyzerComponent>(args.Loader);
    }

    private void OnCartridgeRemoved(Entity<AtmosTekCartridgeComponent> ent, ref CartridgeRemovedEvent args)
    {
        // only remove when the program itself is removed
        if (!_cartridgeLoaderSystem.HasProgram<AtmosTekCartridgeComponent>(args.Loader.AsNullable()))
        {
            RemComp<GasAnalyzerComponent>(args.Loader);
        }
    }
}
