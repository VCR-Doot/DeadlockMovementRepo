using DeadlockMovementAPI.Contents;
using DeadlockMovementAPI.Modules;
using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DeadlockMovementAPI.EntityStates
{
    public class Slide : GenericCharacterMain
    {
        // Speeds
        private float resultStart;
        private float finalSpeed;

        public float groundDecay = 5;
        public float upDecay = 15;
        public float downBuild = 2.5f;

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


            slideInstance = GameObject.Instantiate(DLAssets.slideEffect, characterBody.footPosition, Quaternion.identity, modelLocator.modelBaseTransform);

            base.OnEnter();

            resultStart = 1.2f * moveSpeedStat;

            currentMomentum = resultStart; // Scale the starting speed with your movespeed * 1.2

            RecalcSpeed();
        }

        public override void OnExit()
        {
            GetModelAnimator().SetBool("isSliding", false);
            GameObject.Destroy(slideInstance);
            base.characterMotor.moveDirection = moveVector;
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            if (isAuthority)
            {

                if (inputBank.sprint.justPressed || (finalSpeed <= moveSpeedStat && characterMotor.isGrounded))
                {
                    outer.SetNextStateToMain();
                    return;
                }

                if (inputBank.jump.justPressed && characterMotor.jumpCount == 0)
                {
                    ApplyJumpVelocity(characterMotor, characterBody, 6f, 0.5f); // Arbetrary numbers off feel

                    if (Helpers.GetEstimatedMomentum(travelDirection, characterMotor) >= 0.01f) // Checks if going downhill before allowing extra momentum gain
                    {
                        currentMomentum += 4f;
                    }
                }

                RecalcSpeed();

                travelDirection = (IdealDirection() * finalSpeed) * GetDeltaTime();

                slideInstance.transform.rotation = Quaternion.Euler(travelDirection);

                characterMotor.rootMotion += travelDirection;
            }

            base.FixedUpdate();
        }

        public override void Update()
        {
            UpdateAimDirection();
            characterBody.isSprinting = true;
            base.Update();
        }

        public void UpdateAimDirection()
        {
            base.characterDirection.moveVector = new Vector3(GetAimRay().direction.x, 0, GetAimRay().direction.z).normalized;
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
        } // Updates the direction to be dampened over time
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

        public override void GatherInputs()
        {
            if (hasInputBank)
            {
                moveVector = base.inputBank.moveVector;
                aimDirection = base.inputBank.aimDirection;
                emoteRequest = base.inputBank.emoteRequest;
                base.inputBank.emoteRequest = -1;
                jumpInputReceived |= base.inputBank.jump.justPressed;
                jumpInputReceived &= !base.inputBank.jump.hasPressBeenClaimed;
                sprintInputReceived = base.inputBank.sprint.down;
            }
        }
    }
}
