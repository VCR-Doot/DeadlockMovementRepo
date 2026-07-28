using RoR2;
using UnityEngine;
using System;
using RoR2.Projectile;
using DeadlockMovementAPI.Modules;
using R2API;
using UnityEngine.AddressableAssets;

namespace DeadlockMovementAPI.Contents
{
    public static class DLAssets
    {
        // particle effects
        public static GameObject blueTrailEffect;
        public static GameObject slideEffect;
        public static GameObject dashEffect;

        // networked hit sounds
        public static NetworkSoundEventDef swordHitSoundEvent;

        private static AssetBundle _assetBundle;

        public static void Init(AssetBundle assetBundle)
        {

            _assetBundle = assetBundle;

            swordHitSoundEvent = Content.CreateAndAddNetworkSoundEventDef("DeadlockMovementApiSwordHit");

            CreateEffects();
        }

        #region effects
        private static void CreateEffects()
        {
            CreateDashEffects();
            CreateSlideEffects();
        }


        private static void CreateDashEffects()
        {

        }

        private static void CreateSlideEffects()
        {
            slideEffect = _assetBundle.LoadAsset<GameObject>("SlideVFX");
            MeshRenderer r = slideEffect.transform.GetChild(0).GetComponent<MeshRenderer>();

            //if (r)
            //{
            //    r.sharedMaterial = 
            //}
        }

        #endregion effects
    }
}
