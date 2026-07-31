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
        public float startCoefficient = 1.4f;

        public float groundDecay = 5;
        public float upDecay = 15;
        public float downBuild = 2.5f;

        private Vector3 idealDirection;

        private float currentMomentum = 0f;
        private Vector3 travelDirection;

        public GameObject slideInstance;


        private int cachedStocksToConsume = 0;

        private Rewired.Player player;


        public override void OnEnter()
        {

            idealDirection = characterDirection.forward;

            GetModelAnimator().SetBool("isSliding", true);

            //PlayAnimation("FullBody, Override", "Slide", "Slide.playbackRate", duration);

            characterBody.isSprinting = true;


            slideInstance = GameObject.Instantiate(DLAssets.slideEffect, characterBody.footPosition + new Vector3(0, 0.5f, 0), Quaternion.Euler(idealDirection.x, characterBody.footPosition.y + 0.1f, idealDirection.z), modelLocator.modelBaseTransform);

            base.OnEnter();

            player = characterBody.master.playerCharacterMasterController.networkUser.inputPlayer;


            // Call infinite ammo attempt for primaries
            TryGiveInfiniteAmmo();

            // Result start calculated as your input velocity magnitude (effectively a simpler means than using just movement stat as it's possible to move faster without affecting the stat itself)
            resultStart = startCoefficient * characterMotor.velocity.magnitude;

            currentMomentum = resultStart;

            RecalcChangeRate();
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
                if(finalSpeed > 2.5f) GetModelAnimator().SetBool("isSliding", characterMotor.isGrounded);
                else GetModelAnimator().SetBool("isSliding", false);


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


                if ((!player.GetButton(18) || finalSpeed <= 2) && isGrounded)
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

                // Adds a grace period for entering slide before rate can be reduced. Ideally this is moved into the function itself but for now this is a temp solution
                if (fixedAge > 0.1f)
                {
                    RecalcChangeRate();
                }

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

        public virtual void RecalcChangeRate()
        {
            Log.Debug("Slide estimated momentum: " + Helpers.GetEstimatedMomentum(characterMotor));


            if (Helpers.GetEstimatedMomentum(travelDirection, characterMotor) >= 0.1f)
            {
                currentMomentum += AdjustedRate(downBuild) * GetDeltaTime();
            }
            else if (Helpers.GetEstimatedMomentum(travelDirection, characterMotor) <= -0.6f)
            {
                currentMomentum -= upDecay * GetDeltaTime();
            }
            else
            {
                currentMomentum -= groundDecay * GetDeltaTime();
            }
            

            finalSpeed = Mathf.Clamp(currentMomentum, 0, Mathf.Infinity);
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

        [Tooltip("Attempt to fake infinite stocks for primaries that consume 1 or more on use.")]
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

        [Tooltip("Removes the fake Infinite stocks. This wont do anything unless TryGiveInfiniteAmmo cached the primary skilldef's stockToConsume variable.")]
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
