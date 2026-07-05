using System;
using System.Collections.Generic;

namespace Godus.Core
{
    /// <summary>
    /// Lightweight, reusable finite state machine.
    /// Backbone for player controller and all enemy AI.
    /// Uses explicit states — no boolean flags.
    /// </summary>
    public class StateMachine<TState> where TState : Enum
    {
        private readonly Dictionary<TState, Action> _enterCallbacks = new();
        private readonly Dictionary<TState, Action> _updateCallbacks = new();
        private readonly Dictionary<TState, Action> _exitCallbacks = new();

        public TState CurrentState { get; private set; }
        public TState PreviousState { get; private set; }

        /// <summary>
        /// Register callbacks for a state. Call this in Awake/Start for each state.
        /// </summary>
        public void RegisterState(TState state, Action onEnter = null, Action onUpdate = null, Action onExit = null)
        {
            if (_enterCallbacks.ContainsKey(state))
            {
                UnityEngine.Debug.LogWarning($"[StateMachine] State {state} already registered. Overwriting.");
            }

            _enterCallbacks[state] = onEnter;
            _updateCallbacks[state] = onUpdate;
            _exitCallbacks[state] = onExit;
        }

        /// <summary>
        /// Transition to a new state. Fires exit on current, then enter on new.
        /// Does nothing if already in the requested state.
        /// </summary>
        public void ChangeState(TState newState)
        {
            if (EqualityComparer<TState>.Default.Equals(CurrentState, newState))
                return;

            // Exit current state
            _exitCallbacks.GetValueOrDefault(CurrentState)?.Invoke();

            PreviousState = CurrentState;
            CurrentState = newState;

            // Enter new state
            _enterCallbacks.GetValueOrDefault(newState)?.Invoke();
        }

        /// <summary>
        /// Call every frame (or physics frame) from the owner.
        /// </summary>
        public void Update()
        {
            _updateCallbacks.GetValueOrDefault(CurrentState)?.Invoke();
        }
    }
}
