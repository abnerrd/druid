using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FirstProto
{
    public abstract class Condition
    {
        private List<Action> PossibleActions;

        public abstract bool IsConditionFufilled();
    }

    public class InRange : Condition
    {
        public override bool IsConditionFufilled()
        {
            throw new NotImplementedException();
            //  TODO aherrera : 
        }
    }
}