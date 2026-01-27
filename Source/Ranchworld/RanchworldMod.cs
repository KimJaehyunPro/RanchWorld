using Verse;
using HarmonyLib;

namespace Ranchworld
{
    public class RanchworldMod : Mod
    {
        public static Harmony harmony;

        public RanchworldMod(ModContentPack content) : base(content)
        {
            harmony = new Harmony("ranchworld.core");
            harmony.PatchAll();
        }
    }
}
