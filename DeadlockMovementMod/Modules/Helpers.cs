using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DeadlockMovementAPI.Modules
{
    public static class Helpers
    {
        public static float GetEstimatedMomentum(Vector3 velocity, Vector3 referenceNormal)
        {
            Vector3 val = Vector3.ProjectOnPlane(velocity, referenceNormal);
            float num = Vector3.Dot(val, Vector3.down);
            float result = Mathf.Clamp(num * 2f, -1f, 1f);

            return result;
        }

        public static float GetEstimatedMomentum(Vector3 velocity, CharacterMotor motor)
        {
            return GetEstimatedMomentum(velocity, motor.estimatedGroundNormal);
        }

        public static float GetEstimatedMomentum(CharacterMotor motor)
        {
            return GetEstimatedMomentum(motor.velocity, motor.estimatedGroundNormal);
        }
    }
}
