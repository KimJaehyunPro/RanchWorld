using UnityEngine;
using Verse;
using System;

namespace RanchWorld
{
    public class RanchWorldSettings : ModSettings
    {
        private const float DefaultAge = 10f;
        private const float DefaultMult = 1f;

        // Core Variables
        public float baseAgeSpeed = DefaultAge;
        public float humanAgeMult = DefaultMult;
        public float animalAgeMult = DefaultMult;
        public float generalHungerMult = 0.5f;
        public float herbivoreHungerMult = 1.6f;
        public float carnivoreHungerMult = 1.8f;
        public float omnivoreHungerMult = 1.4f;
        public float generalOutputMult = DefaultMult;
        public float milkOutputMult = DefaultMult;
        public float woolOutputMult = DefaultMult;
        public float gatherFreqMult = 1f;
        public float generalButcherMult = 1f;
        public float meatButcherMult = 1f;
        public float leatherButcherMult = 1f;

        // Buffers for UI synchronization
        private string ageBuf, hAgeBuf, aAgeBuf, gHungerBuf, hHungerBuf, cHungerBuf, oHungerBuf, gOutBuf, mOutBuf, wOutBuf, gFreqBuf, bGenBuf, bMeatBuf, bLeathBuf;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref baseAgeSpeed, "baseAgeSpeed", DefaultAge);
            Scribe_Values.Look(ref humanAgeMult, "humanAgeMult", DefaultMult);
            Scribe_Values.Look(ref animalAgeMult, "animalAgeMult", DefaultMult);
            Scribe_Values.Look(ref generalHungerMult, "generalHungerMult", 0.5f);
            Scribe_Values.Look(ref herbivoreHungerMult, "herbivoreHungerMult", 1.6f);
            Scribe_Values.Look(ref carnivoreHungerMult, "carnivoreHungerMult", 1.8f);
            Scribe_Values.Look(ref omnivoreHungerMult, "omnivoreHungerMult", 1.4f);
            Scribe_Values.Look(ref generalOutputMult, "generalOutputMult", DefaultMult);
            Scribe_Values.Look(ref milkOutputMult, "milkOutputMult", DefaultMult);
            Scribe_Values.Look(ref woolOutputMult, "woolOutputMult", DefaultMult);
            Scribe_Values.Look(ref gatherFreqMult, "gatherFreqMult", 1f);
            Scribe_Values.Look(ref generalButcherMult, "generalButcherMult", 1f);
            Scribe_Values.Look(ref meatButcherMult, "meatButcherMult", 1f);
            Scribe_Values.Look(ref leatherButcherMult, "leatherButcherMult", 1f);
        }

        public void DoWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);

            if (list.ButtonText("Reset to Defaults")) { ResetSettings(); }
            list.Gap();

            DrawNumericSetting(list, "Base Ageing Speed", ref baseAgeSpeed, ref ageBuf, 0.01f, 25f);
            DrawNumericSetting(list, "  Human Age Mult", ref humanAgeMult, ref hAgeBuf, 0.01f, 10f);
            DrawNumericSetting(list, "  Animal Age Mult", ref animalAgeMult, ref aAgeBuf, 0.01f, 10f);
            list.GapLine();

            DrawNumericSetting(list, "Global Hunger Mult", ref generalHungerMult, ref gHungerBuf, 0.1f, 10f);
            DrawNumericSetting(list, "  Herbivore Mult", ref herbivoreHungerMult, ref hHungerBuf, 0.1f, 5f);
            DrawNumericSetting(list, "  Carnivore Mult", ref carnivoreHungerMult, ref cHungerBuf, 0.1f, 5f);
            DrawNumericSetting(list, "  Omnivore Mult", ref omnivoreHungerMult, ref oHungerBuf, 0.1f, 5f);
            list.GapLine();

            DrawNumericSetting(list, "General Output Mult", ref generalOutputMult, ref gOutBuf, 0.1f, 10f);
            DrawNumericSetting(list, "  Milk Output Mult", ref milkOutputMult, ref mOutBuf, 0.1f, 10f);
            DrawNumericSetting(list, "  Wool Output Mult", ref woolOutputMult, ref wOutBuf, 0.1f, 10f);
            DrawNumericSetting(list, "Gathering Frequency", ref gatherFreqMult, ref gFreqBuf, 0.1f, 10f);
            list.GapLine();

            DrawNumericSetting(list, "Butcher Yield (All)", ref generalButcherMult, ref bGenBuf, 0.1f, 10f);
            DrawNumericSetting(list, "  Meat Only Mult", ref meatButcherMult, ref bMeatBuf, 0.1f, 10f);
            DrawNumericSetting(list, "  Leather Only Mult", ref leatherButcherMult, ref bLeathBuf, 0.1f, 10f);

            list.End();
        }

        private void DrawNumericSetting(Listing_Standard list, string label, ref float value, ref string buffer, float min, float max)
        {
            Rect rect = list.GetRect(30f);
            if (buffer == null) buffer = value.ToString("F2");
            Widgets.Label(rect.LeftPart(0.4f), label);
            Rect sliderRect = new Rect(rect.x + rect.width * 0.45f, rect.y, rect.width * 0.35f, rect.height);
            float sliderVal = Widgets.HorizontalSlider(sliderRect, value, min, max);
            if (sliderVal != value) { value = sliderVal; buffer = value.ToString("F2"); }
            Widgets.TextFieldNumeric<float>(rect.RightPart(0.15f), ref value, ref buffer, min, max);
        }

        private void ResetSettings()
        {
            baseAgeSpeed = DefaultAge;
            humanAgeMult = animalAgeMult = generalOutputMult = milkOutputMult = woolOutputMult = gatherFreqMult = generalButcherMult = meatButcherMult = leatherButcherMult = 1f;
            generalHungerMult = 0.5f;
            herbivoreHungerMult = 1.6f;
            carnivoreHungerMult = 1.8f;
            omnivoreHungerMult = 1.4f;
            ageBuf = hAgeBuf = aAgeBuf = gHungerBuf = hHungerBuf = cHungerBuf = oHungerBuf = gOutBuf = mOutBuf = wOutBuf = gFreqBuf = bGenBuf = bMeatBuf = bLeathBuf = null;
        }
    }
}