using UnityEngine;
using Verse;
using System;

namespace RanchWorld
{
    public class RanchWorldSettings : ModSettings
    {
        private const float DefaultMult = 1f;
        private const float GlobalMin = 0.1f;

        public float baseGrowthMult = 5f;
        public float humanGrowthMult = DefaultMult, humanAgeMult = DefaultMult, humanGestMult = DefaultMult;
        public float animalGrowthMult = DefaultMult, animalAgeMult = DefaultMult, animalGestMult = DefaultMult;

        public float generalHungerMult = 1f, humanHungerMult = DefaultMult, animalHungerMult = DefaultMult;
        public float herbivoreHungerMult = 2f, carnivoreHungerMult = 1f, omnivoreHungerMult = 1f;

        public float generalStomachMult = 1f;
        public float humanStomachMult = DefaultMult;
        public float animalStomachMult = DefaultMult;
        public float herbivoreStomachMult = 1f;
        public float carnivoreStomachMult = 1f;
        public float omnivoreStomachMult = 1f;

        public float generalOutputMult = DefaultMult, milkOutputMult = DefaultMult, woolOutputMult = DefaultMult, gatherFreqMult = 1f;
        public float generalButcherMult = DefaultMult, meatButcherMult = DefaultMult, leatherButcherMult = DefaultMult;

        // Combat Settings
        public float healthScaleMult = 1.0f;
        public float damageScaleMult = 0.5f;

        private Vector2 scrollPosition = Vector2.zero;
        private float viewHeight = 1800f;
        private Vector2 previewScroll = Vector2.zero;
        private float previewHeight = 1200f;
        private string bGrowBuf, hGrowBuf, hAgeBuf, hGestBuf, aGrowBuf, aAgeBuf, aGestBuf;
        private string gHungerBuf, hHungerBuf_Cat, aHungerBuf_Cat, hHungerBuf, cHungerBuf, oHungerBuf;
        private string gStomBuf, hStomBuf_Cat, aStomBuf_Cat, hStomBuf, cStomBuf, oStomBuf;
        private string gOutBuf, mOutBuf, wOutBuf, gFreqBuf, bGenBuf, bMeatBuf, bLeathBuf;
        private string healthBuf, damageBuf;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref baseGrowthMult, "baseGrowthMult", 5f);
            Scribe_Values.Look(ref humanGrowthMult, "humanGrowthMult", 1f);
            Scribe_Values.Look(ref humanAgeMult, "humanAgeMult", 1f);
            Scribe_Values.Look(ref humanGestMult, "humanGestMult", 1f);
            Scribe_Values.Look(ref animalGrowthMult, "animalGrowthMult", 1f);
            Scribe_Values.Look(ref animalAgeMult, "animalAgeMult", 1f);
            Scribe_Values.Look(ref animalGestMult, "animalGestMult", 1f);

            Scribe_Values.Look(ref generalHungerMult, "generalHungerMult", 1f);
            Scribe_Values.Look(ref humanHungerMult, "humanHungerMult", 1f);
            Scribe_Values.Look(ref animalHungerMult, "animalHungerMult", 1f);
            Scribe_Values.Look(ref herbivoreHungerMult, "herbivoreHungerMult", 2f);
            Scribe_Values.Look(ref carnivoreHungerMult, "carnivoreHungerMult", 1f);
            Scribe_Values.Look(ref omnivoreHungerMult, "omnivoreHungerMult", 1f);

            Scribe_Values.Look(ref generalStomachMult, "generalStomachMult", 1f);
            Scribe_Values.Look(ref humanStomachMult, "humanStomachMult", 1f);
            Scribe_Values.Look(ref animalStomachMult, "animalStomachMult", 1f);
            Scribe_Values.Look(ref herbivoreStomachMult, "herbivoreStomachMult", 1f);
            Scribe_Values.Look(ref carnivoreStomachMult, "carnivoreStomachMult", 1f);
            Scribe_Values.Look(ref omnivoreStomachMult, "omnivoreStomachMult", 1f);

