using HarmonyLib;
using Verse;
using RimWorld;
using System;

namespace RanchWorld
{
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        static HarmonyInit()
        {
            var harmony = new Harmony("Foo.RanchWorld");

            // We target 'FoodFallPerTick' because it's the property the game uses to drain food.
            var original = AccessTools.PropertyGetter(typeof(Need_Food), "FoodFallPerTick");
            var postfix = new HarmonyMethod(typeof(HungerRate_Patch), nameof(HungerRate_Patch.Postfix));

            if (original != null)
            {
                harmony.Patch(original, null, postfix);
                Log.Message("[RanchWorld] Successfully patched FoodFallPerTick.");
            }
            else
            {
                Log.Error("[RanchWorld] CRITICAL: Could not find FoodFallPerTick in Need_Food!");
            }
        }
    }

    public static class HungerRate_Patch
    {
        public static void Postfix(Need_Food __instance, ref float __result, Pawn ___pawn)
        {
            // The ___pawn field is inherited from the base 'Need' class.
            if (___pawn == null || RanchWorldMod.settings == null) return;

            // Kleiber's Law: Metabolic Rate = (Mass^0.75)
            // We use the pawn's BodySize as the Mass (M).
            double mass = (double)___pawn.BodySize;
            float metabolicScaling = (float)Math.Pow(mass, 0.75);

            // We multiply the game's calculated result by our scaling and the user's multiplier.
            // This preserves vanilla factors (like health/traits) while adding Kleiber's Law.
            __result *= metabolicScaling * RanchWorldMod.settings.hungerMultiplier;
        }
    }
}