using BepInEx;
using DeadlockMovementAPI.Contents;
using R2API;
using RoR2;
using System.Security.Permissions;
using System.Security;
using UnityEngine;
using UnityEngine.AddressableAssets;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace DeadlockMovementAPI
{

    // This one is because we use a .language file for language tokens
    // More info in https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Assets/Localization/
    [BepInDependency(LanguageAPI.PluginGUID)]

    // This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(MODUID, MODNAME, MODVERSION)]

    public class DeadlockMovementApiPlugin : BaseUnityPlugin
    {
        public const string MODUID = "com.vcr.DeadlockMovementAPI";
        public const string MODNAME = "DeadlockMovementAPI";
        public const string MODVERSION = "1.0.0";
        public const string BUNDLENAME = "deadlockassetbundle";

        // a prefix for name tokens to prevent conflicts- please capitalize all name tokens for convention
        public const string DEVELOPER_PREFIX = "VCR";

        public static DeadlockMovementApiPlugin instance;


        // The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            instance = this;

            //easy to use logger
            Log.Init(Logger);

            // used when you want to properly set up language folders
            Modules.Language.Init();

            new Contents.InitializeAssets().Initialize();

            Hooks();

            // make a content pack and add it. this has to be last
            new Modules.ContentPacks().Initialize();
        }

        public void Hooks()
        {
            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(DLBuffs.fauxMovementBuff))
            {
                args.moveSpeedTotalMult += 0.05f;
            }
        }
    }
}
