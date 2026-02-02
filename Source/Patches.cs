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
        static HarmonyInit()
        {
            var harmony = new Harmony("Foo.RanchWorld");

            harmony.Patch(AccessTools.PropertyGetter(typeof(Need_Food), "FoodFallPerTick"),
                null, new HarmonyMethod(typeof(RanchPatches), nameof(RanchPatches.HungerPostfix)));

            harmony.Patch(AccessTools.PropertyGetter(typeof(Pawn_AgeTracker), "BiologicalTicksPerTick"),
                null, new HarmonyMethod(typeof(RanchPatches), nameof(RanchPatches.AgeingPostfix)));

            harmony.Patch(AccessTools.Method(typeof(Hediff), nameof(Hediff.Tick)),
                new HarmonyMethod(typeof(RanchPatches), nameof(RanchPatches.GestationPrefix)));

            harmony.Patch(AccessTools.PropertyGetter(typeof(CompMilkable), "ResourceAmount"),
                null, new HarmonyMethod(typeof(RanchPatches), nameof(RanchPatches.OutputPostfix)));
            harmony.Patch(AccessTools.PropertyGetter(typeof(CompShearable), "ResourceAmount"),
                null, new HarmonyMethod(typeof(RanchPatches), nameof(RanchPatches.OutputPostfix)));

            harmony.Patch(AccessTools.Method(typeof(Pawn), nameof(Pawn.ButcherProducts)),
                null, new HarmonyMethod(typeof(RanchPatches), nameof(RanchPatches.ButcherPostfix)));

            harmony.Patch(AccessTools.Method(typeof(CompHasGatherableBodyResource), "CompTick"),
                new HarmonyMethod(typeof(RanchPatches), nameof(RanchPatches.GatherFreqPrefix)));
        }
    }

    public static class RanchPatches
    {
        private static readonly FieldInfo fullnessField = AccessTools.Field(typeof(CompHasGatherableBodyResource), "fullness");
        private static readonly FieldInfo animalGestField = AccessTools.Field(typeof(Hediff_Pregnant), "gestationProgress");
        private static readonly FieldInfo humanGestField = AccessTools.Field(AccessTools.TypeByName("RimWorld.Hediff_PregnantHuman"), "gestationProgress");

        public static void AgeingPostfix(ref float __result, Pawn ___pawn)
        {
            if (___pawn == null || RanchWorldMod.settings == null) return;
            float mult = RanchWorldMod.settings.baseGrowthMult;
            if (___pawn.RaceProps.Humanlike) mult *= RanchWorldMod.settings.humanGrowthMult * RanchWorldMod.settings.humanAgeMult;
            else if (___pawn.RaceProps.Animal) mult *= RanchWorldMod.settings.animalGrowthMult * RanchWorldMod.settings.animalAgeMult;
            __result *= mult;
        }

        public static void GestationPrefix(Hediff __instance)
        {
            if (RanchWorldMod.settings == null || __instance.pawn == null) return;
            FieldInfo targetField = null;
            if (__instance is Hediff_Pregnant) targetField = animalGestField;
            else if (__instance.GetType().Name == "Hediff_PregnantHuman") targetField = humanGestField;
            if (targetField == null) return;

            float mult = RanchWorldMod.settings.baseGrowthMult;
            if (__instance.pawn.RaceProps.Humanlike) mult *= RanchWorldMod.settings.humanGrowthMult * RanchWorldMod.settings.humanGestMult;
            else if (__instance.pawn.RaceProps.Animal) mult *= RanchWorldMod.settings.animalGrowthMult * RanchWorldMod.settings.animalGestMult;

            if (mult > 1f)
            {
                float current = (float)targetField.GetValue(__instance);
                float extra = (1f / (__instance.pawn.RaceProps.gestationPeriodDays * 60000f)) * (mult - 1f);
                targetField.SetValue(__instance, current + extra);
            }
        }

        public static void HungerPostfix(Need_Food __instance, ref float __result, Pawn ___pawn)
        {
            if (___pawn == null || RanchWorldMod.settings == null) return;

            // 1. Start with Global Hunger
            float mult = RanchWorldMod.settings.generalHungerMult;

            // 2. Add Category (Human vs Animal) Hierarchy
            if (___pawn.RaceProps.Humanlike) mult *= RanchWorldMod.settings.humanHungerMult;
            else if (___pawn.RaceProps.Animal) mult *= RanchWorldMod.settings.animalHungerMult;

            // 3. Keep Diet Tweaks
            FoodTypeFlags flags = ___pawn.RaceProps.foodType;
            bool plants = (flags & FoodTypeFlags.VegetarianAnimal) != 0 || (flags & FoodTypeFlags.Plant) != 0;
            bool meat = (flags & FoodTypeFlags.Meat) != 0 || (flags & FoodTypeFlags.CarnivoreAnimal) != 0;

            if (plants && !meat) mult *= RanchWorldMod.settings.herbivoreHungerMult;
            else if (meat && !plants) mult *= RanchWorldMod.settings.carnivoreHungerMult;
            else mult *= RanchWorldMod.settings.omnivoreHungerMult;

            // 4. Apply final calculation
            __result *= (float)Math.Pow(___pawn.BodySize, 0.75) * mult;
        }

        public static void OutputPostfix(CompHasGatherableBodyResource __instance, ref int __result)
        {
            if (RanchWorldMod.settings == null) return;
            float mult = RanchWorldMod.settings.generalOutputMult;
            if (__instance is CompMilkable) mult *= RanchWorldMod.settings.milkOutputMult;
            else if (__instance is CompShearable) mult *= RanchWorldMod.settings.woolOutputMult;
            __result = Mathf.RoundToInt(__result * mult);
        }

        public static void ButcherPostfix(ref IEnumerable<Thing> __result)
        {
            if (RanchWorldMod.settings == null || __result == null) return;
            List<Thing> processedList = new List<Thing>();
            foreach (Thing t in __result)
            {
                float mult = RanchWorldMod.settings.generalButcherMult;
                if (t.def.IsMeat) mult *= RanchWorldMod.settings.meatButcherMult;
                if (t.def.IsLeather) mult *= RanchWorldMod.settings.leatherButcherMult;
                t.stackCount = Mathf.RoundToInt(t.stackCount * mult);
                if (t.stackCount > 0) processedList.Add(t);
            }
            __result = processedList;
        }

        public static void GatherFreqPrefix(CompHasGatherableBodyResource __instance)
        {
            if (RanchWorldMod.settings == null || RanchWorldMod.settings.gatherFreqMult <= 1f) return;
            float f = (float)fullnessField.GetValue(__instance);
            fullnessField.SetValue(__instance, Mathf.Min(1f, f + (1f / 60000f) * (RanchWorldMod.settings.gatherFreqMult - 1f)));
        }
    }
}