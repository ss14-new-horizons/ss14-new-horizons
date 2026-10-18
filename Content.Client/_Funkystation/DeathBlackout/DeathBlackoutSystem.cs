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
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Effects;
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
    [Dependency] private AudioSystem _audioSystem = null!;

    private ISawmill _sawmill = default!;
    private bool _wasDead;
    private readonly HashSet<EntityUid> _deathAudioEntities = new();
    private EntityUid? _auxEntity;
    private EntityUid? _effectEntity;

    public override void Initialize()
    {
        base.Initialize();
        _sawmill = Logger.GetSawmill("death_blackout");
    }
    // NH-Add-End

    // how fast the crit fade catches up with the damage value
    private const float FadeCatchUp = 4f;

    private PanelContainer? _panel;
    private float _level;
    private bool _wasBlackout;
    private float _lastGain = -1f;
    private bool _muffled;

    public override void Shutdown()
    {
        base.Shutdown();

        RestoreAudio();

        // NH-Add-Start (DeathBlackout)
        CleanupAuxiliary();
        _deathAudioEntities.Clear();
        // NH-Add-End

        _panel?.Orphan();
        _panel = null;
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

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

    // true while player is dead and within blackout duration window
    private bool IsDeadWithBlackout(out DeathBlackoutComponent? blackout) // NH-Edit (IsBlackout() -> IsDeadWithBlackout(out DeathBlackoutComponent? blackout))
    {
        blackout = null; // NH-Add (DeathBlackout)
        if (!_cfg.GetCVar(DeathBlackoutCVars.Enabled))
            return false;

        if (_player.LocalEntity is not { } player)
            return false;

    // NH-Add-Start
        if (_blackoutQuery.TryComp(player, out blackout) && _timing.CurTime < blackout.EndTime)
            return true;

        return false;
    }
    // NH-Add-End

    // true while the player is inside the post death blackout window
    private bool IsBlackout()
    {
        // NH-Edit-Start (DeathBlackout: delay blackout appearance by configured duration)
        if (!IsDeadWithBlackout(out var blackout) || blackout == null)
            return false;

        var delay = TimeSpan.FromSeconds(_cfg.GetCVar(NewHorizonsCCVars.DeathBlackoutDelay));
        return _timing.CurTime >= blackout.StartTime + delay;
        // NH-Edit-End
    }

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

    // NH-Add-Start (DeathBlackout)
    private void EnsureEchoAuxiliary()
    {
        if (_auxEntity != null && !TerminatingOrDeleted(_auxEntity.Value))
            return;

        try
        {
            var (auxEnt, auxComp) = _audioSystem.CreateAuxiliary();
            var (effectEnt, effectComp) = _audioSystem.CreateEffect();
            _audioSystem.SetEffect(auxEnt, auxComp, effectEnt);

            var echoPreset = ReverbPresets.Generic with
            {
                Gain = 1.0f,
                DecayTime = 4.0f,
                EchoTime = 0.25f,
                EchoDepth = 0.85f,
                ReflectionsGain = 0.85f,
                LateReverbGain = 1.3f,
            };
            _audioSystem.SetEffectPreset(effectEnt, effectComp, echoPreset);

            _auxEntity = auxEnt;
            _effectEntity = effectEnt;
        }
        catch (Exception ex)
        {
            _sawmill.Warning($"Failed to initialize death blackout audio echo auxiliary: {ex.Message}");
            _auxEntity = null;
            _effectEntity = null;
        }
    }

    private void CleanupAuxiliary()
    {
        if (_effectEntity != null && !TerminatingOrDeleted(_effectEntity.Value))
        {
            _audioSystem.Stop(_effectEntity);
            Del(_effectEntity.Value);
        }
        _effectEntity = null;

        if (_auxEntity != null && !TerminatingOrDeleted(_auxEntity.Value))
        {
            if (TryComp<AudioAuxiliaryComponent>(_auxEntity.Value, out var auxComp))
            {
                auxComp.Auxiliary?.SetEffect(null);
                auxComp.Auxiliary?.Dispose();
            }
            Del(_auxEntity.Value);
        }
        _auxEntity = null;
    }
    // NH-Add-End

    // scales master gain down with the level
    private void UpdateAudio()
    {
        // NH-Add-Start (DeathBlackout)
        var isDead = IsDeadWithBlackout(out var blackout);

        if (isDead && blackout != null)
        {
            if (!_wasDead)
            {
                _wasDead = true;
                _deathAudioEntities.Clear();

                EnsureEchoAuxiliary();

                var query = AllEntityQuery<AudioComponent>();
                while (query.MoveNext(out var uid, out var audioComp))
                {
                    _deathAudioEntities.Add(uid);
                    if (_auxEntity != null)
                        _audioSystem.SetAuxiliary(uid, audioComp, _auxEntity);
                }
            }

            var elapsed = (float)(_timing.CurTime - blackout.StartTime).TotalSeconds;
            var echoDuration = _cfg.GetCVar(NewHorizonsCCVars.DeathBlackoutEchoDuration);

            float deathGain;
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
            _deathAudioEntities.Clear();
            CleanupAuxiliary();
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
