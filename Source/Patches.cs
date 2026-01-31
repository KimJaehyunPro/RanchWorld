using HarmonyLib;
using Verse;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

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

        public static void HungerPostfix(Need_Food __instance, ref float __result, Pawn ___pawn)
        {
            if (___pawn == null || RanchWorldMod.settings == null) return;
            __result *= (float)System.Math.Pow(___pawn.BodySize, 0.75) * RanchWorldMod.settings.generalHungerMult;
        }

        public static void AgeingPostfix(ref float __result, Pawn ___pawn)
        {
            if (___pawn == null || RanchWorldMod.settings == null) return;
            __result = RanchWorldMod.settings.baseAgeSpeed;
        }

        public static void OutputPostfix(CompHasGatherableBodyResource __instance, ref int __result)
        {
            if (RanchWorldMod.settings == null) return;
            float finalMult = RanchWorldMod.settings.generalOutputMult;
            if (__instance is CompMilkable) finalMult *= RanchWorldMod.settings.milkOutputMult;
            else if (__instance is CompShearable) finalMult *= RanchWorldMod.settings.woolOutputMult;
            __result = Mathf.RoundToInt(__result * finalMult);
        }

        public static void ButcherPostfix(Pawn __instance, ref IEnumerable<Thing> __result)
        {
            if (RanchWorldMod.settings == null) return;
            foreach (Thing t in __result)
            {
                float mult = RanchWorldMod.settings.generalButcherMult;
                if (t.def.IsMeat) mult *= RanchWorldMod.settings.meatButcherMult;
                if (t.def.IsLeather) mult *= RanchWorldMod.settings.leatherButcherMult;
                t.stackCount = Mathf.RoundToInt(t.stackCount * mult);
            }
        }

        public static void GatherFreqPrefix(CompHasGatherableBodyResource __instance)
        {
            if (RanchWorldMod.settings == null || RanchWorldMod.settings.gatherFreqMult <= 1f) return;
            float currentFullness = (float)fullnessField.GetValue(__instance);
            float extraProgress = (1f / 60000f) * (RanchWorldMod.settings.gatherFreqMult - 1f);
            fullnessField.SetValue(__instance, Mathf.Min(1f, currentFullness + extraProgress));
        }
    }
}