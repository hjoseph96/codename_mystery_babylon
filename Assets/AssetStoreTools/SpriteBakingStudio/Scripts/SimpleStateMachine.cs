using System;
using System.Collections.Generic;

namespace SBS
{
    public sealed class SimpleStateMachine<TState>
    {
        private class StateAction
        {
            public readonly Action OnUpdate;

            public StateAction(Action OnUpdate)
            {
                this.OnUpdate = OnUpdate;
            }
        }

        private readonly Dictionary<TState, StateAction> stateDictionary;
        private StateAction currentState;

        public TState CurrentState { get; private set; }

        public SimpleStateMachine()
        {
            stateDictionary = new Dictionary<TState, StateAction>();
        }

        public void AddState(TState label, Action OnUpdate)
        {
            stateDictionary[label] = new StateAction(OnUpdate);
        }

        public void ChangeState(TState newState)
        {
            currentState = stateDictionary[newState];
            CurrentState = newState;
        }

        public void Update()
        {
            if (currentState != null && currentState.OnUpdate != null)
            {
                currentState.OnUpdate();
            }
        }
    }
}
