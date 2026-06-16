using MidnightReturn.Utils;

namespace MidnightReturn.Player.States
{
    public abstract class PlayerState : State<PlayerController>
    {
        protected PlayerController    Player    => Owner;
        protected PlayerMovement      Movement  => Owner.Movement;
        protected PlayerInputHandler  Input     => Owner.Input;
        protected PlayerCombat        Combat    => Owner.Combat;

        protected PlayerState(PlayerController owner, string name) : base(owner, name) { }
    }
}
