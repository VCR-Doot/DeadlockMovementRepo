using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DeadlockMovementAPI.EntityStates
{
    public class Dash : BaseSkillState
    {
        public static float initialDashSpeed = 4f;
        public static float finalDashSpeed = 3f;
        public static float baseDuration = 0.51f;
        public static string soundString = "Play_Deadlock_Dash";

        public GameObject dustInstance;

        private float duration;

        float moveSpeed;
        Vector3 moveDir;


        public override void OnEnter()
        {
            base.OnEnter();

            duration = baseDuration;
            
            PlayDashAnimation();

            Util.PlaySound(soundString, gameObject);

            RecalculateMoveSpeed();
            moveDir = new Vector3(inputBank.moveVector.x, 0, inputBank.moveVector.z);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            RecalculateMoveSpeed();

            characterMotor.velocity = (moveDir * moveSpeed);

            if (fixedAge >= duration)
            {
                outer.SetNextState(new DLMain());
            }

        }

        private void RecalculateMoveSpeed()
        {
            moveSpeed = Mathf.Lerp(initialDashSpeed, finalDashSpeed, fixedAge / duration) * moveSpeedStat;
        }

        private void PlayDashAnimation()
        {
            if (characterMotor.isGrounded)
            {
                PlayAnimation("FullBody, Override", "Dash", "Special.PlaybackRate", duration);
            }
            else
            {
                PlayAnimation("FullBody, Override", "Dash_Air", "Special.PlaybackRate", duration);
            }
        }
    }
}
