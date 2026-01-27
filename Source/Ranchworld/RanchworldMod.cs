using Verse;
using HarmonyLib;

namespace RanchWorld
{
    [StaticConstructorOnStartup]
    public static class RanchWorldInit
    {
        static RanchWorldInit()
        {
            Log.Message("[RanchWorld] Loaded");
        }
    }
}
