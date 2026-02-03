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

            Rect outRect = new Rect(0f, 40f, inRect.width, inRect.height - 40f);
            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, 1750f);

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

            list.End();
            Widgets.EndScrollView();
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