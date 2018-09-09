using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FirstProto
{
    public abstract class Action
    {
        public abstract void DoAction();
    }

    public class MoveTo : Action
    {
        public Transform TargetLocation { get; private set; }

        public MoveTo(Transform target)
        {
            TargetLocation = target;
        }

        public MoveTo(GameObject target)
        {
            TargetLocation = target.transform;
        }

        public override void DoAction()
        {
            throw new NotImplementedException();
            //  TODO aherrera : move towards target
        }
    }

    public class SearchFor : Action
    {
        public SearchFor()
        {
            //  TODO aherrera : "what am I searching for?"
        }

        public override void DoAction()
        {
            throw new NotImplementedException();
            //  TODO aherrera : Scan the area for target
        }
    }
}
