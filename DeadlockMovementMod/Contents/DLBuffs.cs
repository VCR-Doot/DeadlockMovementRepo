using RoR2;
using UnityEngine;

namespace DeadlockMovementAPI.Contents
{
    public static class DLBuffs
    {
        public static BuffDef fauxMovementBuff; //This buff does nothing but indicate that the expanded movements are available

        public static BuffDef hiddenWallJumpBuff; // Hidden buff to detect when a has been detected


        public static void Init(AssetBundle assetBundle)
        {
            fauxMovementBuff = Modules.Content.CreateAndAddBuff("FauxMovementBuff",
             LegacyResourcesAPI.Load<BuffDef>("BuffDefs/HiddenInvincibility").iconSprite,
             Color.white,
             false,
             false);

            hiddenWallJumpBuff = Modules.Content.CreateAndAddBuff("HiddenWallJumpBuff",
                null,
                Color.white,
                true,
                false);
        }
    }
}
