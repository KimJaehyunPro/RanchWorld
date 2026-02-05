using HarmonyLib;
using Verse;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.Linq;

namespace RanchWorld
{
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        // --- FIELD REFS (Direct access to private variables) ---
        public static AccessTools.FieldRef<Hediff, float> HediffSeverity =
            AccessTools.FieldRefAccess<Hediff, float>("severityInt");

        public static AccessTools.FieldRef<Pawn_AgeTracker, long> AgeBioTicks =
            AccessTools.FieldRefAccess<Pawn_AgeTracker, long>("ageBiologicalTicksInt");

        public static AccessTools.FieldRef<Pawn_AgeTracker, long> BirthAbsTicks =
            AccessTools.FieldRefAccess<Pawn_AgeTracker, long>("birthAbsTicksInt");

        public static AccessTools.FieldRef<Pawn_AgeTracker, Pawn> PawnRef =
            AccessTools.FieldRefAccess<Pawn_AgeTracker, Pawn>("pawn");

        // [FIX] Direct Field Ref for 'growth' instead of Property Setter
        public static AccessTools.FieldRef<Pawn_AgeTracker, float> GrowthField =
            AccessTools.FieldRefAccess<Pawn_AgeTracker, float>("growth");

        // --- DELEGATES ---
        public static Func<Pawn_AgeTracker, float> GetGrowthPointsPerTick;

        // --- METHOD REFS ---
        public static MethodInfo RecalculateLifeStageIndexMI =
            AccessTools.Method(typeof(Pawn_AgeTracker), "RecalculateLifeStageIndex");

        public static MethodInfo BirthdayBiologicalMI =
            AccessTools.Method(typeof(Pawn_AgeTracker), "BirthdayBiological");

        static HarmonyInit()
        {
            var harmony = new Harmony("Foo.RanchWorld");
            Log.Message("[RanchWorld] Starting Harmony patches...");

            // 1. REFLECTION SETUP
            try
            {
                PropertyInfo gptProp = AccessTools.Property(typeof(Pawn_AgeTracker), "GrowthPointsPerTick");
                if (gptProp != null)
                {
                    MethodInfo getter = gptProp.GetGetMethod(true);
                    if (getter != null) GetGrowthPointsPerTick = AccessTools.MethodDelegate<Func<Pawn_AgeTracker, float>>(getter);
                }
            }
            catch (Exception e)
            {
                Log.Warning($"[RanchWorld] Reflection Warning: {e.Message}");
            }

            // 2. AGEING PATCHES
            MethodInfo ageTick = AccessTools.Method(typeof(Pawn_AgeTracker), "TickBiologicalAge");
            if (ageTick != null)
                harmony.Patch(ageTick, new HarmonyMethod(typeof(RanchPatches), nameof(RanchPatches.TickBiologicalAgePrefix)));

            // 3. METABOLISM PATCHES
            TryPatchProperty(harmony, typeof(Need_Food), "FoodFallPerTick", nameof(RanchPatches.HungerPostfix));
            TryPatchProperty(harmony, typeof(Need_Food), "MaxLevel", nameof(RanchPatches.StomachPostfix));

            MethodInfo gpGetter = AccessTools.PropertyGetter(typeof(Pawn_AgeTracker), "GrowthPointsPerTick");
            if (gpGetter != null)
                harmony.Patch(gpGetter, null, new HarmonyMethod(typeof(RanchPatches), nameof(RanchPatches.GrowthPointsPostfix)));

            // 4. GESTATION PATCHES
            MethodInfo validTick = FindImplementedTick(typeof(Hediff_Pregnant));
            if (validTick != null)
                harmony.Patch(validTick, null, new HarmonyMethod(typeof(RanchPatches), nameof(RanchPatches.AnimalGestationTick)));

            Type humanPreg = AccessTools.TypeByName("RimWorld.Hediff_PregnantHuman");
            if (humanPreg != null)
            {
                MethodInfo humanTick = FindImplementedTick(humanPreg);
                if (humanTick != null)
                    harmony.Patch(humanTick, null, new HarmonyMethod(typeof(RanchPatches), nameof(RanchPatches.HumanGestationTick)));
            }

            // 5. PRODUCTION PATCHES
            TryPatchProperty(harmony, typeof(CompMilkable), "ResourceAmount", nameof(RanchPatches.ResourceOutputPostfix));
            TryPatchProperty(harmony, typeof(CompShearable), "ResourceAmount", nameof(RanchPatches.ResourceOutputPostfix));

            MethodInfo butcher = AccessTools.Method(typeof(Pawn), nameof(Pawn.ButcherProducts));
            if (butcher != null)
                harmony.Patch(butcher, null, new HarmonyMethod(typeof(RanchPatches), nameof(RanchPatches.ButcherPostfix)));

            Log.Message("[RanchWorld] Patches applied successfully.");
        }

