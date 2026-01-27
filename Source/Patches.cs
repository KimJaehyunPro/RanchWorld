using HarmonyLib;
using Verse;
using RimWorld;
using System;

namespace RealisticRanching
{
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        static HarmonyInit()
        {
            var harmony = new Harmony("YourName.RealisticRanching");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(Need_Food), "get_HungerRate")]
    public static class HungerRate_Patch
    {
        public static void Postfix(Need_Food __instance, ref float __result, Pawn ___pawn)
        {
            // Use ___pawn (3 underscores) because that is how it's defined in the arguments above
            if (___pawn?.RaceProps == null) return;

            var s = RealisticRanchingMod.settings;
            float dietMult = 1f;

            if (___pawn.RaceProps.IsFlesh)
            {
                switch (___pawn.RaceProps.ResolvedDietCategory)
                {
                    case DietCategory.Herbivorous: dietMult = s.hungerHerbivorous; break;
                    case DietCategory.Omnivorous: dietMult = s.hungerOmnivorous; break;
                    case DietCategory.Carnivorous: dietMult = s.hungerCarnivorous; break;
                }
            }

            // Logic check: Use ___pawn here as well
            float activityMult = 1.4f;
            if (___pawn.RaceProps.foodType.HasFlag(FoodTypeFlags.Meat) && !___pawn.RaceProps.foodType.HasFlag(FoodTypeFlags.Plant))
                activityMult = 1.8f;
            else if (___pawn.RaceProps.foodType.HasFlag(FoodTypeFlags.Plant))
                activityMult = 1.6f;

            // Formula: Use ___pawn.BodySize
            __result = ((float)Math.Pow(___pawn.BodySize, 0.75) * activityMult * dietMult) / 1.6f;
        }
    }

    [HarmonyPatch(typeof(Pawn_AgeTracker), "Tick")]
    public static class Age_Patch
    {
        public static void Postfix(Pawn_AgeTracker __instance, Pawn ___pawn)
        {
            // Accessing private field ___pawn via Harmony notation
            if (___pawn.IsHashIntervalTick(60))
            {
                var s = RealisticRanchingMod.settings;
                float totalMult = s.ageingSpeedMultiplier;

                if (___pawn.RaceProps.Humanlike) totalMult *= s.humanAgeingSpeedMultiplier;
                else if (___pawn.RaceProps.Animal) totalMult *= s.animalAgeingSpeedMultiplier;

                if (totalMult > 1f)
                {
                    long extraTicks = (long)((totalMult - 1f) * 60);
                    // Adds extra age biological ticks
                    __instance.DebugSetAge(__instance.AgeBiologicalTicks + extraTicks);
                }
            }
        }
    }
}