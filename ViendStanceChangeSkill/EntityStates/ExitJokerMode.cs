using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace EntityStates.VoidSurvivor.JokerMode
{
    public class ExitJokerMode : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            this.outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
