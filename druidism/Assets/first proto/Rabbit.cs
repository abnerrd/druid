using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FirstProto
{
    // For now, implementing LABOURER into this class
        
    public class Rabbit : MonoBehaviour, IGoap
    {
        int hunger = 20;
        int moveSpeed = 1;
        
        //  When we generalize this class into !Rabbit, this method will be abstract
        public HashSet<KeyValuePair<string, object>> CreateGoalState()
        {
            HashSet<KeyValuePair<string, object>> goal = new HashSet<KeyValuePair<string, object>>();

            goal.Add(new KeyValuePair<string, object>("eatFood", true));

            return goal;
        }

        public HashSet<KeyValuePair<string, object>> GetWorldState()
        {
            HashSet<KeyValuePair<string, object>> worldData = new HashSet<KeyValuePair<string, object>>();

            worldData.Add(new KeyValuePair<string, object>("isHungry", hunger <= 10));

            return worldData;
        }

        public bool MoveAgent(GOAPAction nextAction)
        {
            float step = moveSpeed * Time.deltaTime;
            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, nextAction.Target.transform.position, step);

            if(gameObject.transform.position.Equals(nextAction.Target.transform.position))
            {
                nextAction.IsInRange = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void PlanAborted(GOAPAction aborter)
        {

        }

        public void PlanFailed(HashSet<KeyValuePair<string, object>> failedGoal)
        {

        }

        public void PlanFound(HashSet<KeyValuePair<string, object>> goal, Queue<GOAPAction> actions)
        {

        }

        public void ActionsCompleted()
        {

        }
    }
}
