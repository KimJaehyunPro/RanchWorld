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

            // Patch 1: Hunger (Property Getter)
            var hungerGetter = AccessTools.PropertyGetter(typeof(Need_Food), "FoodFallPerTick");
            if (hungerGetter != null)
                harmony.Patch(hungerGetter, null, new HarmonyMethod(typeof(RanchPatches), nameof(RanchPatches.HungerPostfix)));

            // Patch 2: Ageing (Property Getter)
            var ageGetter = AccessTools.PropertyGetter(typeof(Pawn_AgeTracker), "BiologicalTicksPerTick");
            if (ageGetter != null)
                harmony.Patch(ageGetter, null, new HarmonyMethod(typeof(RanchPatches), nameof(RanchPatches.AgeingPostfix)));

            // Patch 3 & 4: Production (Patching Concrete Classes)
            // We patch the specific classes because the base property has no body.
            var milkGetter = AccessTools.PropertyGetter(typeof(CompMilkable), "ResourceAmount");
            if (milkGetter != null)
                harmony.Patch(milkGetter, null, new HarmonyMethod(typeof(RanchPatches), nameof(RanchPatches.OutputPostfix)));

            var woolGetter = AccessTools.PropertyGetter(typeof(CompShearable), "ResourceAmount");
            if (woolGetter != null)
                harmony.Patch(woolGetter, null, new HarmonyMethod(typeof(RanchPatches), nameof(RanchPatches.OutputPostfix)));

            Log.Message("[RanchWorld] All patches initialized successfully.");
        }
    }

    public static class RanchPatches
    {
        public static void HungerPostfix(Need_Food __instance, ref float __result, Pawn ___pawn)
        {
            if (___pawn == null || RanchWorldMod.settings == null) return;
            float massScaling = (float)Math.Pow(___pawn.BodySize, 0.75);
            __result *= massScaling * RanchWorldMod.settings.generalHungerMult;

            var race = ___pawn.RaceProps;
            if (race.Eats(FoodTypeFlags.Meat) && race.Eats(FoodTypeFlags.Plant))
                __result *= RanchWorldMod.settings.omnivoreHungerMult;
            else if (race.Eats(FoodTypeFlags.Meat))
                __result *= RanchWorldMod.settings.carnivoreHungerMult;
            else if (race.Eats(FoodTypeFlags.Plant))
                __result *= RanchWorldMod.settings.herbivoreHungerMult;
        }

        public static void AgeingPostfix(ref float __result, Pawn ___pawn)
        {
            if (___pawn == null || RanchWorldMod.settings == null) return;
            float speed = RanchWorldMod.settings.baseAgeSpeed;
            if (___pawn.RaceProps.Humanlike) speed *= RanchWorldMod.settings.humanAgeMult;
            else if (___pawn.RaceProps.Animal) speed *= RanchWorldMod.settings.animalAgeMult;
            __result = speed;
        }

        public static void OutputPostfix(CompHasGatherableBodyResource __instance, ref int __result)
        {
            if (RanchWorldMod.settings == null) return;
            float mult = RanchWorldMod.settings.generalOutputMult;
            if (__instance is CompMilkable) mult *= RanchWorldMod.settings.milkOutputMult;
            if (__instance is CompShearable) mult *= RanchWorldMod.settings.woolOutputMult;
            __result = (int)(__result * mult);
        }
    }
}