namespace Godus.Player
{
    /// <summary>
    /// Player finite states. Used with Godus.Core.StateMachine.
    /// Explicit states per the design doc — no boolean flags.
    /// </summary>
    public enum PlayerState
    {
        Idle,
        Run,
        Attack,
        Dash,
        Hurt,
        Dead
    }
}
