using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FirstProto
{
    public abstract class Goal
    {
        public Condition PreCondition;
        public Action MainAction;
        public Action PostAction;
    }
}