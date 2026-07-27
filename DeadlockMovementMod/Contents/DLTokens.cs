using DeadlockMovementAPI.Modules;
using System;

namespace DeadlockMovementAPI.Contents
{
    public static class DLTokens
    {
        public static void Init()
        {
            AddDeadlockMovementApiTokens();

            ////uncomment this to spit out a lanuage file with all the above tokens that people can translate
            ////make sure you set Language.usingLanguageFolder and printingEnabled to true
            //Language.PrintOutput("DeadlockMovementApi.txt");
            ////refer to guide on how to build and distribute your mod with the proper folders
        }

        public static void AddDeadlockMovementApiTokens()
        {
            string prefix = DeadlockMovementApiPlugin.DEVELOPER_PREFIX;

            string keyword = "<style=cSub>Slide: Press sprint while grounded on incline or moving faster than 11 m/s to slide. This slide has momentum which decays and builds depending on slope.</style>" + Environment.NewLine + Environment.NewLine
                + "<style=cSub>Dash: Press sprint while mid-air to dash in input direction." + Environment.NewLine + Environment.NewLine
                + "<style=cSub>Dash-Jump: Timing Jump near the end of a dash will propel in dash direction." + Environment.NewLine + Environment.NewLine
                + "<style=cSub>Wall-Jump: TBD." + Environment.NewLine + Environment.NewLine;

            Language.Add(prefix + "DL_MISC_NAME", "Expanded Movement");
            Language.Add(prefix + "DL_MISC_DESCRIPTION", "This character has extra base movement options after 0.5 seconds of moving. Hover for more Information.");
            Language.Add(prefix + "KW_MISC", $"<style=cKeywordName>Movement Expansion</style>" + keyword);


        }
    }
}
