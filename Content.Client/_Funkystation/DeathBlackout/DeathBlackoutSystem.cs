using Content.Shared._Funkystation.CCVar;
using Content.Shared._Funkystation.DeathBlackout;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Client.Audio;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;
// NH-Add-Start (DeathBlackout)
using Content.Shared._NewHorizons.CCVar;
using Content.Shared.Audio;
using Robust.Shared.Audio.Components;
// NH-Add-End

namespace Content.Client._Funkystation.DeathBlackout;

public sealed partial class DeathBlackoutSystem : EntitySystem
{
    [Dependency] private IAudioManager _audio = null!;
    [Dependency] private IConfigurationManager _cfg = null!;
    [Dependency] private IGameTiming _timing = null!;
    [Dependency] private IPlayerManager _player = null!;
    [Dependency] private IUserInterfaceManager _ui = null!;
    [Dependency] private DamageableSystem _damage = null!;
    [Dependency] private MobThresholdSystem _thresholds = null!;

    [Dependency] private EntityQuery<DamageableComponent> _damageableQuery;
    [Dependency] private EntityQuery<DeathBlackoutComponent> _blackoutQuery;
    [Dependency] private EntityQuery<MobStateComponent> _mobStateQuery;
    [Dependency] private EntityQuery<MobThresholdsComponent> _thresholdsQuery;
    // NH-Add-Start (DeathBlackout)
    private bool _wasDead;
    private bool _wasInCrit;
    private bool _diedFromCrit;
    private TimeSpan _deathStartTime;
    private readonly HashSet<EntityUid> _deathAudioEntities = new();
    // NH-Add-End

    // how fast the crit fade catches up with the damage value
    private const float FadeCatchUp = 4f;

    private PanelContainer? _panel;
    private float _level;
    private bool _wasBlackout;
    private float _lastGain = -1f;
    private bool _muffled;

    // NH-Add-Start (DeathBlackout: ensure blackout audio update runs after engine audio system)
    public override void Initialize()
    {
        base.Initialize();
        UpdatesAfter.Add(typeof(AudioSystem));
    }
    // NH-Add-End

    public override void Shutdown()
    {
        base.Shutdown();

        RestoreAudio();

        // NH-Add-Start (DeathBlackout: restore audio volumes and clear death tracking)
        _deathAudioEntities.Clear();
        var restoreQuery = AllEntityQuery<AudioComponent>();
        while (restoreQuery.MoveNext(out var uid, out var audioComp))
        {
            if (audioComp.LifeStage >= ComponentLifeStage.Initialized && audioComp.LifeStage < ComponentLifeStage.Stopping)
            {
                try
                {
                    audioComp.Volume = audioComp.Params.Volume;
                }
                catch
                {
                    // Ignore
                }
            }
        }
        // NH-Add-End

        _panel?.Orphan();
        _panel = null;
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        // NH-Add-Start (DeathBlackout: track whether player was in critical state before dying)
        if (_player.LocalEntity is { } localPlayer &&
            _mobStateQuery.TryComp(localPlayer, out var localMobState))
        {
            if (localMobState.CurrentState == MobState.Critical)
                _wasInCrit = true;
            else if (localMobState.CurrentState == MobState.Alive)
                _wasInCrit = false;
        }
        // NH-Add-End

        var blackout = IsBlackout();
        var target = blackout ? 1f : GetCritFadeTarget();

        if (blackout)
        {
            // hard cut in
            _level = 1f;
        }
        // NH-Edit-Start (DeathBlackout: smoothly fade out blackout over configured duration instead of hard cut)
        else if (_level > target)
        {
            var fadeOutDuration = _cfg.GetCVar(NewHorizonsCCVars.DeathBlackoutFadeOutDuration);
            var fadeRate = frameTime / MathF.Max(0.01f, fadeOutDuration);
            _level = MathF.Max(target, _level - fadeRate);

            if (target <= 0f && _level < 0.001f)
                _level = 0f;
        }
        // NH-Edit-End
        else
        {
            // ease the crit fade
            _level += (target - _level) * MathF.Min(1f, frameTime * FadeCatchUp);

            if (target <= 0f && _level < 0.001f)
                _level = 0f;
        }

        _wasBlackout = blackout;

        UpdatePanel();
        UpdateAudio();
    }

