using UnityEngine;
using MidnightReturn.Utils;
using MidnightReturn.Systems;

namespace MidnightReturn.Player.States
{
    // ══════════════════════════════════════════════════════════════════════════
    //  HourglassState — Death's Hourglass time-rewind ability.
    //
    //  Activated from CrouchState (Spell while crouching) or from
    //  JumpState/FallState (Down + Spell while airborne).
    //
    //  Reads the ring buffer recorded by PlayerController each FixedUpdate
    //  and replays 3 seconds of position history backwards over 0.7 real
    //  seconds (slow-mo rewind). HP is restored to where it was at the
    //  beginning of the recorded window.
    //
    //  Time.timeScale drops to 0.12x during rewind, restoring on exit.
    //  10-second cooldown prevents back-to-back use.
    // ══════════════════════════════════════════════════════════════════════════
    public class HourglassState : PlayerState
    {
        private const float REWIND_DURATION = 0.70f; // real seconds to sweep the full buffer
        private const float COOLDOWN        = 10f;

        private float _t;

        public HourglassState(PlayerController owner) : base(owner, "Hourglass") { }

        public override void OnEnter(State<PlayerController> prev)
        {
            _t = 0f;

            Player.IsRewinding      = true;
            Movement.IsSuppressed   = true;
            Player.HourglassCooldown = COOLDOWN;

            Time.timeScale = 0.12f;

            EventBus.Emit(new HourglassActivatedEvent { PlayerPos = Player.transform.position });
            EventBus.Emit(new ScreenFlashEvent { Color = new Color(0.9f, 0.75f, 0.1f), Duration = 0.5f });
            EventBus.Emit(new CameraShakeEvent { Intensity = 0.25f, Duration = 0.4f });

            VFXManager.Instance?.PlayDashBurst(Player.transform.position, 0f); // golden burst reuse
            Player.Animator?.CrossFadeInFixedTime("Idle", 0.04f);
        }

        public override void OnUpdate(float dt)
        {
            float udt = Time.unscaledDeltaTime;
            _t += udt;

            float progress   = Mathf.Clamp01(_t / REWIND_DURATION);
            int   ageFrames  = Mathf.FloorToInt(progress * (PlayerController.HOURGLASS_BUFFER - 1));

            if (Player.TryGetHourglassSnapshot(ageFrames, out var snap))
                Player.transform.position = snap.Pos;

            if (_t >= REWIND_DURATION)
            {
                // Restore HP to the state at the oldest snapshot (3 seconds ago)
                if (Player.TryGetHourglassSnapshot(PlayerController.HOURGLASS_BUFFER - 1, out var oldest))
                {
                    int delta = oldest.Hp - Player.Stats.Hp;
                    if (delta > 0) Player.Heal(delta);
                }

                Finish();
            }
        }

        public override void OnExit(State<PlayerController> next) => Finish();

        private void Finish()
        {
            Time.timeScale        = 1f;
            Movement.IsSuppressed = false;
            Player.IsRewinding    = false;
            if (Player.FSM.CurrentState != "Hourglass") return;
            Player.FSM.Transition("Idle");
        }
    }
}
