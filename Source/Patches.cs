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
        // FIX 1: Use 'severityInt' (Base class field) to prevent MissingFieldException
        public static AccessTools.FieldRef<Hediff, float> HediffSeverity =
            AccessTools.FieldRefAccess<Hediff, float>("severityInt");

        static HarmonyInit()
        {
            Log.Message("[RanchWorld] Init: Starting...");
            var harmony = new Harmony("Foo.RanchWorld");

            // --- METABOLISM ---
            TryPatchProperty(harmony, typeof(Need_Food), "FoodFallPerTick", nameof(RanchPatches.HungerPostfix));
            TryPatchProperty(harmony, typeof(Need_Food), "MaxLevel", nameof(RanchPatches.StomachPostfix));

            // --- AGEING (Force Update) ---
            MethodInfo ageTick = AccessTools.Method(typeof(Pawn_AgeTracker), "AgeTick");
            if (ageTick != null)
                harmony.Patch(ageTick, null, new HarmonyMethod(typeof(RanchPatches), nameof(RanchPatches.AgeTickPostfix)));

            // --- LEARNING (Biotech) ---
            TryPatchProperty(harmony, typeof(Pawn_AgeTracker), "GrowthPointsPerTick", nameof(RanchPatches.GrowthPointsPostfix));

            // --- GESTATION (HIERARCHY FIX) ---
            // We search for the Tick method starting at the Child and moving up to the Parent/Grandparent.
            // This solves the "ArgumentException: You can only patch implemented methods" error.
            MethodInfo validAnimalTick = FindImplementedTick(typeof(Hediff_Pregnant));
            if (validAnimalTick != null)
            {
                Log.Message($"[RanchWorld] Patching Animal Gestation on: {validAnimalTick.DeclaringType.Name}.Tick");
                harmony.Patch(validAnimalTick, null, new HarmonyMethod(typeof(RanchPatches), nameof(RanchPatches.AnimalGestationTick)));
            }

            // Human Pregnancy (Biotech Check)
            Type humanType = AccessTools.TypeByName("RimWorld.Hediff_PregnantHuman");
            if (humanType != null)
            {
                MethodInfo validHumanTick = FindImplementedTick(humanType);
                if (validHumanTick != null && validHumanTick != validAnimalTick) // Don't double-patch if they share the same base method
                {
                    Log.Message($"[RanchWorld] Patching Human Gestation on: {validHumanTick.DeclaringType.Name}.Tick");
                    harmony.Patch(validHumanTick, null, new HarmonyMethod(typeof(RanchPatches), nameof(RanchPatches.HumanGestationTick)));
                }
            }

            // --- PRODUCTION ---
            TryPatchProperty(harmony, typeof(CompMilkable), "ResourceAmount", nameof(RanchPatches.ResourceOutputPostfix));
            TryPatchProperty(harmony, typeof(CompShearable), "ResourceAmount", nameof(RanchPatches.ResourceOutputPostfix));

            MethodInfo gatherTick = AccessTools.Method(typeof(CompHasGatherableBodyResource), "CompTick");
            if (gatherTick != null)
                harmony.Patch(gatherTick, new HarmonyMethod(typeof(RanchPatches), nameof(RanchPatches.GatherFreqPrefix)));

            // --- BUTCHER ---
            MethodInfo butcher = AccessTools.Method(typeof(Pawn), nameof(Pawn.ButcherProducts));
            if (butcher != null)
                harmony.Patch(butcher, null, new HarmonyMethod(typeof(RanchPatches), nameof(RanchPatches.ButcherPostfix)));

            // --- COMBAT ---
            TryPatchProperty(harmony, typeof(Pawn), "HealthScale", nameof(RanchPatches.HealthScalePostfix));

            MethodInfo damageMethod = AccessTools.Method(typeof(VerbProperties), "AdjustedMeleeDamageAmount",
                new Type[] { typeof(Tool), typeof(Pawn), typeof(Thing), typeof(HediffComp_VerbGiver) });
            if (damageMethod != null)
                harmony.Patch(damageMethod, null, new HarmonyMethod(typeof(RanchPatches), nameof(RanchPatches.DamageScalePostfix)));

            Log.Message("[RanchWorld] Init: Complete.");
        }

        // Helper: Safe Property Patcher
        static void TryPatchProperty(Harmony harmony, Type type, string propName, string patchName)
        {
            if (type == null) return;
            MethodInfo method = AccessTools.PropertyGetter(type, propName);
            if (method != null)
                harmony.Patch(method, null, new HarmonyMethod(typeof(RanchPatches), patchName));
        }

        // Helper: Finds the actual class that implements the method to avoid "Abstract/Not Implemented" crashes
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
        private static FieldInfo ageBiologicalTicks = AccessTools.Field(typeof(Pawn_AgeTracker), "ageBiologicalTicksInt");
        private static MethodInfo recalcLifeStage = AccessTools.Method(typeof(Pawn_AgeTracker), "RecalculateLifeStageIndex");
        private static FieldInfo fullnessField = AccessTools.Field(typeof(CompHasGatherableBodyResource), "fullness");

        // --- AGEING ---
        public static void AgeTickPostfix(Pawn_AgeTracker __instance, Pawn ___pawn)
        {
            if (___pawn == null || RanchWorldMod.settings == null) return;
            if (!___pawn.RaceProps.Animal) return; // Only affect animals

            float mult = RanchWorldMod.settings.baseGrowthMult * RanchWorldMod.settings.animalGrowthMult * RanchWorldMod.settings.animalAgeMult;

            // Debug Log (Throttled)
            if (mult > 1.0f && ___pawn.IsHashIntervalTick(600))
                Log.Message($"[RanchWorld] Ageing {___pawn.LabelShort} at {mult}x speed.");

            if (mult <= 1f) return;

            float extraTicks = mult - 1f;
            long currentAge = (long)ageBiologicalTicks.GetValue(__instance);
            ageBiologicalTicks.SetValue(__instance, currentAge + (long)extraTicks);

            recalcLifeStage.Invoke(__instance, null);
        }

        // --- GESTATION ---
        public static void AnimalGestationTick(Hediff __instance)
        {
            if (RanchWorldMod.settings == null || __instance.pawn == null) return;
            if (!(__instance is Hediff_Pregnant)) return; // Strict Check

            float mult = RanchWorldMod.settings.baseGrowthMult * RanchWorldMod.settings.animalGrowthMult * RanchWorldMod.settings.animalGestMult;

            // Debug Log (Throttled)
            if (mult > 1.0f && __instance.pawn.IsHashIntervalTick(600))
                Log.Message($"[RanchWorld] Animal Gestation Tick {__instance.pawn.LabelShort} at {mult}x speed.");

            if (mult <= 1f) return;

            float daily = 1f / (__instance.pawn.RaceProps.gestationPeriodDays * 60000f);
            float extra = daily * (mult - 1f);

            // FIX: Uses the valid base field 'severityInt'
            HarmonyInit.HediffSeverity(__instance) += extra;
        }

        public static void HumanGestationTick(Hediff __instance)
        {
            if (RanchWorldMod.settings == null || __instance.pawn == null) return;

            float mult = RanchWorldMod.settings.baseGrowthMult * RanchWorldMod.settings.humanGrowthMult * RanchWorldMod.settings.humanGestMult;
            if (mult <= 1f) return;

            float daily = 1f / (__instance.pawn.RaceProps.gestationPeriodDays * 60000f);
            float extra = daily * (mult - 1f);

            HarmonyInit.HediffSeverity(__instance) += extra;
        }

        // --- BIOTECH GROWTH ---
        public static void GrowthPointsPostfix(ref float __result, Pawn ___pawn)
        {
            if (___pawn == null || RanchWorldMod.settings == null) return;
            float mult = RanchWorldMod.settings.baseGrowthMult;
            if (___pawn.RaceProps.Humanlike) mult *= RanchWorldMod.settings.humanGrowthMult * RanchWorldMod.settings.humanAgeMult;
            else if (___pawn.RaceProps.Animal) mult *= RanchWorldMod.settings.animalGrowthMult * RanchWorldMod.settings.animalAgeMult;
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
            {
                amount = __result * pawn.BodySize;
                amount *= RanchWorldMod.settings.generalOutputMult * RanchWorldMod.settings.milkOutputMult;
            }
            else if (__instance is CompShearable)
            {
                float surfaceAreaFactor = Mathf.Pow(pawn.BodySize, 2f / 3f);
                amount = __result * surfaceAreaFactor;
                amount *= RanchWorldMod.settings.generalOutputMult * RanchWorldMod.settings.woolOutputMult;
            }
            else return;

            __result = Mathf.RoundToInt(amount);
        }

        public static void GatherFreqPrefix(CompHasGatherableBodyResource __instance)
        {
            if (RanchWorldMod.settings == null || RanchWorldMod.settings.gatherFreqMult <= 1f) return;
            if (!(__instance is CompMilkable) && !(__instance is CompShearable)) return;

            float f = (float)fullnessField.GetValue(__instance);
            fullnessField.SetValue(__instance, Mathf.Min(1f, f + (1f / 60000f) * (RanchWorldMod.settings.gatherFreqMult - 1f)));
        }

        // --- BUTCHER ---
        public static void ButcherPostfix(Pawn __instance, ref IEnumerable<Thing> __result)
        {
            if (RanchWorldMod.settings == null || __result == null || __instance == null) return;
            List<Thing> processedList = new List<Thing>();
            float bodySize = __instance.BodySize;
            if (bodySize <= 0.01f) bodySize = 0.01f;

            float leatherConversionFactor = Mathf.Pow(bodySize, -1f / 3f);

            foreach (Thing t in __result)
            {
                float finalCount = t.stackCount;
                if (t.def.IsLeather)
                {
                    finalCount *= leatherConversionFactor;
                    finalCount *= RanchWorldMod.settings.leatherButcherMult * RanchWorldMod.settings.generalButcherMult;
                }
                else if (t.def.IsMeat)
                {
                    finalCount *= RanchWorldMod.settings.meatButcherMult * RanchWorldMod.settings.generalButcherMult;
                }
                else
                {
                    finalCount *= RanchWorldMod.settings.generalButcherMult;
                }
                t.stackCount = GenMath.RoundRandom(finalCount);
                if (t.stackCount > 0) processedList.Add(t);
            }
            __result = processedList;
        }

        // --- COMBAT & METABOLISM ---
        public static void HealthScalePostfix(Pawn __instance, ref float __result)
        {
            if (__instance == null || RanchWorldMod.settings == null) return;
            if (__instance.RaceProps.Animal)
            {
                __result = __instance.BodySize * RanchWorldMod.settings.healthScaleMult;
            }
        }

        public static void DamageScalePostfix(VerbProperties __instance, Tool tool, Pawn attacker, ref float __result)
        {
            if (attacker == null || RanchWorldMod.settings == null) return;
            if (attacker.RaceProps.Animal)
            {
                __result *= attacker.BodySize * RanchWorldMod.settings.damageScaleMult;
            }
        }

        public static void HungerPostfix(Need_Food __instance, ref float __result, Pawn ___pawn)
        {
            if (___pawn == null || RanchWorldMod.settings == null) return;
            float mult = RanchWorldMod.settings.generalHungerMult;
            if (___pawn.RaceProps.Humanlike) mult *= RanchWorldMod.settings.humanHungerMult;
            else if (___pawn.RaceProps.Animal) mult *= RanchWorldMod.settings.animalHungerMult;

            FoodTypeFlags flags = ___pawn.RaceProps.foodType;
            bool plants = (flags & FoodTypeFlags.VegetarianAnimal) != 0 || (flags & FoodTypeFlags.Plant) != 0;
            bool meat = (flags & FoodTypeFlags.Meat) != 0 || (flags & FoodTypeFlags.CarnivoreAnimal) != 0;

            if (plants && !meat) mult *= RanchWorldMod.settings.herbivoreHungerMult;
            else if (meat && !plants) mult *= RanchWorldMod.settings.carnivoreHungerMult;
            else mult *= RanchWorldMod.settings.omnivoreHungerMult;

            __result *= (float)Math.Pow(___pawn.BodySize, 0.75) * mult;
        }

        public static void StomachPostfix(Need_Food __instance, ref float __result, Pawn ___pawn)
        {
            if (___pawn == null || RanchWorldMod.settings == null) return;
            float mult = RanchWorldMod.settings.generalStomachMult;
            if (___pawn.RaceProps.Humanlike) mult *= RanchWorldMod.settings.humanStomachMult;
            else if (___pawn.RaceProps.Animal) mult *= RanchWorldMod.settings.animalStomachMult;

            FoodTypeFlags flags = ___pawn.RaceProps.foodType;
            bool plants = (flags & FoodTypeFlags.VegetarianAnimal) != 0 || (flags & FoodTypeFlags.Plant) != 0;
            bool meat = (flags & FoodTypeFlags.Meat) != 0 || (flags & FoodTypeFlags.CarnivoreAnimal) != 0;

            if (plants && !meat) mult *= RanchWorldMod.settings.herbivoreStomachMult;
            else if (meat && !plants) mult *= RanchWorldMod.settings.carnivoreStomachMult;
            else mult *= RanchWorldMod.settings.omnivoreStomachMult;

            __result = ___pawn.BodySize * mult;
        }
    }
}