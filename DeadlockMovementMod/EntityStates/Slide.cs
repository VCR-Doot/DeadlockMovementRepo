using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using DeadlockMovementAPI.Modules;
using UnityEngine;

namespace DeadlockMovementAPI.EntityStates
{
    public class Slide : GenericCharacterMain
    {
        // Speeds
        private float resultStart;
        private float finalSpeed;

        public float groundDecay = 3;
        public float upDecay = 6;
        public float downBuild = 3.25f;

        private Vector3 idealDirection;

        public float desiredMomentum = 0f;
        private float currentMomentum = 0f;
        private Vector3 travelDirection;

        public GameObject slideInstance;

        public override void OnEnter()
        {
            idealDirection = characterDirection.forward;

            GetModelAnimator().SetBool("isSliding", true);


            //PlayAnimation("FullBody, Override", "Slide", "Slide.playbackRate", duration);

            characterBody.isSprinting = true;

            //slideInstance = GameObject.Instantiate(DeadlockMovementApi.Survivors.DeadlockMovementApi.DeadlockMovementApiAssets.swordHitImpactEffect, characterBody.footPosition, Quaternion.identity, modelLocator.modelBaseTransform);

            base.OnEnter();


            resultStart = 1.2f * moveSpeedStat;

            currentMomentum = resultStart;


            RecalcSpeed();
        }

        public override void OnExit()
        {
            GetModelAnimator().SetBool("isSliding", false);
            GameObject.Destroy(slideInstance);
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            if (isAuthority)
            {
                if (KeyDownAuthority() || finalSpeed <= moveSpeedStat)
                {
                    ProcessJump();
                    outer.SetNextStateToMain();
                    return;
                }

                RecalcSpeed();

                travelDirection = (IdealDirection() * finalSpeed) * GetDeltaTime();

                characterMotor.rootMotion += travelDirection;
            }

            base.FixedUpdate();
        }

        public override void Update()
        {
            UpdateAimDirection();
            base.Update();
        }

        public void UpdateAimDirection()
        {
            base.characterDirection.moveVector = new Vector3(GetAimRay().direction.x, 0, GetAimRay().direction.z).normalized;
        }

        public bool KeyDownAuthority()
        {
            return inputBank.jump.down;
        }

        public Vector3 IdealDirection()
        {
            Vector2 vector = Util.Vector3XZToVector2XY(base.inputBank.moveVector);
            if (vector != Vector2.zero)
            {
                vector.Normalize();
                idealDirection = Vector3.RotateTowards(idealDirection, new Vector3(vector.x, 0f, vector.y).normalized, 2.25f * GetDeltaTime(), 1);
                slideInstance.transform.rotation = Quaternion.Euler(idealDirection);
            }

            return idealDirection;
        }
        public void RecalcSpeed()
        {
            if (Helpers.GetEstimatedMomentum(travelDirection, characterMotor) > 0.1f)
            {
                currentMomentum += downBuild * GetDeltaTime();
            }
            else if (Helpers.GetEstimatedMomentum(travelDirection, characterMotor) < -0.1f)
            {
                currentMomentum -= upDecay * GetDeltaTime();
            }
            else
            {
                currentMomentum -= groundDecay * GetDeltaTime();
            }

            finalSpeed = currentMomentum;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Stun;
        }

        // Disabled generic movement
        public override void HandleMovements()
        {
        }
    }
}
