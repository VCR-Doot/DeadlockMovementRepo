using RoR2;
using UnityEngine;

namespace DeadlockMovementAPI.Contents
{
    public static class DLBuffs
    {
        public static BuffDef hiddenWallJumpBuff; // Hidden buff to detect when a has been detected


        public static void Init(AssetBundle assetBundle)
        {
            hiddenWallJumpBuff = Modules.Content.CreateAndAddBuff("HiddenWallJumpBuff",
                null,
                Color.white,
                true,
                false);
        }
    }
}
