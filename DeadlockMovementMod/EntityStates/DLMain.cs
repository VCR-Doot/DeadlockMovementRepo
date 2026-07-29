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
        public List<Vector3> lod0 = new List<Vector3>();
        public List<Vector3> lod1 = new List<Vector3>();
        public List<Vector3> lod2 = new List<Vector3>();

        public override void OnEnter()
        {
            base.OnEnter();

            //
            //Level of detail:
            //0: north, east, south, west added (default)
            //1: northeast, southeast, southwest, northwest added
            //2: downward tilts added, effectively a cornerboost check
            // upward diagonals reserved for ledge-climb detection

            //Assumes a forward direction
            //List of rotation modifiers to a Raycast, each checking a direction

            lod0.Add(new Vector3(0f, 0f, 0f)); //North
            lod0.Add(new Vector3(0f, 90f, 0f)); //East
            lod0.Add(new Vector3(0f, 180f, 0f)); //South
            lod0.Add(new Vector3(0f, 270f, 0f)); //West

            lod1.Add(new Vector3(0f, 45f, 0f)); //Northeast
            lod1.Add(new Vector3(0f, 135f, 0f)); //Southeast
            lod1.Add(new Vector3(0f, 225f, 0f)); //Southwest
            lod1.Add(new Vector3(0f, 315f, 0f)); //Northwest

            lod2.Add(new Vector3(-45f, 0f, 0f)); //Corner North
            lod2.Add(new Vector3(-45f, 90f, 0f)); //Corner East
            lod2.Add(new Vector3(-45f, 180f, 0f)); //Corner South
            lod2.Add(new Vector3(-45f, 270f, 0f)); //Corner West

        }

        public override void FixedUpdate()
        {
            WallJumpCheck();

            if (isAuthority)
            {
                if (characterMotor.Motor.GroundingStatus.IsStableOnGround && characterBody.HasBuff(DLBuffs.hiddenHasDashed))
                {
                    characterBody.RemoveBuff(DLBuffs.hiddenHasDashed);
                }

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
                if (!characterBody.HasBuff(DLBuffs.hiddenHasDashed))
                {
                    characterBody.AddBuff(DLBuffs.hiddenHasDashed);
                    outer.SetNextState(new Dash());
                    return;
                }
            }
        }

        //add parameter for level of detail return whether it's a wall jump
        public RaycastHit WallJumpCheck()
        {
            //Goal: Get the nearest face, then average with adjacent faces, 
            bool lod1Checks = false;
            bool lod2Checks = false;

            //Adds each node from previous runs, better for simple wall bounce checking
            if (lod1Checks) 
                foreach (var wallcheck in lod1)
                    lod0.Add(wallcheck);

            if (lod2Checks)
                foreach (var wallcheck in lod2)
                    lod0.Add(wallcheck);

            bool foundWall = false;
            var closestDistance = 0.0f;
            RaycastHit closestWall = default; 

            //Check each node, if one is closer it becomes cached for final decision
            foreach (var check in lod0)
            {
                var checksRotation = Quaternion.Euler(check.x, check.y, check.z) * GetAimRay().direction.normalized;
                Ray mond = new Ray(gameObject.transform.position, checksRotation);
                RaycastHit hit;

                if(Util.CharacterSpherecast(gameObject, mond, 0.5f, out hit, 0.5f, LayerIndex.world.mask, QueryTriggerInteraction.Collide))
                {
                    foundWall = true;
                    if(hit.distance < closestDistance)
                        closestWall = hit;
                        closestDistance = hit.distance;
                }
            }
            if (foundWall) return closestWall;
            return default;

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
                if (base.hasAimAnimator && aimAnimator.aimType == AimAnimator.AimType.Smart)
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

        public bool NearWall(Vector3 check = default)
        {
            var rayRotation = Quaternion.Euler(check.x, check.y, check.z) * GetAimRay().direction.normalized;
            Ray mond = new Ray(gameObject.transform.position, rayRotation);
            RaycastHit hit;

            return Util.CharacterSpherecast(gameObject, mond, 0.5f, out hit, 0.5f, LayerIndex.world.mask, QueryTriggerInteraction.Collide);
        } // Near wall checl?
    }
}
