using HarmonyLib;
using Verse;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

namespace RanchWorld
{
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        public static AccessTools.FieldRef<Hediff, float> HediffSeverity =
            AccessTools.FieldRefAccess<Hediff, float>("severityInt");

        static HarmonyInit()
        {
            var harmony = new Harmony("Foo.RanchWorld");

            Log.Message("[RanchWorld] Starting Harmony patches...");

            // --- AGEING - THE CORRECT METHOD FOR RIMWORLD 1.6 ---
            MethodInfo ageTick = AccessTools.Method(typeof(Pawn_AgeTracker), "TickBiologicalAge");
            if (ageTick != null)
            {
                harmony.Patch(ageTick, new HarmonyMethod(typeof(RanchPatches), nameof(RanchPatches.TickBiologicalAgePrefix)));
                Log.Message("[RanchWorld] Successfully patched TickBiologicalAge");
            }
            else
            {
                Log.Error("[RanchWorld] Could not find TickBiologicalAge method!");
            }

            // --- METABOLISM & GROWTH ---
            TryPatchProperty(harmony, typeof(Need_Food), "FoodFallPerTick", nameof(RanchPatches.HungerPostfix));
            TryPatchProperty(harmony, typeof(Need_Food), "MaxLevel", nameof(RanchPatches.StomachPostfix));
            TryPatchProperty(harmony, typeof(Pawn_AgeTracker), "GrowthPointsPerTick", nameof(RanchPatches.GrowthPointsPostfix));

            // --- GESTATION ---
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

            // --- PRODUCTION ---
            TryPatchProperty(harmony, typeof(CompMilkable), "ResourceAmount", nameof(RanchPatches.ResourceOutputPostfix));
            TryPatchProperty(harmony, typeof(CompShearable), "ResourceAmount", nameof(RanchPatches.ResourceOutputPostfix));

            MethodInfo gatherTick = AccessTools.Method(typeof(CompHasGatherableBodyResource), "CompTick");
            if (gatherTick != null)
                harmony.Patch(gatherTick, new HarmonyMethod(typeof(RanchPatches), nameof(RanchPatches.GatherFreqPrefix)));

            MethodInfo butcher = AccessTools.Method(typeof(Pawn), nameof(Pawn.ButcherProducts));
            if (butcher != null)
                harmony.Patch(butcher, null, new HarmonyMethod(typeof(RanchPatches), nameof(RanchPatches.ButcherPostfix)));

            // --- COMBAT ---
            TryPatchProperty(harmony, typeof(Pawn), "HealthScale", nameof(RanchPatches.HealthScalePostfix));

            MethodInfo dmg = AccessTools.Method(typeof(VerbProperties), "AdjustedMeleeDamageAmount",
                new Type[] { typeof(Tool), typeof(Pawn), typeof(Thing), typeof(HediffComp_VerbGiver) });
            if (dmg != null)
                harmony.Patch(dmg, null, new HarmonyMethod(typeof(RanchPatches), nameof(RanchPatches.DamageScalePostfix)));

            Log.Message("[RanchWorld] All Harmony patches complete!");
        }