        static void TryPatchProperty(Harmony harmony, Type type, string prop, string patch)
        {
            if (type == null) return;
            MethodInfo method = AccessTools.PropertyGetter(type, prop);
            if (method != null)
                harmony.Patch(method, null, new HarmonyMethod(typeof(RanchPatches), patch));
        }

        static MethodInfo FindImplementedTick(Type type)
        {
            while (type != null && type != typeof(object))
            {
                MethodInfo m = AccessTools.DeclaredMethod(type, "Tick");
                if (m != null) return m;
                type = type.BaseType;
            }
            return null;
        }
    }

    public static class RanchPatches
    {
        // --- AGEING (Includes the FIXED Animal Growth Logic) ---
        public static bool TickBiologicalAgePrefix(Pawn_AgeTracker __instance)
        {
            if (RanchWorldMod.settings == null) return true;

            Pawn pawn = HarmonyInit.PawnRef(__instance);
            if (pawn == null || pawn.Destroyed) return true;

            // 1. Calculate Multiplier
            float mult = RanchWorldMod.settings.baseGrowthMult;
            if (pawn.RaceProps.Humanlike)
                mult *= RanchWorldMod.settings.humanGrowthMult * RanchWorldMod.settings.humanAgeMult;
            else if (pawn.RaceProps.Animal)
                mult *= RanchWorldMod.settings.animalGrowthMult * RanchWorldMod.settings.animalAgeMult;

            if (mult <= 1.05f) return true;

            // 2. Add Extra Ticks
            int extraTicks = GenMath.RoundRandom(mult) - 1;
            if (extraTicks <= 0) return true;

            long currentBio = HarmonyInit.AgeBioTicks(__instance);
            long newBio = currentBio + extraTicks;

            HarmonyInit.AgeBioTicks(__instance) = newBio;
            HarmonyInit.BirthAbsTicks(__instance) -= extraTicks;

            // 3. FORCE GROWTH UPDATE [THE FIX]
            // We use the direct FieldRef 'HarmonyInit.GrowthField' which cannot be null.
            if (pawn.RaceProps.Humanlike)
            {
                if (HarmonyInit.GetGrowthPointsPerTick != null)
                {
                    float currentGrowth = HarmonyInit.GrowthField(__instance);
                    if (currentGrowth < 1f)
                    {
                        float points = HarmonyInit.GetGrowthPointsPerTick(__instance) * extraTicks;
                        HarmonyInit.GrowthField(__instance) = Mathf.Min(currentGrowth + points, 1f);
                    }
                }
            }
            else
            {
                // ANIMAL LOGIC
                var stages = pawn.RaceProps.lifeStageAges;
                if (stages != null && stages.Count > 0)
                {
                    // Find the age (in years) required to be fully mature
                    float adultAgeYears = stages[stages.Count - 1].minAge;

                    if (adultAgeYears > 0.01f)
                    {
                        float currentAgeYears = newBio / 3600000f;
                        // Force growth to match the new age ratio
                        float newGrowth = Mathf.Clamp01(currentAgeYears / adultAgeYears);
                        HarmonyInit.GrowthField(__instance) = newGrowth;
                    }
                }
            }

            // 4. Trigger Birthdays (Diseases/Events)
            // We invoke this manually for every year skipped so diseases trigger.
            if (HarmonyInit.BirthdayBiologicalMI != null)
            {
                long yearTicks = 3600000L;
                long oldAgeYears = currentBio / yearTicks;
                long newAgeYears = newBio / yearTicks;

                if (newAgeYears > oldAgeYears)
                {
                    for (long i = oldAgeYears + 1; i <= newAgeYears; i++)
                    {
                        HarmonyInit.BirthdayBiologicalMI.Invoke(__instance, new object[] { (int)i });
                    }
                }
            }

            // 5. Update Life Stage
            // Since GrowthField is now correct, this will finally work.
            if (HarmonyInit.RecalculateLifeStageIndexMI != null)
                HarmonyInit.RecalculateLifeStageIndexMI.Invoke(__instance, null);

            return true;
        }

        // --- GESTATION ---
        public static void AnimalGestationTick(Hediff __instance)
        {
            if (RanchWorldMod.settings == null || __instance.pawn == null) return;
            if (!(__instance is Hediff_Pregnant)) return;

            float mult = RanchWorldMod.settings.baseGrowthMult *
                         RanchWorldMod.settings.animalGrowthMult *
                         RanchWorldMod.settings.animalGestMult;

            if (mult <= 1f) return;

            float extra = (1f / (__instance.pawn.RaceProps.gestationPeriodDays * 60000f)) * (mult - 1f);
            HarmonyInit.HediffSeverity(__instance) += extra;
        }

        public static void HumanGestationTick(Hediff __instance)
        {
            if (RanchWorldMod.settings == null || __instance.pawn == null) return;

            float mult = RanchWorldMod.settings.baseGrowthMult *
                         RanchWorldMod.settings.humanGrowthMult *
                         RanchWorldMod.settings.humanGestMult;

            if (mult <= 1f) return;

            float extra = (1f / (__instance.pawn.RaceProps.gestationPeriodDays * 60000f)) * (mult - 1f);
            HarmonyInit.HediffSeverity(__instance) += extra;
        }

        // --- PRODUCTION ---
        public static void ResourceOutputPostfix(CompHasGatherableBodyResource __instance, ref int __result)
        {
            if (RanchWorldMod.settings == null) return;
            Pawn pawn = (Pawn)AccessTools.Field(typeof(ThingComp), "parent").GetValue(__instance);
            if (pawn == null) return;

            float amount = (__instance is CompMilkable)
                ? __result * pawn.BodySize * RanchWorldMod.settings.generalOutputMult * RanchWorldMod.settings.milkOutputMult
                : __result * Mathf.Pow(pawn.BodySize, 2f / 3f) * RanchWorldMod.settings.generalOutputMult * RanchWorldMod.settings.woolOutputMult;

            __result = Mathf.RoundToInt(amount);
        }

        // --- METABOLISM ---
        public static void HungerPostfix(ref float __result, Pawn ___pawn)
        {
            if (___pawn == null || RanchWorldMod.settings == null) return;
            float mult = RanchWorldMod.settings.generalHungerMult;
            mult *= (___pawn.RaceProps.Humanlike) ? RanchWorldMod.settings.humanHungerMult : RanchWorldMod.settings.animalHungerMult;
            __result *= (float)Math.Pow(___pawn.BodySize, 0.75) * mult;
        }

        public static void StomachPostfix(ref float __result, Pawn ___pawn)
        {
            if (___pawn == null || RanchWorldMod.settings == null) return;
            float mult = RanchWorldMod.settings.generalStomachMult;
            mult *= (___pawn.RaceProps.Humanlike) ? RanchWorldMod.settings.humanStomachMult : RanchWorldMod.settings.animalStomachMult;
            __result = ___pawn.BodySize * mult;
        }

        public static void GrowthPointsPostfix(ref float __result, Pawn ___pawn)
        {
            if (___pawn == null || RanchWorldMod.settings == null) return;
            float mult = RanchWorldMod.settings.baseGrowthMult;
            mult *= (___pawn.RaceProps.Humanlike) ? RanchWorldMod.settings.humanGrowthMult : RanchWorldMod.settings.animalGrowthMult;
            __result *= mult;
        }

        public static void ButcherPostfix(Pawn __instance, ref IEnumerable<Thing> __result)
        {
            if (RanchWorldMod.settings == null || __result == null || __instance == null) return;
            List<Thing> list = new List<Thing>();
            float size = Mathf.Max(__instance.BodySize, 0.01f);
            float lath = Mathf.Pow(size, -1f / 3f);

            foreach (Thing t in __result)
            {
                float count = t.stackCount;
                if (t.def.IsLeather) count *= lath * RanchWorldMod.settings.leatherButcherMult * RanchWorldMod.settings.generalButcherMult;
                else if (t.def.IsMeat) count *= RanchWorldMod.settings.meatButcherMult * RanchWorldMod.settings.generalButcherMult;
                else count *= RanchWorldMod.settings.generalButcherMult;

                t.stackCount = GenMath.RoundRandom(count);
                if (t.stackCount > 0) list.Add(t);
            }
            __result = list;
        }
    }
}