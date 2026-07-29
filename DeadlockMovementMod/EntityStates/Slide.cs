using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using DeadlockMovementAPI.Modules;
using UnityEngine;
using DeadlockMovementAPI.Contents;
using UnityEngine.Networking;

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

        private int cachedStocksToConsume = 0;

        public override void OnEnter()
        {
            idealDirection = characterDirection.forward;

            GetModelAnimator().SetBool("isSliding", true);

            //PlayAnimation("FullBody, Override", "Slide", "Slide.playbackRate", duration);

            characterBody.isSprinting = true;


            slideInstance = GameObject.Instantiate(DLAssets.slideEffect, characterBody.footPosition + new Vector3(0, 0.5f, 0), Quaternion.Euler(idealDirection.x, characterBody.footPosition.y + 0.1f, idealDirection.z), modelLocator.modelBaseTransform);

            base.OnEnter();

            TryGiveInfiniteAmmo();

            resultStart = 1.4f * moveSpeedStat;

            currentMomentum = resultStart; // Scale the starting speed with your movespeed * 1.4

            RecalcSpeed();
        }

        public override void OnExit()
        {
            GetModelAnimator().SetBool("isSliding", false);
            GameObject.Destroy(slideInstance);
            base.characterMotor.moveDirection = moveVector;

            TryRemoveInfiniteAmmo();

            base.OnExit();
        }

        public override void FixedUpdate()
        {
            if (isAuthority)
            {

                GetModelAnimator().SetBool("isSliding", characterMotor.isGrounded);


                // Handling the deletion of slide instance
                if (!isGrounded && slideInstance)
                {
                    GameObject.Destroy(slideInstance);

                    TryRemoveInfiniteAmmo();
                }
                else if (isGrounded && !slideInstance)
                {
                    slideInstance = GameObject.Instantiate(DLAssets.slideEffect, characterBody.footPosition + new Vector3(0, 0.5f, 0), Quaternion.Euler(IdealDirection().x, characterBody.footPosition.y + 0.1f, IdealDirection().z), modelLocator.modelBaseTransform);

                    TryGiveInfiniteAmmo();
                }


                if (inputBank.sprint.justPressed || finalSpeed <= moveSpeedStat)
                {
                    outer.SetNextStateToMain();
                    return;
                }

                if (inputBank.jump.justPressed && characterMotor.jumpCount == 0)
                {
                    ApplyJumpVelocity(characterMotor, characterBody, 6f, 0.5f); // Arbetrary numbers off feel

                    if (Helpers.GetEstimatedMomentum(travelDirection, characterMotor) >= 0.01f) // Checks if going downhill or on flat ground before allowing extra momentum gain
                    {
                        currentMomentum += AdjustedRate(6f);
                    }
                }

                RecalcSpeed();

                travelDirection = (IdealDirection() * finalSpeed) * GetDeltaTime();


                if (slideInstance)
                {
                    slideInstance.transform.rotation = Quaternion.LookRotation(new Vector3(travelDirection.x, 0, travelDirection.z), transform.up);
                }

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
            }

            return idealDirection;
        } // Updates the direction to be dampened over time

        public void RecalcSpeed()
        {
            if (Helpers.GetEstimatedMomentum(travelDirection, characterMotor) > 0.1f)
            {
                currentMomentum += AdjustedRate(downBuild) * GetDeltaTime();
            }
            else if (Helpers.GetEstimatedMomentum(travelDirection, characterMotor) < -0.1f)
            {
                currentMomentum -= upDecay * GetDeltaTime();
            }
            else
            {
                if (isGrounded)
                {
                    currentMomentum -= groundDecay * GetDeltaTime();
                }
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

        private float AdjustedRate(float input)
        {
            return input * (moveSpeedStat/((characterBody.baseMoveSpeed * characterBody.sprintingSpeedMultiplier) * 1.1f));
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

        public void TryGiveInfiniteAmmo()
        {
            if (skillLocator.primary.skillDef.stockToConsume > 0)
            {
                if (NetworkServer.active)
                {
                    characterBody.AddBuff(DLBuffs.slideInfiniteAmmoBuff);
                }
                cachedStocksToConsume = skillLocator.primary.skillDef.stockToConsume;
                skillLocator.primary.skillDef.stockToConsume = 0;
            }

        }

        public void TryRemoveInfiniteAmmo()
        {
            if (cachedStocksToConsume > 0)
            {
                if (NetworkServer.active)
                {
                    characterBody.RemoveBuff(DLBuffs.slideInfiniteAmmoBuff);
                }
                skillLocator.primary.skillDef.stockToConsume = cachedStocksToConsume;
            }
        }
    }
}