        static void TryPatchProperty(Harmony harmony, Type type, string prop, string patch)
        {
            if (type == null) return;
            MethodInfo method = AccessTools.PropertyGetter(type, prop);
            if (method != null)
            {
                harmony.Patch(method, null, new HarmonyMethod(typeof(RanchPatches), patch));
            }
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
        private static int debugCounter = 0;

        // --- THE CORRECT AGING METHOD FOR RIMWORLD 1.6 ---
        public static bool TickBiologicalAgePrefix(Pawn_AgeTracker __instance)
        {
            try
            {
                Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
                if (pawn == null) return true;

                // Get multiplier
                float mult = 1f;
                if (RanchWorldMod.settings != null)
                {
                    mult = RanchWorldMod.settings.baseGrowthMult;
                    if (pawn.RaceProps.Humanlike)
                        mult *= RanchWorldMod.settings.humanGrowthMult * RanchWorldMod.settings.humanAgeMult;
                    else if (pawn.RaceProps.Animal)
                        mult *= RanchWorldMod.settings.animalGrowthMult * RanchWorldMod.settings.animalAgeMult;
                }

                // Debug log occasionally
                debugCounter++;
                if (debugCounter % 1000 == 0 && pawn.IsColonist)
                {
                    Log.Message($"[RanchWorld] TickBiologicalAge for {pawn.LabelShort}: mult={mult:F2}");
                }

                if (mult <= 1.001f) return true; // Use vanilla

                // REPLACE VANILLA - add accelerated ticks
                int ticksToAge = Mathf.RoundToInt(mult);

                Traverse traverse = Traverse.Create(__instance);

                // Get current biological age
                long ageBioTicks = traverse.Field("ageBiologicalTicksInt").GetValue<long>();

                // Add the accelerated ticks to biological age
                long newBioTicks = ageBioTicks + ticksToAge;
                traverse.Field("ageBiologicalTicksInt").SetValue(newBioTicks);

                // ALSO update chronological age (important for humans)
                long ageChronoTicks = traverse.Field("ageChronologicalTicksInt").GetValue<long>();
                long newChronoTicks = ageChronoTicks + ticksToAge;
                traverse.Field("ageChronologicalTicksInt").SetValue(newChronoTicks);

                // Handle growth for animals
                if (pawn.RaceProps.Animal)
                {
                    float growth = traverse.Field("growthInt").GetValue<float>();
                    if (growth < 1f)
                    {
                        float lifeExpectancyTicks = pawn.RaceProps.lifeExpectancy * 3600000f;
                        float adultAgeTicks = lifeExpectancyTicks * 0.1f;

                        if (adultAgeTicks > 0)
                        {
                            float growthGain = (ticksToAge / adultAgeTicks);
                            float newGrowth = Mathf.Min(1f, growth + growthGain);
                            traverse.Field("growthInt").SetValue(newGrowth);
                        }
                    }
                }

                // Recalculate life stage
                traverse.Method("RecalculateLifeStageIndex").GetValue();

                // Check for biological birthdays
                long ageBioYears = newBioTicks / 3600000L;
                long prevAgeBioYears = ageBioTicks / 3600000L;

                if (ageBioYears > prevAgeBioYears)
                {
                    for (long i = prevAgeBioYears + 1; i <= ageBioYears; i++)
                    {
                        try
                        {
                            traverse.Method("BirthdayBiological", new object[] { (int)i }).GetValue();
                        }
                        catch
                        {
                            // Birthday method might not exist or have different signature
                        }
                    }
                }

                // Check for chronological birthdays (important for humans - affects skills, health, etc.)
                if (pawn.RaceProps.Humanlike)
                {
                    long ageChronoYears = newChronoTicks / 3600000L;
                    long prevAgeChronoYears = ageChronoTicks / 3600000L;

                    if (ageChronoYears > prevAgeChronoYears)
                    {
                        for (long i = prevAgeChronoYears + 1; i <= ageChronoYears; i++)
                        {
                            try
                            {
                                traverse.Method("BirthdayChronological", new object[] { (int)i }).GetValue();
                            }
                            catch
                            {
                                // Birthday method might not exist or have different signature
                            }
                        }
                    }
                }

                return false; // Skip vanilla execution
            }
            catch (Exception e)
            {
                Log.Error($"[RanchWorld] Error in TickBiologicalAgePrefix: {e}");
                return true;
            }
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

        // --- GROWTH POINTS ---
        public static void GrowthPointsPostfix(ref float __result, Pawn ___pawn)
        {
            if (___pawn == null || RanchWorldMod.settings == null) return;

            float mult = RanchWorldMod.settings.baseGrowthMult;
            if (___pawn.RaceProps.Humanlike)
                mult *= RanchWorldMod.settings.humanGrowthMult * RanchWorldMod.settings.humanAgeMult;
            else if (___pawn.RaceProps.Animal)
                mult *= RanchWorldMod.settings.animalGrowthMult * RanchWorldMod.settings.animalAgeMult;

            __result *= mult;
        }

        // --- PRODUCTION ---
        public static void ResourceOutputPostfix(CompHasGatherableBodyResource __instance, ref int __result)
        {
            if (RanchWorldMod.settings == null) return;

            Pawn pawn = (Pawn)AccessTools.Field(typeof(ThingComp), "parent").GetValue(__instance);
            if (pawn == null) return;

            float amount = 0f;
            if (__instance is CompMilkable)
                amount = __result * pawn.BodySize *
                         RanchWorldMod.settings.generalOutputMult *
                         RanchWorldMod.settings.milkOutputMult;
            else if (__instance is CompShearable)
                amount = __result * Mathf.Pow(pawn.BodySize, 2f / 3f) *
                         RanchWorldMod.settings.generalOutputMult *
                         RanchWorldMod.settings.woolOutputMult;
            else return;

            __result = Mathf.RoundToInt(amount);
        }

        public static void GatherFreqPrefix(CompHasGatherableBodyResource __instance)
        {
            if (RanchWorldMod.settings == null || RanchWorldMod.settings.gatherFreqMult <= 1f) return;
            if (!(__instance is CompMilkable) && !(__instance is CompShearable)) return;

            FieldInfo fullnessField = AccessTools.Field(typeof(CompHasGatherableBodyResource), "fullness");
            float f = (float)fullnessField.GetValue(__instance);
            fullnessField.SetValue(__instance, Mathf.Min(1f, f + (1f / 60000f) * (RanchWorldMod.settings.gatherFreqMult - 1f)));
        }

        // --- BUTCHER ---
        public static void ButcherPostfix(Pawn __instance, ref IEnumerable<Thing> __result)
        {
            if (RanchWorldMod.settings == null || __result == null || __instance == null) return;

            List<Thing> list = new List<Thing>();
            float size = Mathf.Max(__instance.BodySize, 0.01f);
            float lath = Mathf.Pow(size, -1f / 3f);

            foreach (Thing t in __result)
            {
                float count = t.stackCount;
                if (t.def.IsLeather)
                    count *= lath * RanchWorldMod.settings.leatherButcherMult * RanchWorldMod.settings.generalButcherMult;
                else if (t.def.IsMeat)
                    count *= RanchWorldMod.settings.meatButcherMult * RanchWorldMod.settings.generalButcherMult;
                else
                    count *= RanchWorldMod.settings.generalButcherMult;

                t.stackCount = GenMath.RoundRandom(count);
                if (t.stackCount > 0) list.Add(t);
            }
            __result = list;
        }

        // --- COMBAT ---
        public static void HealthScalePostfix(Pawn __instance, ref float __result)
        {
            if (__instance == null || RanchWorldMod.settings == null) return;
            if (__instance.RaceProps.Animal)
                __result = __instance.BodySize * RanchWorldMod.settings.healthScaleMult;
        }

        public static void DamageScalePostfix(VerbProperties __instance, Tool tool, Pawn attacker, ref float __result)
        {
            if (attacker == null || RanchWorldMod.settings == null) return;
            if (attacker.RaceProps.Animal)
                __result *= attacker.BodySize * RanchWorldMod.settings.damageScaleMult;
        }

        // --- METABOLISM ---
        public static void HungerPostfix(Need_Food __instance, ref float __result, Pawn ___pawn)
        {
            if (___pawn == null || RanchWorldMod.settings == null) return;

            float mult = RanchWorldMod.settings.generalHungerMult;
            if (___pawn.RaceProps.Humanlike)
                mult *= RanchWorldMod.settings.humanHungerMult;
            else if (___pawn.RaceProps.Animal)
                mult *= RanchWorldMod.settings.animalHungerMult;

            __result *= (float)Math.Pow(___pawn.BodySize, 0.75) * mult;
        }

        public static void StomachPostfix(Need_Food __instance, ref float __result, Pawn ___pawn)
        {
            if (___pawn == null || RanchWorldMod.settings == null) return;

            float mult = RanchWorldMod.settings.generalStomachMult;
            if (___pawn.RaceProps.Humanlike)
                mult *= RanchWorldMod.settings.humanStomachMult;
            else if (___pawn.RaceProps.Animal)
                mult *= RanchWorldMod.settings.animalStomachMult;

            __result = ___pawn.BodySize * mult;
        }
    }
}