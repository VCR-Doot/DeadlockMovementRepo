using RoR2;
using UnityEngine;

namespace DeadlockMovementAPI.Contents
{
    public static class DLBuffs
    {
        public static BuffDef movementBuff; //Buff to indicate that the movement stuff is active and give a 10% total move speed multi

        public static BuffDef slideInfiniteAmmoBuff; //Buff to indicate and reference for infinite m1 stocks if applicible (m1 must have minimum 1 stockToConsume)

        public static BuffDef hiddenHasDashed; // Hidden buff to detect when a has been detected


        public static void Init(AssetBundle assetBundle)
        {
            movementBuff = Modules.Content.CreateAndAddBuff("DLMovementBuff",
                LegacyResourcesAPI.Load<BuffDef>("BuffDefs/HiddenInvincibility").iconSprite,
                Color.white,
                false,
                false);

            hiddenHasDashed = Modules.Content.CreateAndAddBuff("HiddenHasDashed",
               null,
               Color.white,
               false,
               true);
        }
    }
}
