using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//https://gamedevelopment.tutsplus.com/tutorials/goal-oriented-action-planning-for-a-smarter-ai--cms-20793

namespace FirstProto
{
    public abstract class GOAPAction : MonoBehaviour
    {
        public HashSet<KeyValuePair<string, object>> Preconditions { get; private set; }
        public HashSet<KeyValuePair<string, object>> Effects { get; private set; }

        public bool IsInRange { get; set; }

        public float Cost;

        public abstract bool IsDone();

        public GameObject Target;

        public GOAPAction()
        {
            Preconditions = new HashSet<KeyValuePair<string, object>>();
            Effects = new HashSet<KeyValuePair<string, object>>();
        }

        public void DoReset()
        {
            IsInRange = false;
            Target = null;
            Reset();
        }


        public abstract void Reset();

        public abstract bool Perform(GameObject agent);

        public virtual bool RequiresInRange { get { return true; } }

        public abstract bool CheckProceduralPrecondition(GameObject agent);

        public void AddPrecondition(string condition, object value)
        {
            Preconditions.Add(new KeyValuePair<string, object>(condition, value));
        }

        public void RemovePrecondition(string condition)
        {
            KeyValuePair<string, object> remove = default(KeyValuePair<string, object>);
            foreach(KeyValuePair<string, object> kvp in Preconditions)
            {
                if(kvp.Key.Equals(condition))
                {
                    remove = kvp;
                }
            }

            if(!default(KeyValuePair<string, object>).Equals(remove))
            {
                Preconditions.Remove(remove);
            }
        }

        public void AddEffect(string effect, object value)
        {
            Effects.Add(new KeyValuePair<string, object>(effect, value));
        }

        public void RemoveEffect(string effect)
        {
            KeyValuePair<string, object> remove = default(KeyValuePair<string, object>);
            foreach(KeyValuePair<string, object> kvp in Effects)
            {
                if(kvp.Key.Equals(effect))
                {
                    remove = kvp;
                }
            }

            if(!default(KeyValuePair<string, object>).Equals(remove))
            {
                Effects.Remove(remove);
            }
        }

    }
    
    public class GOAPPlanner
    {
        public Queue<GOAPAction> Plan(GameObject agent, HashSet<GOAPAction> availableActions, HashSet<KeyValuePair<string, object>> worldState, HashSet<KeyValuePair<string, object>> goal)
        {
            foreach(GOAPAction a in availableActions)
            {
                a.DoReset();
            }

            //  check which actions can run 
            var usableActions = new HashSet<GOAPAction>();
            foreach(GOAPAction a in availableActions)
            {
                if (a.CheckProceduralPrecondition(agent))
                {
                    usableActions.Add(a);
                }
            }

            //  we now have all actions that can run, stored in usableActions

            //  build tree and record leaf nodes that provide solution to goal
            HashSet<PlannerNode> leaves = new HashSet<PlannerNode>();

            //  build tree
            PlannerNode start = new PlannerNode(null, 0, worldState, null);
            bool success = BuildGraph(start, leaves, usableActions, goal);

            if(!success)
            {
                //  no plan created
                return null;
            }

            //  get cheapest leaf
            PlannerNode cheapest = null;
            foreach(PlannerNode l in leaves)
            {
                if(cheapest == null)
                {
                    cheapest = l;
                }
                else
                {
                    if(l.RunningCost < cheapest.RunningCost)
                    {
                        cheapest = l;
                    }
                }
            }

            //  get its node and work back through parents
            List<GOAPAction> result = new List<GOAPAction>();
            PlannerNode n = cheapest;
            while(n != null)
            {
                if(n.Action != null)
                {
                    //  slide action in the front
                    result.Insert(0, n.Action);
                }
                n = n.Parent;
            }
            //  we now have this action HashSet in correct order
            Queue<GOAPAction> queue = new Queue<GOAPAction>();
            foreach(GOAPAction a in result)
            {
                queue.Enqueue(a);
            }

            return queue;
        }

