using DeadlockMovementAPI.Modules;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DeadlockMovementAPI.Contents
{
    public class InitializeAssets
    {
        public AssetBundle assetBundle { get; protected set; }


        public void Initialize()
        {
            assetBundle = Asset.LoadAssetBundle(DeadlockMovementApiPlugin.BUNDLENAME);

            DLStates.Init();
            DLTokens.Init();

            DLAssets.Init(assetBundle);
            DLBuffs.Init(assetBundle);
        }
    }
}
