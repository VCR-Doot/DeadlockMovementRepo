using DeadlockMovementAPI.Modules;
using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using DeadlockMovementAPI.Modules;
using UnityEngine;
using UnityEngine.Networking;
using DeadlockMovementAPI.Contents;

namespace DeadlockMovementAPI.EntityStates
{
    public class DLMain : GenericCharacterMain
    {
        public float stopWatch;

        // Slide Calculations?
        public float desiredMomentum = 0f;


        public override void FixedUpdate()
        {
            WallJumpCheck();

            if (isAuthority)
            {

                if (stopWatch > 0.5f)
                {
                    if (!characterBody.HasBuff(DLBuffs.fauxMovementBuff))
                    {
                        characterBody.AddBuff(DLBuffs.fauxMovementBuff);
                    }
                    if (inputBank.sprint.justPressed)
                    {
                        TrySprintInput();
                    }
                }
                else
                {
                    if (characterBody.HasBuff(DLBuffs.fauxMovementBuff))
                    {
                        characterBody.RemoveBuff(DLBuffs.fauxMovementBuff);
                    }
                }

                if (characterBody && characterBody.isSprinting)
                {
                    stopWatch += GetDeltaTime();
                }
                else if (characterMotor.velocity.magnitude <= 0.5f || !characterBody.isSprinting) stopWatch = 0;
            }

            base.FixedUpdate();
        }

        public void TrySprintInput()
        {
            if (characterMotor.isGrounded)
            {
                if (moveSpeedStat >= 11 || Helpers.GetEstimatedMomentum(characterMotor) >= 0.5f)
                {
                    outer.SetNextState(new Slide());
                    return;
                }
            }
            else
            {
                outer.SetNextState(new Dash());
                return;
            }
        }

        public void WallJumpCheck()
        {
         
        }

        public override void HandleMovements()
        {
            if (useRootMotion)
            {
                if (hasCharacterMotor)
                {
                    base.characterMotor.moveDirection = Vector3.zero;
                }
                if (hasRailMotor)
                {
                    base.railMotor.inputMoveVector = moveVector;
                }
            }
            else
            {
                if (hasCharacterMotor)
                {
                    base.characterMotor.moveDirection = moveVector;
                }
                if (hasRailMotor)
                {
                    base.railMotor.inputMoveVector = moveVector;
                }
            }
            _ = base.isGrounded;
            if (!hasRailMotor && hasCharacterDirection && hasCharacterBody)
            {
                if (hasAimAnimator && aimAnimator.aimType == AimAnimator.AimType.Smart)
                {
                    Vector3 vector = ((moveVector == Vector3.zero) ? base.characterDirection.forward : moveVector);
                    float num = Vector3.Angle(aimDirection, vector);
                    float num2 = Mathf.Max(aimAnimator.pitchRangeMax + aimAnimator.pitchGiveupRange, aimAnimator.yawRangeMax + aimAnimator.yawGiveupRange);
                    base.characterDirection.moveVector = (((bool)base.characterBody && base.characterBody.shouldAim && num > num2) ? aimDirection : vector);
                }
                else
                {
                    base.characterDirection.moveVector = (((bool)base.characterBody && base.characterBody.shouldAim) ? aimDirection : moveVector);
                }
            }
            if (!base.isAuthority)
            {
                return;
            }
            ProcessJump();
            if (hasCharacterBody)
            {
                bool isSprinting = sprintInputReceived;

                if (stopWatch >= 0.5f)
                {
                    base.characterBody.isSprinting = true;

                    if (moveVector.magnitude <= 0.5f)
                    {
                        characterBody.isSprinting = false;
                    }
                }
                else
                {
                    if (moveVector.magnitude <= 0.5f)
                    {
                        isSprinting = false;
                    }
                    base.characterBody.isSprinting = isSprinting;
                }
            }
        } // Enable autosprint

        public bool NearWall()
        {
            Ray mond = new Ray(gameObject.transform.position, GetAimRay().direction.normalized);
            RaycastHit hit;

            return Util.CharacterSpherecast(gameObject, mond, 0.5f, out hit, 0.5f, LayerIndex.world.mask, QueryTriggerInteraction.Collide);
        } // Near wall checl?
    }
}
