using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FirstProto
{
    public class EatPlantAction : GOAPAction
    {
        private bool isEaten = false;
        private Plant targetPlant;

        /// <summary>
        /// When did we begin this action?
        /// </summary>
        private float startTime = 0;

        public float EatDuration = 3;

        public EatPlantAction()
        {
            AddPrecondition("isHungry", true);
            AddEffect("eatFood", true);
        }

        public override void Reset()
        {
            isEaten = false;
            targetPlant = null;
            startTime = 0;
        }

        public override bool IsDone()
        {
            return isEaten;
        }

        public override bool RequiresInRange
        {
            get
            {
                return true;
            }
        }

        public override bool CheckProceduralPrecondition(GameObject agent)
        {
            //  find the nearest plant
            Plant[] plants = FindObjectsOfType<Plant>();
            Plant closestPlant = null;
            float closestDistance = 0;

            foreach(var p in plants)
            {
                if(closestPlant == null)
                {
                    closestPlant = p;
                    closestDistance = (p.gameObject.transform.position - agent.transform.position).magnitude;
                }
                else
                {
                    var dist = (p.gameObject.transform.position - agent.transform.position).magnitude;
                    if(dist < closestDistance)
                    {
                        closestPlant = p;
                        closestDistance = dist;
                    }
                }
            }

            if (closestPlant != null)
            {
                targetPlant = closestPlant;
                Target = targetPlant.gameObject;
            }
            else
            {
                Target = null;
            }

            return closestPlant != null;
        }

        public override bool Perform(GameObject agent)
        {
            Debug.Log("EatPlantAction -- PERFORM");
            if(startTime == 0)
            {
                startTime = Time.time;
            }

            if(Time.time - startTime > EatDuration)
            {
                //  finished eating, yum yum
                isEaten = true;

                Destroy(Target);
                //  TODO aherrera : update w/ hunger benefits on agent
            }

            return true;
        }
    }
}
