using UnityEngine;
using Verse;
using System;

namespace RanchWorld
{
    public class RanchWorldSettings : ModSettings
    {
        private const float DefaultMult = 1f;
        private const float GlobalMin = 0.1f;

        // Core Variables with new requested defaults
        public float baseGrowthMult = 5f;
        public float humanGrowthMult = DefaultMult, humanAgeMult = DefaultMult, humanGestMult = DefaultMult;
        public float animalGrowthMult = DefaultMult, animalAgeMult = DefaultMult, animalGestMult = DefaultMult;

        public float generalHungerMult = 1f;
        public float herbivoreHungerMult = 2f;
        public float carnivoreHungerMult = 1f;
        public float omnivoreHungerMult = 1f;

        public float generalOutputMult = DefaultMult, milkOutputMult = DefaultMult, woolOutputMult = DefaultMult, gatherFreqMult = 1f;
        public float generalButcherMult = DefaultMult, meatButcherMult = DefaultMult, leatherButcherMult = DefaultMult;

        private Vector2 scrollPosition = Vector2.zero;
        private string bGrowBuf, hGrowBuf, hAgeBuf, hGestBuf, aGrowBuf, aAgeBuf, aGestBuf, gHungerBuf, hHungerBuf, cHungerBuf, oHungerBuf, gOutBuf, mOutBuf, wOutBuf, gFreqBuf, bGenBuf, bMeatBuf, bLeathBuf;

        public override void ExposeData()
        {
            base.ExposeData();
            // Defaults in Look() updated to match your new design
            Scribe_Values.Look(ref baseGrowthMult, "baseGrowthMult", 5f);
            Scribe_Values.Look(ref humanGrowthMult, "humanGrowthMult", DefaultMult);
            Scribe_Values.Look(ref humanAgeMult, "humanAgeMult", DefaultMult);
            Scribe_Values.Look(ref humanGestMult, "humanGestMult", DefaultMult);
            Scribe_Values.Look(ref animalGrowthMult, "animalGrowthMult", DefaultMult);
            Scribe_Values.Look(ref animalAgeMult, "animalAgeMult", DefaultMult);
            Scribe_Values.Look(ref animalGestMult, "animalGestMult", DefaultMult);

            Scribe_Values.Look(ref generalHungerMult, "generalHungerMult", 1f);
            Scribe_Values.Look(ref herbivoreHungerMult, "herbivoreHungerMult", 2f);
            Scribe_Values.Look(ref carnivoreHungerMult, "carnivoreHungerMult", 1f);
            Scribe_Values.Look(ref omnivoreHungerMult, "omnivoreHungerMult", 1f);

            Scribe_Values.Look(ref generalOutputMult, "generalOutputMult", DefaultMult);
            Scribe_Values.Look(ref milkOutputMult, "milkOutputMult", DefaultMult);
            Scribe_Values.Look(ref woolOutputMult, "woolOutputMult", DefaultMult);
            Scribe_Values.Look(ref gatherFreqMult, "gatherFreqMult", 1f);
            Scribe_Values.Look(ref generalButcherMult, "generalButcherMult", DefaultMult);
            Scribe_Values.Look(ref meatButcherMult, "meatButcherMult", DefaultMult);
            Scribe_Values.Look(ref leatherButcherMult, "leatherButcherMult", DefaultMult);
        }

        public void DoWindowContents(Rect inRect)
        {
            if (Widgets.ButtonText(new Rect(0f, 0f, 150f, 30f), "Reset to Defaults")) ResetSettings();

            Rect outRect = new Rect(0f, 40f, inRect.width, inRect.height - 40f);
            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, 1100f);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            Listing_Standard list = new Listing_Standard();
            list.Begin(viewRect);

