using UnityEngine;
using UnityEngine.InputSystem;

namespace MidnightReturn.Player
{
    // Uses Unity New Input System with sub-frame action buffering
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputHandler : MonoBehaviour
    {
        private const int BUFFER_FRAMES = 8;

        // Buffered action counters (count down each frame)
        public int JumpBuffer   { get; private set; }
        public int DashBuffer   { get; private set; }
        public int AttackBuffer { get; private set; }
        public int SpellBuffer  { get; private set; }

        // Held axes (from Input System callbacks)
        public Vector2 MoveAxis { get; private set; }
        public bool    JumpHeld { get; private set; }

        private PlayerInput _input;
        private InputAction _moveAction, _jumpAction, _dashAction, _attackAction, _spellAction, _interactAction;

        private void Awake()
        {
            _input        = GetComponent<PlayerInput>();
            var map       = _input.actions.FindActionMap("Player", true);
            _moveAction   = map.FindAction("Move",     true);
            _jumpAction   = map.FindAction("Jump",     true);
            _dashAction   = map.FindAction("Dash",     true);
            _attackAction = map.FindAction("Attack",   true);
            _spellAction  = map.FindAction("Spell",    true);
            _interactAction = map.FindAction("Interact", true);
        }

        private void OnEnable()
        {
            _jumpAction.performed    += _ => JumpBuffer    = BUFFER_FRAMES;
            _dashAction.performed    += _ => DashBuffer    = BUFFER_FRAMES;
            _attackAction.performed  += _ => AttackBuffer  = BUFFER_FRAMES;
            _spellAction.performed   += _ => SpellBuffer   = BUFFER_FRAMES;
            _interactAction.performed+= _ => _interactBuffer = BUFFER_FRAMES;
            _jumpAction.canceled     += _ => JumpHeld      = false;
            _jumpAction.performed    += _ => JumpHeld      = true;
        }

        private int _interactBuffer;

        private void Update()
        {
            MoveAxis = _moveAction.ReadValue<Vector2>();
            if (JumpBuffer      > 0) JumpBuffer--;
            if (DashBuffer      > 0) DashBuffer--;
            if (AttackBuffer    > 0) AttackBuffer--;
            if (SpellBuffer     > 0) SpellBuffer--;
            if (_interactBuffer > 0) _interactBuffer--;
        }

        public bool HasJump   => JumpBuffer   > 0;
        public bool HasDash   => DashBuffer   > 0;
        public bool HasAttack => AttackBuffer > 0;
        public bool HasSpell  => SpellBuffer  > 0;

        public void ConsumeJump()   => JumpBuffer   = 0;
        public void ConsumeDash()   => DashBuffer   = 0;
        public void ConsumeAttack() => AttackBuffer = 0;
        public void ConsumeSpell()  => SpellBuffer  = 0;

        public bool IsPressingLeft  => MoveAxis.x < -0.1f;
        public bool IsPressingRight => MoveAxis.x >  0.1f;
        public bool IsPressingDown  => MoveAxis.y < -0.1f;
        public bool IsPressingUp    => MoveAxis.y >  0.1f;
        public bool IsInteractJustDown => _interactAction.WasPerformedThisFrame();

        // Buffered consume — used by SaveStatue and future interactables
        public bool ConsumeInteract()
        {
            if (_interactBuffer <= 0) return false;
            _interactBuffer = 0;
            return true;
        }
    }
}
