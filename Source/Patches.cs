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
        public static void Postfix(Need_Food __instance, ref float __result)
        {
            Pawn pawn = __instance.pawn;
            if (pawn?.RaceProps == null) return;

            var s = RealisticRanchingMod.settings;
            float dietMult = 1f;

            // Apply the Diet Multiplier from Settings
            if (pawn.RaceProps.IsFlesh)
            {
                switch (pawn.RaceProps.ResolvedDietCategory)
                {
                    case DietCategory.Herbivorous: dietMult = s.hungerHerbivorous; break;
                    case DietCategory.Dendrivorous: dietMult = s.hungerDendrivorous; break;
                    case DietCategory.Omnivorous: dietMult = s.hungerOmnivorous; break;
                    case DietCategory.Carnivorous: dietMult = s.hungerCarnivorous; break;
                }
            }

            // Kleiber's Law Calculation
            // Using your Activity Multipliers: Carnivore (1.8), Herbivore (1.6), Omnivore (1.4)
            float activityMult = pawn.RaceProps.Carnivore ? 1.8f : (pawn.RaceProps.FoodHerbivore ? 1.6f : 1.4f);
            
            // Formula: (BodySize^0.75) * Activity * DietSetting
            // We divide by 1.6 to normalize it so a 'Human' (Size 1) remains roughly 1.0 base.
            __result = ((float)Math.Pow(pawn.BodySize, 0.75) * activityMult * dietMult) / 1.6f;
        }
    }

    [HarmonyPatch(typeof(Pawn_AgeTracker), "Tick")]
    public static class Age_Patch
    {
        public static void Postfix(Pawn_AgeTracker __instance, Pawn ___pawn)
        {
            if (___pawn.IsHashIntervalTick(60))
            {
                var s = RealisticRanchingMod.settings;
                float totalMult = s.ageingSpeedMultiplier;

                if (___pawn.RaceProps.Humanlike) totalMult *= s.humanAgeingSpeedMultiplier;
                else if (___pawn.RaceProps.Animal) totalMult *= s.animalAgeingSpeedMultiplier;

                if (totalMult > 1f)
                {
                    // Adding extra biological age ticks
                    long extraTicks = (long)((totalMult - 1f) * 60);
                    __instance.DebugSetAge(__instance.AgeBiologicalTicks + extraTicks);
                }
            }
        }
    }
}