            Text.Font = GameFont.Medium;
            list.Label("Growth & Ageing Hierarchy");
            Text.Font = GameFont.Small;
            DrawNumericSetting(list, "Global Growth Mult", ref baseGrowthMult, ref bGrowBuf, GlobalMin, 25f);
            DrawNumericSetting(list, "  Human Growth", ref humanGrowthMult, ref hGrowBuf, GlobalMin, 10f);
            DrawNumericSetting(list, "    Human Ageing", ref humanAgeMult, ref hAgeBuf, GlobalMin, 10f);
            DrawNumericSetting(list, "    Human Gestation", ref humanGestMult, ref hGestBuf, GlobalMin, 10f);
            DrawNumericSetting(list, "  Animal Growth", ref animalGrowthMult, ref aGrowBuf, GlobalMin, 10f);
            DrawNumericSetting(list, "    Animal Ageing", ref animalAgeMult, ref aAgeBuf, GlobalMin, 10f);
            DrawNumericSetting(list, "    Animal Gestation", ref animalGestMult, ref aGestBuf, GlobalMin, 10f);
            list.GapLine();

            Text.Font = GameFont.Medium;
            list.Label("Dietary Hunger");
            Text.Font = GameFont.Small;
            DrawNumericSetting(list, "Global Hunger Mult", ref generalHungerMult, ref gHungerBuf, GlobalMin, 10f);
            DrawNumericSetting(list, "  Herbivore Mult", ref herbivoreHungerMult, ref hHungerBuf, GlobalMin, 5f);
            DrawNumericSetting(list, "  Carnivore Mult", ref carnivoreHungerMult, ref cHungerBuf, GlobalMin, 5f);
            DrawNumericSetting(list, "  Omnivore Mult", ref omnivoreHungerMult, ref oHungerBuf, GlobalMin, 5f);
            list.GapLine();

            Text.Font = GameFont.Medium;
            list.Label("Production Output");
            Text.Font = GameFont.Small;
            DrawNumericSetting(list, "General Output Mult", ref generalOutputMult, ref gOutBuf, GlobalMin, 10f);
            DrawNumericSetting(list, "  Milk Amount Mult", ref milkOutputMult, ref mOutBuf, GlobalMin, 10f);
            DrawNumericSetting(list, "  Wool Amount Mult", ref woolOutputMult, ref wOutBuf, GlobalMin, 10f);
            DrawNumericSetting(list, "Gathering Frequency", ref gatherFreqMult, ref gFreqBuf, GlobalMin, 10f);
            list.GapLine();

            Text.Font = GameFont.Medium;
            list.Label("Butcher Yields");
            Text.Font = GameFont.Small;
            DrawNumericSetting(list, "Butcher Yield (All)", ref generalButcherMult, ref bGenBuf, GlobalMin, 10f);
            DrawNumericSetting(list, "  Meat Only Mult", ref meatButcherMult, ref bMeatBuf, GlobalMin, 10f);
            DrawNumericSetting(list, "  Leather Only Mult", ref leatherButcherMult, ref bLeathBuf, GlobalMin, 10f);

            list.End();
            Widgets.EndScrollView();
        }

        private void DrawNumericSetting(Listing_Standard list, string label, ref float val, ref string buf, float min, float max)
        {
            Rect rect = list.GetRect(30f);
            if (buf == null) buf = val.ToString("F1");
            Widgets.Label(rect.LeftPart(0.4f), label);

            float newVal = Widgets.HorizontalSlider(new Rect(rect.x + rect.width * 0.45f, rect.y, rect.width * 0.35f, rect.height), val, min, max);
            if (newVal != val)
            {
                val = (float)Math.Round(newVal, 1);
                buf = val.ToString("F1");
            }
            Widgets.TextFieldNumeric<float>(rect.RightPart(0.15f), ref val, ref buf, min, max);
        }

        private void ResetSettings()
        {
            // Reset logic updated to your new defaults
            baseGrowthMult = 5f;
            humanGrowthMult = humanAgeMult = humanGestMult = animalGrowthMult = animalAgeMult = animalGestMult = 1f;
            generalHungerMult = 1f;
            herbivoreHungerMult = 2f;
            carnivoreHungerMult = 1f;
            omnivoreHungerMult = 1f;
            generalOutputMult = milkOutputMult = woolOutputMult = gatherFreqMult = generalButcherMult = meatButcherMult = leatherButcherMult = 1f;
            bGrowBuf = hGrowBuf = hAgeBuf = hGestBuf = aGrowBuf = aAgeBuf = aGestBuf = gHungerBuf = hHungerBuf = cHungerBuf = oHungerBuf = gOutBuf = mOutBuf = wOutBuf = gFreqBuf = bGenBuf = bMeatBuf = bLeathBuf = null;
        }
    }
}