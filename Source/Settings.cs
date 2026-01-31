using UnityEngine; // Provides Rect, Event, EventType
using Verse;      // Provides ModSettings, Widgets, Listing_Standard
using System;     // Provides Math

namespace RanchWorld
{
    public class RanchWorldSettings : ModSettings
    {
        private const float DefaultAge = 10f;
        private const float DefaultMult = 1f;

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

        private string ageBuf, hAgeBuf, aAgeBuf, gHungerBuf, hHungerBuf, cHungerBuf, oHungerBuf, gOutBuf;

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
        }

        public void DoWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);

            if (list.ButtonText("Reset to Defaults"))
            {
                ResetSettings();
            }

            list.Gap();

            DrawNumericSetting(list, "Base Ageing Speed", ref baseAgeSpeed, ref ageBuf, 0.01f, 25f);
            DrawNumericSetting(list, "  Human Age Mult", ref humanAgeMult, ref hAgeBuf, 0.01f, 25f);
            DrawNumericSetting(list, "  Animal Age Mult", ref animalAgeMult, ref aAgeBuf, 0.01f, 25f);
            list.GapLine();
            DrawNumericSetting(list, "Global Hunger Mult", ref generalHungerMult, ref gHungerBuf, 0.1f, 10f);
            DrawNumericSetting(list, "  Herbivore Mult", ref herbivoreHungerMult, ref hHungerBuf, 0.1f, 5f);
            DrawNumericSetting(list, "  Carnivore Mult", ref carnivoreHungerMult, ref cHungerBuf, 0.1f, 5f);
            DrawNumericSetting(list, "  Omnivore Mult", ref omnivoreHungerMult, ref oHungerBuf, 0.1f, 5f);
            list.GapLine();
            DrawNumericSetting(list, "General Output Mult", ref generalOutputMult, ref gOutBuf, 0.1f, 10f);

            list.End();
        }

        private void DrawNumericSetting(Listing_Standard list, string label, ref float value, ref string buffer, float min, float max)
        {
            Rect rect = list.GetRect(30f);

            // 1. Initial Sync: If the buffer is null (first time opening), fill it with the current value
            if (buffer == null)
            {
                buffer = value.ToString("F2");
            }

            // 2. Draw Label
            Widgets.Label(rect.LeftPart(0.4f), label);

            // 3. Draw Slider
            Rect sliderRect = new Rect(rect.x + rect.width * 0.45f, rect.y, rect.width * 0.35f, rect.height);
            float sliderVal = Widgets.HorizontalSlider(sliderRect, value, min, max);

            // 4. Slider -> Input Sync
            // If the slider moves, we force the text box to update
            if (sliderVal != value)
            {
                value = sliderVal;
                buffer = value.ToString("F2");
            }

            // 5. Input -> Slider Sync
            // We pass the 'buffer' to the text field. If you type, it updates 'value'.
            Widgets.TextFieldNumeric<float>(rect.RightPart(0.15f), ref value, ref buffer, min, max);
        }

        private void ResetSettings()
        {
            baseAgeSpeed = DefaultAge;
            humanAgeMult = animalAgeMult = generalOutputMult = 1f;
            generalHungerMult = 0.5f;
            herbivoreHungerMult = 1.6f;
            carnivoreHungerMult = 1.8f;
            omnivoreHungerMult = 1.4f;
            ageBuf = hAgeBuf = aAgeBuf = gHungerBuf = hHungerBuf = cHungerBuf = oHungerBuf = gOutBuf = null;
        }
    }
}