        private bool BuildGraph(PlannerNode parent, HashSet<PlannerNode> leaves, HashSet<GOAPAction> usableActions, HashSet<KeyValuePair<string, object>> goal)
        {
            bool foundOne = false;

            //  go through each action available at this node and see if we can use it here
            foreach(GOAPAction action in usableActions)
            {
                //  if the parent state has conditions for this action's preconditions, use it here
                if(InState(action.Preconditions, parent._state))
                {
                    //  apply action's effects to parent state
                    HashSet<KeyValuePair<string, object>> currentState = PopulateState(parent._state, action.Effects);
                    PlannerNode node = new PlannerNode(parent, parent.RunningCost + action.Cost, currentState, action);

                    if(InState(goal, currentState))
                    {
                        //  found solution!
                        leaves.Add(node);
                        foundOne = true;
                    }
                    else
                    {
                        //  not at a solution yet, so test all remaining actions and branch out the tree
                        HashSet<GOAPAction> subset = ActionSubset(usableActions, action);
                        bool found = BuildGraph(node, leaves, subset, goal);
                        if(found)
                        {
                            foundOne = true;
                        }
                    }
                }
            }

            return foundOne;
        }

        /// <summary>
        /// create a subset of actions excluding the removeMe one. Creates a new set
        /// </summary>
        /// <param name="actions"></param>
        /// <param name="removeMe"></param>
        /// <returns></returns>
        private HashSet<GOAPAction> ActionSubset(HashSet<GOAPAction> actions, GOAPAction removeMe)
        {
            HashSet<GOAPAction> subset = new HashSet<GOAPAction>();
            foreach(GOAPAction a in actions)
            {
                if(!a.Equals(removeMe))
                {
                    subset.Add(a);
                }
            }

            return subset;
        }

        /// <summary>
        /// Check that all items in 'test' are in 'state'. If just one doesn't match or is missing,
        /// return false.
        /// </summary>
        /// <param name="test"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        private bool InState(HashSet<KeyValuePair<string, object>> test, HashSet<KeyValuePair<string, object>> state)
        {
            bool allMatch = true;
            foreach(KeyValuePair<string, object> t in test)
            {
                bool match = false;
                foreach(KeyValuePair<string, object> s in state)
                {
                    if(s.Equals(t))
                    {
                        match = true;
                        break;
                    }
                }

                if(!match)
                {
                    allMatch = false;
                }
            }

            return allMatch;
        }

        private HashSet<KeyValuePair<string, object>> PopulateState(HashSet<KeyValuePair<string, object>> currentState, HashSet<KeyValuePair<string, object>> stateChange)
        {
            HashSet<KeyValuePair<string, object>> state = new HashSet<KeyValuePair<string, object>>();

            //  copy KVPs over as new objects
            foreach(KeyValuePair<string, object> s in currentState)
            {
                state.Add(new KeyValuePair<string, object>(s.Key, s.Value));
            }

            foreach(KeyValuePair<string, object> change in stateChange)
            {
                //  if the key exists int he current state, update the Vlaue
                bool exists = false;

                foreach(KeyValuePair<string, object> s in state)
                {
                    if(s.Equals(change))
                    {
                        exists = true;
                        break;
                    }
                }

                if(exists)
                {
                    state.RemoveWhere((KeyValuePair<string, object> kvp) => { return kvp.Key.Equals(change.Key); });
                    KeyValuePair<string, object> updated = new KeyValuePair<string, object>(change.Key, change.Value);
                    state.Add(updated);
                }
                else
                {
                    //  if it does not exist in current state, add it
                    state.Add(new KeyValuePair<string, object>(change.Key, change.Value));
                }
            }

            return state;
        }

        //  used to construct plan trees
        private class PlannerNode
        {
            public PlannerNode Parent;
            public float RunningCost;
            public HashSet<KeyValuePair<string, object>> _state;
            public GOAPAction Action;

            public PlannerNode(PlannerNode parent, float runningCost, HashSet<KeyValuePair<string, object>> state, GOAPAction action)
            {
                Parent = parent;
                RunningCost = runningCost;
                _state = state;
                Action = action;
            }
        }
    }
    
    /// <summary>
    /// Any agent that wants to use GOAP must implement this interface.
    /// 
    /// Provides information to GOAP planner about world state.
    /// </summary>
    public interface IGoap
    {
        HashSet<KeyValuePair<string, object>> GetWorldState();

        HashSet<KeyValuePair<string, object>> CreateGoalState();

        void PlanFailed(HashSet<KeyValuePair<string, object>> failedGoal);

        void ActionsCompleted();

        /**
	    * A plan was found for the supplied goal.
	    * These are the actions the Agent will perform, in order.
	    */
        void PlanFound(HashSet<KeyValuePair<string, object>> goal, Queue<GOAPAction> actions);

        void PlanAborted(GOAPAction aborter);

        bool MoveAgent(GOAPAction nextAction);
    }
}