using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FirstProto
{
    /// <summary>
    /// The 'brain' that uses the planner and data provided to 
    /// create a sequence of actions.
    /// 
    /// Needs a constant update for FSM.
    /// </summary>
    public sealed class GOAPAgent : MonoBehaviour
    {
        //  needs a small FSM to drive it
        private FSM _fsm;
        private FSM.FSMState _idleState;
        private FSM.FSMState _moveState;
        private FSM.FSMState _performState;

        private HashSet<GOAPAction> _availableActions;
        private Queue<GOAPAction> _currentActions;

        private IGoap _dataProvider;

        private GOAPPlanner _planner;

        private void Start()
        {
            _fsm = new FSM();
            _availableActions = new HashSet<GOAPAction>();
            _currentActions = new Queue<GOAPAction>();
            _planner = new GOAPPlanner();

            FindDataProvider();

            CreateIdleState();
            CreateMoveToState();
            CreatePerformActionState();

            _fsm.PushState(_idleState);

            LoadActions();
        }

        private void Update()
        {
            _fsm.Update(this.gameObject);
        }

        public void AddAction(GOAPAction action)
        {
            _availableActions.Add(action);
        }

        public GOAPAction GetAction(GOAPAction action)
        {
            foreach (GOAPAction a in _availableActions)
            {
                if (a.GetType().Equals(action.GetType()))
                {
                    return a;
                }
            }
            return null;
        }

        public void RemoveAction(GOAPAction action)
        {
            _availableActions.Remove(action);
        }

        private bool HasActionPlan()
        {
            return _currentActions.Count > 0;
        }

        private void CreateIdleState()
        {
            _idleState = (fsm, gameObject) =>
            {
                //  GOAP Planning -- What're you gonna do?!

                //  get world state and goal we want to plan for
                var worldState = _dataProvider.GetWorldState();
                var goal = _dataProvider.CreateGoalState();

                //  PLAN
                Queue<GOAPAction> plan = _planner.Plan(gameObject, _availableActions, worldState, goal);
                if (plan != null)
                {
                    //  plan created, success
                    _currentActions = plan;
                    _dataProvider.PlanFound(goal, plan);

                    _fsm.PopState();
                    _fsm.PushState(_performState);
                }
                else
                {
                    //  plan could not be created
                    _dataProvider.PlanFailed(goal);
                    _fsm.PopState();
                    _fsm.PushState(_idleState);
                }
            };
        }

        private void CreateMoveToState()
        {
            _moveState = (fsm, gameObject) =>
            {
                var action = _currentActions.Peek();
                if (action.RequiresInRange && action.Target == null)
                {
                    //  error? requires target, but target not found
                    //  return to IDLE
                    _fsm.PopOff();
                    _fsm.PushState(_idleState);
                    return;
                }

                //  get agent to move itself
                if (_dataProvider.MoveAgent(action))
                {
                    _fsm.PopState();
                }
            };
        }

        private void CreatePerformActionState()
        {
            _performState = (fsm, gameObject) =>
            {
                //  Perform the action
                if (!HasActionPlan())
                {
                    //  no actions left
                    _fsm.PopState();
                    _fsm.PushState(_idleState);
                    _dataProvider.ActionsCompleted();
                    return;
                }

                var action = _currentActions.Peek();
                if (action.IsDone())
                {
                    //  action completed, remove and continue w/ sequence
                    _currentActions.Dequeue();
                }

                if (HasActionPlan())
                {
                    action = _currentActions.Peek();

                    bool inRange = !action.RequiresInRange || action.IsInRange;
                    if (inRange)
                    {
                        bool actionSuccess = action.Perform(gameObject);

                        if (!actionSuccess)
                        {
                            //  action failed, plan again
                            _fsm.PopState();
                            _fsm.PushState(_idleState);
                            _dataProvider.PlanAborted(action);
                        }
                        else
                        {
                            //  we ened to move there first?
                            //  what?

                            _fsm.PushState(_moveState);
                        }
                    }
                }
                else
                {
                    //  no actions left
                    _fsm.PopState();
                    _fsm.PushState(_idleState);
                    _dataProvider.ActionsCompleted();
                }
            };
        }

        private void FindDataProvider()
        {
            foreach (Component comp in gameObject.GetComponents(typeof(Component)))
            {
                if (typeof(IGoap).IsAssignableFrom(comp.GetType()))
                {
                    _dataProvider = (IGoap)comp;
                    return;
                }
            }
        }

        private void LoadActions()
        {
            GOAPAction[] actions = gameObject.GetComponents<GOAPAction>();
            foreach (GOAPAction a in actions)
            {
                _availableActions.Add(a);
            }
        }

    }
}
