using System;
using System.Collections.Generic;
using UnityEngine;

namespace FirstProto
{
    public class FSM
    {
        public delegate void FSMState(FSM fsm, GameObject gameObject);

        private readonly Stack<FSMState> _stateStack = new Stack<FSMState>();

        public void Update(GameObject gameObject)
        {
            if(_stateStack.Peek() != null)
            {
                _stateStack.Peek().Invoke(this, gameObject);
            }
        }

        public void PushState(FSMState state)
        {
            _stateStack.Push(state);
        }

        public void PopState()
        {
            _stateStack.Pop();
        }

        public void PopOff()
        {
            while(_stateStack.Peek() != null)
            {
                _stateStack.Pop();
            }
        }
    }
}