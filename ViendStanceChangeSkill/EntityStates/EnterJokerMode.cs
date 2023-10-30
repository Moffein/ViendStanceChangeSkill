using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using ViendStanceChangeSkill;

namespace EntityStates.VoidSurvivor.JokerMode
{
    public class EnterJokerMode : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            this.voidSurvivorController = base.GetComponent<VoidSurvivorController>();

            if (base.skillLocator && base.skillLocator.special)
            {
                //This is needed because the stock setting needs to be run after CorruptMode OnExit

                if (base.characterBody)
                {
                    ViendSkillDataComponent vc = base.characterBody.GetComponent<ViendSkillDataComponent>();
                    if (!vc) vc = base.characterBody.gameObject.AddComponent<ViendSkillDataComponent>();

                    vc.s1Stock = base.skillLocator.primary ? base.skillLocator.primary.stock : 0;
                    vc.s1RechargeStopwatch = base.skillLocator.primary ? base.skillLocator.primary.rechargeStopwatch : 0;

                    vc.s2Stock = base.skillLocator.secondary ? base.skillLocator.secondary.stock : 0;
                    vc.s2RechargeStopwatch = base.skillLocator.secondary ? base.skillLocator.secondary.rechargeStopwatch : 0;

                    vc.s3Stock = base.skillLocator.utility ? base.skillLocator.utility.stock : 0;
                    vc.s3RechargeStopwatch = base.skillLocator.utility ? base.skillLocator.utility.rechargeStopwatch : 0;

                    vc.s4Stock = base.skillLocator.special ? base.skillLocator.special.stock : 0;
                    vc.s4RechargeStopwatch = base.skillLocator.special ? base.skillLocator.special.rechargeStopwatch : 0;
                }

                base.skillLocator.special.SetSkillOverride(this, exitSkillDef, GenericSkill.SkillOverridePriority.Contextual);
            }
            if (base.isAuthority && this.voidSurvivorController && this.voidSurvivorController.bodyStateMachine)
            {
                this.voidSurvivorController.bodyStateMachine.SetInterruptState(new EnterCorruptionTransition(), InterruptPriority.Skill);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority)
            {
                GenericSkill skillSlot = base.skillLocator ? base.skillLocator.special : null;
                if (!skillSlot || skillSlot.stock <= 0)
                {
                    this.outer.SetNextStateToMain();
                }
            }
        }

        public override void OnExit()
        {
            if (base.isAuthority && this.voidSurvivorController && this.voidSurvivorController.bodyStateMachine)
            {
                this.voidSurvivorController.bodyStateMachine.SetInterruptState(new ExitCorruptionTransition(), InterruptPriority.Skill);
            }
            base.OnExit();
            if (base.skillLocator && base.skillLocator.special)
            {
                base.skillLocator.special.UnsetSkillOverride(this, exitSkillDef, GenericSkill.SkillOverridePriority.Contextual);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        private VoidSurvivorController voidSurvivorController;

        public static SkillDef exitSkillDef;
    }
}