            Scribe_Values.Look(ref generalOutputMult, "generalOutputMult", 1f);
            Scribe_Values.Look(ref milkOutputMult, "milkOutputMult", 1f);
            Scribe_Values.Look(ref woolOutputMult, "woolOutputMult", 1f);
            Scribe_Values.Look(ref gatherFreqMult, "gatherFreqMult", 1f);
            Scribe_Values.Look(ref generalButcherMult, "generalButcherMult", 1f);
            Scribe_Values.Look(ref meatButcherMult, "meatButcherMult", 1f);
            Scribe_Values.Look(ref leatherButcherMult, "leatherButcherMult", 1f);

            Scribe_Values.Look(ref healthScaleMult, "healthScaleMult", 1.0f);
            Scribe_Values.Look(ref damageScaleMult, "damageScaleMult", 0.5f);
        }

        public void DoWindowContents(Rect inRect)
        {
            if (Widgets.ButtonText(new Rect(0f, 0f, 150f, 30f), "Reset to Defaults")) ResetSettings();

            // Left: sliders. Right: live preview of the final multipliers, so changes show up immediately.
            float colGap = 12f;
            float leftWidth = (inRect.width - colGap) * 0.55f;
            Rect outRect = new Rect(0f, 40f, leftWidth, inRect.height - 40f);
            Rect viewRect = new Rect(0f, 0f, leftWidth - 16f, viewHeight);

            Rect previewOut = new Rect(leftWidth + colGap, 40f, inRect.width - leftWidth - colGap, inRect.height - 40f);
            Rect previewInner = previewOut.ContractedBy(4f);
            Rect previewView = new Rect(0f, 0f, previewInner.width - 16f, previewHeight);
            Widgets.DrawMenuSection(previewOut);
            Widgets.BeginScrollView(previewInner, ref previewScroll, previewView);
            Listing_Standard preview = new Listing_Standard();
            preview.Begin(previewView);
            DrawEffectiveSummary(preview);
            previewHeight = Mathf.Max(preview.CurHeight + 20f, 200f);
            preview.End();
            Widgets.EndScrollView();

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            Listing_Standard list = new Listing_Standard();
            list.Begin(viewRect);

            // GROWTH
            Text.Font = GameFont.Medium; list.Label("Growth"); Text.Font = GameFont.Small;
            DrawNumericSetting(list, "Global Growth", ref baseGrowthMult, ref bGrowBuf, GlobalMin, 60f);
            DrawNumericSetting(list, "  Human Growth", ref humanGrowthMult, ref hGrowBuf, GlobalMin, 10f);
            DrawNumericSetting(list, "    Human Ageing", ref humanAgeMult, ref hAgeBuf, GlobalMin, 10f);
            DrawNumericSetting(list, "    Human Gestation", ref humanGestMult, ref hGestBuf, GlobalMin, 10f);
            DrawNumericSetting(list, "  Animal Growth", ref animalGrowthMult, ref aGrowBuf, GlobalMin, 10f);
            DrawNumericSetting(list, "    Animal Ageing", ref animalAgeMult, ref aAgeBuf, GlobalMin, 10f);
            DrawNumericSetting(list, "    Animal Gestation", ref animalGestMult, ref aGestBuf, GlobalMin, 10f);
            list.GapLine();

            // HUNGER
            Text.Font = GameFont.Medium; list.Label("Hunger Rate"); Text.Font = GameFont.Small;
            DrawNumericSetting(list, "Global Hunger Rate", ref generalHungerMult, ref gHungerBuf, GlobalMin, 5f);
            DrawNumericSetting(list, "  Human Hunger Rate", ref humanHungerMult, ref hHungerBuf_Cat, GlobalMin, 5f);
            DrawNumericSetting(list, "  Animal Hunger Rate", ref animalHungerMult, ref aHungerBuf_Cat, GlobalMin, 5f);
            DrawNumericSetting(list, "    Herbivore Hunger Rate", ref herbivoreHungerMult, ref hHungerBuf, GlobalMin, 5f);
            DrawNumericSetting(list, "    Carnivore Hunger Rate", ref carnivoreHungerMult, ref cHungerBuf, GlobalMin, 5f);
            DrawNumericSetting(list, "    Omnivore Hunger Rate", ref omnivoreHungerMult, ref oHungerBuf, GlobalMin, 5f);
            list.GapLine();

            // STOMACH
            Text.Font = GameFont.Medium; list.Label("Stomach Capacity"); Text.Font = GameFont.Small;
            DrawNumericSetting(list, "Global Stomach Capacity", ref generalStomachMult, ref gStomBuf, GlobalMin, 10f);
            DrawNumericSetting(list, "  Human Stomach Capacity", ref humanStomachMult, ref hStomBuf_Cat, GlobalMin, 10f);
            DrawNumericSetting(list, "  Animal Stomach Capacity", ref animalStomachMult, ref aStomBuf_Cat, GlobalMin, 10f);
            DrawNumericSetting(list, "    Herbivore Stomach Capacity", ref herbivoreStomachMult, ref hStomBuf, GlobalMin, 10f);
            DrawNumericSetting(list, "    Carnivore Stomach Capacity", ref carnivoreStomachMult, ref cStomBuf, GlobalMin, 10f);
            DrawNumericSetting(list, "    Omnivore Stomach Capacity", ref omnivoreStomachMult, ref oStomBuf, GlobalMin, 10f);
            list.GapLine();

            // PRODUCTION
            Text.Font = GameFont.Medium; list.Label("Production Yields (Auto-Scaled by Body Size)"); Text.Font = GameFont.Small;
            DrawNumericSetting(list, "General Production Yields", ref generalOutputMult, ref gOutBuf, GlobalMin, 10f);
            DrawNumericSetting(list, "  Milk Yields (3D Scale)", ref milkOutputMult, ref mOutBuf, GlobalMin, 10f);
            DrawNumericSetting(list, "  Wool Yields (2D Scale)", ref woolOutputMult, ref wOutBuf, GlobalMin, 10f);
            DrawNumericSetting(list, "Gathering Frequency", ref gatherFreqMult, ref gFreqBuf, GlobalMin, 10f);
            list.GapLine();

            // BUTCHER
            Text.Font = GameFont.Medium; list.Label("Butcher Yields (Auto-Scaled by Body Size)"); Text.Font = GameFont.Small;
            DrawNumericSetting(list, "General Butcher Yield", ref generalButcherMult, ref bGenBuf, GlobalMin, 10f);
            DrawNumericSetting(list, "  Meat Yield (Linear Scale)", ref meatButcherMult, ref bMeatBuf, GlobalMin, 10f);
            DrawNumericSetting(list, "  Leather Yield (2D Scale)", ref leatherButcherMult, ref bLeathBuf, GlobalMin, 10f);
            list.GapLine();

            // COMBAT
            Text.Font = GameFont.Medium; list.Label("Combat Scaling (Based on Body Size)"); Text.Font = GameFont.Small;
            DrawNumericSetting(list, "Health Scale Multiplier", ref healthScaleMult, ref healthBuf, 0.1f, 5f);
            DrawNumericSetting(list, "Melee Damage Multiplier", ref damageScaleMult, ref damageBuf, 0.1f, 5f);

            viewHeight = Mathf.Max(list.CurHeight + 20f, 600f);
            list.End();
            Widgets.EndScrollView();
        }

        // --- EFFECTIVE MULTIPLIER PREVIEW ---
        // These formulas mirror what Patches.cs actually does, so the numbers shown here
        // are what the game applies, including quirks (e.g. ageing can't go below 1x).
        private static readonly Color NotAppliedColor = new Color(0.6f, 0.6f, 0.6f);
        private static readonly Color WarnColor = new Color(1f, 0.8f, 0.4f);

        // Ageing and gestation patches only ever ADD progress; a product below 1 does nothing.
        private static float SpeedUpOnly(float mult) => mult > 1f ? mult : 1f;
        private static float AgeingEffective(float mult) => mult > 1.05f ? mult : 1f;

        private static string X(float v) => v >= 100f ? v.ToString("F0") + "x" : v.ToString("0.##") + "x";

        private void DrawEffectiveSummary(Listing_Standard list)
        {
            Text.Font = GameFont.Medium; list.Label("Effective Multipliers"); Text.Font = GameFont.Small;
            GUI.color = NotAppliedColor;
            list.Label("Final values after all sliders are multiplied together, matching what the mod's code applies. 1x = vanilla.");
            GUI.color = Color.white;

            float B = baseGrowthMult;
            float hAge = B * humanGrowthMult * humanAgeMult;
            float hGrowthPts = B * humanGrowthMult;
            float hGestOwn = B * humanGrowthMult * humanGestMult;
            float aAge = B * animalGrowthMult * animalAgeMult;
            float aGest = B * animalGrowthMult * animalGestMult;
            // Human pregnancy is a subclass of animal pregnancy, so both gestation patches run on it
            // and their extra progress adds up.
            float hGest = 1f + (SpeedUpOnly(hGestOwn) - 1f) + (SpeedUpOnly(aGest) - 1f);

            SubHeader(list, "Growth & Ageing");
            Row(list, "Human ageing", X(AgeingEffective(hAge)),
                $"Global {B:0.##} x Human Growth {humanGrowthMult:0.##} x Human Ageing {humanAgeMult:0.##}" + (hAge < 1f ? "  (below 1x not supported: stays 1x)" : ""), hAge < 1f);
            Row(list, "Human growth points", X(hGrowthPts),
                $"Global {B:0.##} x Human Growth {humanGrowthMult:0.##}  (growth moment progress per tick)", false);
            Row(list, "Human gestation", X(hGest),
                $"1 + (human {X(SpeedUpOnly(hGestOwn))} - 1) + (animal {X(SpeedUpOnly(aGest))} - 1)  (animal gestation also applies to humans)", aGest > 1f);
            Row(list, "Animal ageing", X(AgeingEffective(aAge)),
                $"Global {B:0.##} x Animal Growth {animalGrowthMult:0.##} x Animal Ageing {animalAgeMult:0.##}" + (aAge < 1f ? "  (below 1x not supported: stays 1x)" : ""), aAge < 1f);
            Row(list, "Animal gestation", X(SpeedUpOnly(aGest)),
                $"Global {B:0.##} x Animal Growth {animalGrowthMult:0.##} x Animal Gestation {animalGestMult:0.##}" + (aGest < 1f ? "  (below 1x not supported: stays 1x)" : ""), aGest < 1f);

            float hHunger = generalHungerMult * humanHungerMult;
            float aHunger = generalHungerMult * animalHungerMult;
            SubHeader(list, "Hunger Rate");
            Row(list, "Human hunger", X(hHunger) + " x size^0.75",
                $"Global {generalHungerMult:0.##} x Human {humanHungerMult:0.##}" + SizeExamples(hHunger, 0.75f), false);
            Row(list, "Animal hunger", X(aHunger) + " x size^0.75",
                $"Global {generalHungerMult:0.##} x Animal {animalHungerMult:0.##}" + SizeExamples(aHunger, 0.75f), false);
            NotApplied(list, "Herbivore / Carnivore / Omnivore hunger", "not used by the code yet");

            float hStom = generalStomachMult * humanStomachMult;
            float aStom = generalStomachMult * animalStomachMult;
            SubHeader(list, "Stomach Capacity (replaces vanilla)");
            Row(list, "Human stomach", X(hStom) + " x body size",
                $"Global {generalStomachMult:0.##} x Human {humanStomachMult:0.##}", false);
            Row(list, "Animal stomach", X(aStom) + " x body size",
                $"Global {generalStomachMult:0.##} x Animal {animalStomachMult:0.##}", false);
            NotApplied(list, "Herbivore / Carnivore / Omnivore stomach", "not used by the code yet");

            float milk = generalOutputMult * milkOutputMult;
            float wool = generalOutputMult * woolOutputMult;
            SubHeader(list, "Production");
            Row(list, "Milk amount", X(milk) + " x size",
                $"General {generalOutputMult:0.##} x Milk {milkOutputMult:0.##}" + SizeExamples(milk, 1f), false);
            Row(list, "Wool amount", X(wool) + " x size^(2/3)",
                $"General {generalOutputMult:0.##} x Wool {woolOutputMult:0.##}" + SizeExamples(wool, 2f / 3f), false);
            NotApplied(list, "Gathering frequency", "not used by the code yet");

            float meat = generalButcherMult * meatButcherMult;
            float leather = generalButcherMult * leatherButcherMult;
            SubHeader(list, "Butchering");
            Row(list, "Meat", X(meat),
                $"General {generalButcherMult:0.##} x Meat {meatButcherMult:0.##}", false);
            Row(list, "Leather", X(leather) + " x size^(-1/3)",
                $"General {generalButcherMult:0.##} x Leather {leatherButcherMult:0.##}" + SizeExamples(leather, -1f / 3f), false);
            Row(list, "Other products", X(generalButcherMult), "General butcher yield", false);

            SubHeader(list, "Combat");
            NotApplied(list, "Health scale", "not used by the code yet");
            NotApplied(list, "Melee damage", "not used by the code yet");
        }

        private static string SizeExamples(float mult, float exponent)
        {
            return $"; size 0.5: {X(mult * Mathf.Pow(0.5f, exponent))}, size 1: {X(mult)}, size 2: {X(mult * Mathf.Pow(2f, exponent))}";
        }

        private static void SubHeader(Listing_Standard list, string text)
        {
            list.Gap(6f);
            GUI.color = new Color(0.8f, 0.9f, 1f);
            list.Label(text);
            GUI.color = Color.white;
        }

        private static void Row(Listing_Standard list, string label, string value, string formula, bool warn)
        {
            Rect rect = list.GetRect(Text.LineHeight);
            Widgets.Label(rect.LeftPart(0.45f), "  " + label);
            GUI.color = warn ? WarnColor : Color.white;
            Widgets.Label(rect.RightPart(0.55f), value);
            GUI.color = Color.white;
            TooltipHandler.TipRegion(rect, formula);
            Text.Font = GameFont.Tiny;
            GUI.color = NotAppliedColor;
            list.Label("      = " + formula);
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
        }

        private static void NotApplied(Listing_Standard list, string label, string reason)
        {
            GUI.color = NotAppliedColor;
            list.Label($"  {label}: no effect ({reason})");
            GUI.color = Color.white;
        }

        private void DrawNumericSetting(Listing_Standard list, string label, ref float val, ref string buf, float min, float max)
        {
            Rect rect = list.GetRect(30f);
            if (buf == null) buf = val.ToString("F1");
            Widgets.Label(rect.LeftPart(0.4f), label);
            float newVal = Widgets.HorizontalSlider(new Rect(rect.x + rect.width * 0.45f, rect.y, rect.width * 0.35f, rect.height), val, min, max);
            if (newVal != val) { val = (float)Math.Round(newVal, 1); buf = val.ToString("F1"); }
            Widgets.TextFieldNumeric<float>(rect.RightPart(0.15f), ref val, ref buf, min, max);
        }

        private void ResetSettings()
        {
            baseGrowthMult = 10f;
            humanGrowthMult = humanAgeMult = humanGestMult = animalGrowthMult = animalAgeMult = animalGestMult = 1f;
            generalHungerMult = 1f; humanHungerMult = 1f; animalHungerMult = 1f; herbivoreHungerMult = 1.7f; carnivoreHungerMult = 0.7f; omnivoreHungerMult = 1f;
            generalStomachMult = 3f; humanStomachMult = 1f; animalStomachMult = 1f; herbivoreStomachMult = 2.5f; carnivoreStomachMult = 1.3f; omnivoreStomachMult = 1f;
            generalOutputMult = milkOutputMult = woolOutputMult = gatherFreqMult = generalButcherMult = meatButcherMult = leatherButcherMult = 1f;
            healthScaleMult = 1.0f;
            damageScaleMult = 0.5f;

            bGrowBuf = hGrowBuf = hAgeBuf = hGestBuf = aGrowBuf = aAgeBuf = aGestBuf = gHungerBuf = hHungerBuf_Cat = aHungerBuf_Cat = hHungerBuf = cHungerBuf = oHungerBuf = gStomBuf = hStomBuf_Cat = aStomBuf_Cat = hStomBuf = cStomBuf = oStomBuf = gOutBuf = mOutBuf = wOutBuf = gFreqBuf = bGenBuf = bMeatBuf = bLeathBuf = null;
            healthBuf = damageBuf = null;
        }
    }
}