    // true while the player is inside the post death blackout window
    // NH-Edit-Start (DeathBlackout: support out blackout component and seamless transition without split-second flicker)
    private bool IsBlackout(out DeathBlackoutComponent? blackout)
    {
        blackout = null;
        if (!_cfg.GetCVar(DeathBlackoutCVars.Enabled))
            return false;

        if (_player.LocalEntity is not { } player)
            return false;

        if (_blackoutQuery.TryComp(player, out blackout))
            return _timing.CurTime < blackout.EndTime;

        // If DeathBlackoutComponent has not arrived from server yet (e.g. during client prediction tick),
        // but the player is already dead, maintain blackout so there is no split-second flicker.
        if (_mobStateQuery.TryComp(player, out var mobState) && mobState.CurrentState == MobState.Dead)
            return true;

        return false;
    }

    private bool IsBlackout()
    {
        return IsBlackout(out _);
    }
    // NH-Edit-End

    // 0 to 1 progress through hard crit towards death
    private float GetCritFadeTarget()
    {
        if (!_cfg.GetCVar(DeathBlackoutCVars.Enabled) || !_cfg.GetCVar(DeathBlackoutCVars.CritFade))
            return 0f;

        if (_player.LocalEntity is not { } player)
            return 0f;

        if (!_mobStateQuery.TryComp(player, out var mobState) ||
            !_thresholdsQuery.TryComp(player, out var thresholds) ||
            !_damageableQuery.HasComp(player))
            return 0f;

        // species without softcrit fade over their normal crit
        if (mobState.CurrentState is not MobState.Critical)
            return 0f;

        if (!_thresholds.TryGetThresholdForState(player, mobState.CurrentState, out var start, thresholds) ||
            !_thresholds.TryGetDeadThreshold(player, out var dead, thresholds))
            return 0f;

        var range = (dead.Value - start.Value).Float();

        if (range <= 0f)
            return 0f;

        var progress = (_damage.GetTotalDamage(player) - start.Value).Float() / range; // how bout i do anyway
        return Math.Clamp(progress, 0f, 1f);
    }

    // black control sits in the popup root so it covers all the ui. this is a great idea and can have no negative repercussions :godo:
    private void UpdatePanel()
    {
        if (_level <= 0f)
        {
            if (_panel != null)
                _panel.Visible = false;

            return;
        }

        if (_panel == null)
        {
            _panel = new PanelContainer
            {
                Name = "DeathBlackout",
                MouseFilter = Control.MouseFilterMode.Ignore,
                PanelOverride = new StyleBoxFlat { BackgroundColor = Color.Black },
            };
            LayoutContainer.SetAnchorAndMarginPreset(_panel, LayoutContainer.LayoutPreset.Wide);
        }

        if (_panel.Parent == null)
            _ui.PopupRoot.AddChild(_panel);

        if (!_panel.Visible)
        {
            _panel.Visible = true;

            _panel.SetPositionInParent(_ui.PopupRoot.ChildCount - 1);
        }

        _panel.Modulate = Color.White.WithAlpha(_level);
    }

