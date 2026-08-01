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
        public List<Vector3> directionChecks = new List<Vector3>();
        public List<Vector3> checkList1 = new List<Vector3>();
        public List<Vector3> checkList2 = new List<Vector3>();

        private Rewired.Player player;

        private InputBankTest.ButtonState sprintButton = new InputBankTest.ButtonState();

        public override void OnEnter()
        {
            base.OnEnter();

            player = characterBody.master.playerCharacterMasterController.networkUser.inputPlayer;

            characterBody.bodyFlags |= CharacterBody.BodyFlags.SprintAnyDirection;

            //
            //Level of detail:
            //0: north, east, south, west added (default)
            //1: northeast, southeast, southwest, northwest added
            //2: downward tilts added, effectively a cornerboost check
            // upward diagonals reserved for ledge-climb detection

            //Assumes a forward direction
            //List of rotation modifiers to a Raycast, each checking a direction

            directionChecks.Add(new Vector3(0f, 0f, 0f)); //North
            directionChecks.Add(new Vector3(0f, 90f, 0f)); //East
            directionChecks.Add(new Vector3(0f, 180f, 0f)); //South
            directionChecks.Add(new Vector3(0f, 270f, 0f)); //West

            checkList1.Add(new Vector3(0f, 45f, 0f)); //Northeast
            checkList1.Add(new Vector3(0f, 135f, 0f)); //Southeast
            checkList1.Add(new Vector3(0f, 225f, 0f)); //Southwest
            checkList1.Add(new Vector3(0f, 315f, 0f)); //Northwest

            checkList2.Add(new Vector3(-45f, 0f, 0f)); //Corner North
            checkList2.Add(new Vector3(-45f, 90f, 0f)); //Corner East
            checkList2.Add(new Vector3(-45f, 180f, 0f)); //Corner South
            checkList2.Add(new Vector3(-45f, 270f, 0f)); //Corner West

        }

        public override void FixedUpdate()
        {
            if (isAuthority)
            {
                if (characterMotor.Motor.GroundingStatus.IsStableOnGround && characterBody.HasBuff(DLBuffs.hiddenHasDashed))
                {
                    characterBody.RemoveBuff(DLBuffs.hiddenHasDashed);
                }

                if (stopWatch > 0.8f)
                {
                    if (!characterBody.HasBuff(DLBuffs.movementBuff))
                    {
                        characterBody.AddBuff(DLBuffs.movementBuff);
                    }
                }
                else
                {
                    if (characterBody.HasBuff(DLBuffs.movementBuff))
                    {
                        characterBody.RemoveBuff(DLBuffs.movementBuff);
                    }
                }

                if (sprintButton.justPressed && stopWatch >= 0.05f)
                {
                    TrySprintInput();
                }

                if (characterBody && characterBody.isSprinting)
                {
                    stopWatch += GetDeltaTime();
                }
                else if (characterMotor.velocity.magnitude <= 0.5f || !characterBody.isSprinting) stopWatch = 0;

                if (inputBank.jump.justPressed)
                {
                    bool wallJumpable = WallJumpCheck(out Vector3 wallNormal);
                    if (wallJumpable)
                    {
                        WallJump(wallNormal);
                    }
                }
            }

            base.FixedUpdate();
        }

        public override void GatherInputs()
        {
            base.GatherInputs();
            sprintButton.PushState(player.GetButton(18));
        }

        public void TrySprintInput()
        {
            if (characterMotor.isGrounded)
            {
                Log.Debug("Sprint Input call: " + Helpers.GetEstimatedMomentum(characterMotor));

                if (Helpers.GetEstimatedMomentum(characterMotor) >= 0.9f)
                {
                    outer.SetNextState(SlideState());
                    return;
                }
                else
                {
                    outer.SetNextState(RollState());
                    return;
                }
            }
            else
            {
                if (!characterBody.HasBuff(DLBuffs.hiddenHasDashed))
                {
                    characterBody.AddBuff(DLBuffs.hiddenHasDashed);
                    outer.SetNextState(DashState());
                    return;
                }
            }
        }

        //add parameter for level of detail return whether it's a wall jump
        public bool WallJumpCheck(out Vector3 wallNormal)
        {
            //Checking more directions, disabled to speed up compute times
            bool checkList1Enabled = false;
            bool checkList2Enabled = false;

            if (checkList1Enabled) 
                foreach (var wallcheck in checkList1)
                    directionChecks.Add(wallcheck);

            if (checkList2Enabled)
                foreach (var wallcheck in checkList2)
                    directionChecks.Add(wallcheck);

            //Check each node, if one is closer it becomes cached for final decision
            foreach (var check in directionChecks)
            {
                if (NearWall(check, out RaycastHit hit))
                {
                    wallNormal = hit.normal;
                    return true;
                }
            }
            wallNormal = default;
            return false;

        }
        
        public void WallJump(Vector3 wall = default)
        {

            //If have stamina bounce upwards | Queued for change after stamina additions
            ProcessJump();

            if (wall == new Vector3(0f, 0f, 0f))
            {
                Chat.AddMessage("Wall not defined in parametered call of WallJump()");
                Log.Message("[Warning] DLMain | Wall not defined in call of WallJump(), use ProcessJump() for basic jump calls");
                return;
            }

            characterMotor.velocity = ReflectOffSurface(wall);
        }

        public Vector3 ReflectOffSurface(Vector3 surface)
        {
            Vector3 currentVelocity = characterMotor.velocity;

            //DirectionBounced = Reflected 
            float movingToWall = Vector3.Dot(currentVelocity, -surface);
            Vector3 bouncedVelocity = currentVelocity;

            //1: Both Vectors in exact same direction
            //0: Vectors at a 90* angle from eachother (sideways)
            //-1: Vectors point in opposite directions
            if (movingToWall > 0f)
            {
                bouncedVelocity = Vector3.Reflect(currentVelocity, surface);
            }

            bouncedVelocity += surface;

            return bouncedVelocity;

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

                if (stopWatch >= 0.1f)
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


        //For outputting wall detection in wall jump
        public bool NearWall(Vector3 check, out RaycastHit hit)
        {
            var rayRotation = Quaternion.Euler(check.x, check.y, check.z) * GetAimRay().direction.normalized;
            Ray mond = new Ray(gameObject.transform.position, rayRotation);

            return Util.CharacterSpherecast(gameObject, mond, 0.5f, out hit, 0.5f, LayerIndex.world.mask, QueryTriggerInteraction.Collide);
        }

        //Overload for checking front only
        public bool NearWall(Vector3 check = default)
        {
            var rayRotation = Quaternion.Euler(check.x, check.y, check.z) * GetAimRay().direction.normalized;
            Ray mond = new Ray(gameObject.transform.position, rayRotation);
            RaycastHit hit;

            return Util.CharacterSpherecast(gameObject, mond, 0.5f, out hit, 0.5f, LayerIndex.world.mask, QueryTriggerInteraction.Collide);
        }


        public virtual EntityState SlideState()
        {
            return new Slide();
        }

        public virtual EntityState RollState() // This needs updated when Roll is added
        {
            return new Roll();
        }

        public virtual EntityState DashState()
        {
            return new Dash();
        }
    }
}
