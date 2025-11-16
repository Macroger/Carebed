using Carebed.Infrastructure.Enums;

namespace Carebed.Models.Actuators
{
    public class StateMachine<TState> where TState : Enum
    {
        /// <summary>
        /// Private field to hold the current state.
        /// </summary>
        private TState _current;

        /// <summary>
        /// Dictionary to define valid state transitions.
        /// </summary>
        private readonly Dictionary<TState, TState[]> _transitions;

        /// <summary>
        /// Current state of the state machine.
        /// </summary>
        public TState Current => _current;


        public StateMachine(TState initial, Dictionary<TState, TState[]> transitions)
        {
            _current = initial;
            _transitions = transitions;
        }

        /// <summary>
        /// Tries to transition to the next state.
        /// </summary>
        /// <param name="next"> The next state to transition to.</param>
        /// <returns> bool: true if the transition was successful, false otherwise.</returns>
        public bool TryTransition(TState next)
        {
            if (_transitions.TryGetValue(_current, out var allowed) && allowed.Contains(next))
            {
                _current = next;
                return true;
            }
            return false;
        }
    }
}