    // NH-Add-Start (DeathBlackout: filter out death rattle and environmental/ambient sounds)
    private bool IsSuppressedDeathAudio(EntityUid uid, AudioComponent audioComp, EntityUid? player, TransformComponent? xform)
    {
        string fileName = audioComp.FileName;

        // Только голосовой предсмертный хрип и звуки голоса персонажа (deathgasp, стоны, эмоуты)
        if (fileName.Contains("gasp", StringComparison.OrdinalIgnoreCase) ||
            fileName.Contains("/Voice/", StringComparison.OrdinalIgnoreCase) ||
            fileName.Contains("/Emotes/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Звуки окружения (фоновый шум станции, зацикленные звуки машин/вентиляции/эмбиента)
        var audioParams = audioComp.Params;
        if (audioParams.Loop)
            return true;

        if (fileName.Contains("ambience", StringComparison.OrdinalIgnoreCase) ||
            fileName.Contains("ambient", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (HasComp<AmbientSoundComponent>(uid))
            return true;

        if (xform != null)
        {
            if (HasComp<AmbientSoundComponent>(xform.ParentUid))
                return true;
        }
        else if (TryComp(uid, out TransformComponent? xf))
        {
            if (HasComp<AmbientSoundComponent>(xf.ParentUid))
                return true;
        }

        return false;
    }
    // NH-Add-End

    // scales master gain down with the level
    private void UpdateAudio()
    {
        // NH-Add-Start (DeathBlackout: death audio handling with echo, crit silence, and graceful fade restoration)
        var isDead = IsBlackout(out _);

        if (isDead)
        {
            if (!_wasDead)
            {
                _wasDead = true;
                _diedFromCrit = _wasInCrit;
                _deathStartTime = _timing.CurTime;
                _deathAudioEntities.Clear();

                if (_diedFromCrit)
                {
                    // If dying while already in critical state, sounds must NOT echo.
                    // Stop deathgasp immediately.
                    var gaspQuery = AllEntityQuery<AudioComponent>();
                    while (gaspQuery.MoveNext(out var uid, out var audioComp))
                    {
                        string fileName = audioComp.FileName;
                        if (fileName.Contains("gasp", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                audioComp.StopPlaying();
                            }
                            catch
                            {
                                // Ignore
                            }
                        }
                    }
                }
                else
                {
                    // Sudden death: preserve non-suppressed action audio to continue and fade out
                    var player = _player.LocalEntity;
                    var query = AllEntityQuery<AudioComponent, TransformComponent>();
                    while (query.MoveNext(out var uid, out var audioComp, out var xform))
                    {
                        string fileName = audioComp.FileName;
                        if (fileName.Contains("gasp", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                audioComp.StopPlaying();
                            }
                            catch
                            {
                                // Ignore
                            }
                            continue;
                        }

                        if (IsSuppressedDeathAudio(uid, audioComp, player, xform))
                            continue;

                        _deathAudioEntities.Add(uid);
                    }

                    var globalQuery = AllEntityQuery<AudioComponent>();
                    while (globalQuery.MoveNext(out var uid, out var audioComp))
                    {
                        if (_deathAudioEntities.Contains(uid))
                            continue;

                        string fileName = audioComp.FileName;
                        if (fileName.Contains("gasp", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                audioComp.StopPlaying();
                            }
                            catch
                            {
                                // Ignore
                            }
                            continue;
                        }

                        if (IsSuppressedDeathAudio(uid, audioComp, player, null))
                            continue;

                        _deathAudioEntities.Add(uid);
                    }
                }
            }

            // During blackout, mute deathgasp and any audio that is not part of the sudden death echo.
            // (We set Gain = 0f instead of StopPlaying so continuing/ambient sounds are not permanently destroyed)
            var allQuery = AllEntityQuery<AudioComponent>();
            while (allQuery.MoveNext(out var uid, out var audioComp))
            {
                if (audioComp.LifeStage < ComponentLifeStage.Initialized || audioComp.LifeStage >= ComponentLifeStage.Stopping)
                    continue;

                string fileName = audioComp.FileName;
                if (fileName.Contains("gasp", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        audioComp.StopPlaying();
                    }
                    catch
                    {
                        // Ignore
                    }
                    continue;
                }

                if (_diedFromCrit || !_deathAudioEntities.Contains(uid))
                {
                    try
                    {
                        audioComp.Gain = 0f;
                    }
                    catch
                    {
                        // Ignore
                    }
                }
            }

            float deathGain;
            if (_diedFromCrit)
            {
                deathGain = 0f;
            }
            else
            {
                var elapsed = (float)(_timing.CurTime - _deathStartTime).TotalSeconds;
                var echoDuration = _cfg.GetCVar(NewHorizonsCCVars.DeathBlackoutEchoDuration);

                if (elapsed < echoDuration)
                {
                    var progress = Math.Clamp(elapsed / echoDuration, 0f, 1f);
                    var fade = MathF.Pow(1f - progress, 2f);
                    deathGain = _cfg.GetCVar(CVars.AudioMasterVolume) * fade;
                }
                else
                {
                    deathGain = 0f;
                }
            }

            _muffled = true;

            if (MathF.Abs(deathGain - _lastGain) < 0.001f)
                return;

            _lastGain = deathGain;
            _audio.SetMasterGain(deathGain);
            return;
        }

        if (_wasDead)
        {
            _wasDead = false;
            _diedFromCrit = false;
            _deathAudioEntities.Clear();

            // Un-mute all audio components that were muted during blackout
            var restoreQuery = AllEntityQuery<AudioComponent>();
            while (restoreQuery.MoveNext(out var uid, out var audioComp))
            {
                if (audioComp.LifeStage >= ComponentLifeStage.Initialized && audioComp.LifeStage < ComponentLifeStage.Stopping)
                {
                    try
                    {
                        audioComp.Volume = audioComp.Params.Volume;
                    }
                    catch
                    {
                        // Ignore
                    }
                }
            }
        }
        // NH-Add-End

        if (_level <= 0f)
        {
            RestoreAudio();
            return;
        }

        // squared
        var gain = _cfg.GetCVar(CVars.AudioMasterVolume) * (1f - _level) * (1f - _level);

        _muffled = true;

        if (MathF.Abs(gain - _lastGain) < 0.001f)
            return;

        _lastGain = gain;
        _audio.SetMasterGain(gain);
    }

    // puts master gain back to whatever the volume cvar says
    private void RestoreAudio()
    {
        if (!_muffled)
            return;

        _muffled = false;
        _lastGain = -1f;
        _audio.SetMasterGain(_cfg.GetCVar(CVars.AudioMasterVolume));
    }
}
