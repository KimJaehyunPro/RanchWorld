using UnityEngine;
using Verse;

namespace RanchWorld
{
    public class RanchWorldSettings : ModSettings
    {
        // Ageing
        public float baseAgeSpeed = 10f;
        public float humanAgeMult = 1f;
        public float animalAgeMult = 1f;

        // Hunger & Metabolism
        public float generalHungerMult = 0.5f;
        public float herbivoreHungerMult = 1.6f;
        public float carnivoreHungerMult = 1.8f;
        public float omnivoreHungerMult = 1.4f;

        // Production
        public float generalOutputMult = 1f;
        public float milkOutputMult = 1f;
        public float woolOutputMult = 1f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref baseAgeSpeed, "baseAgeSpeed", 10f);
            Scribe_Values.Look(ref humanAgeMult, "humanAgeMult", 1f);
            Scribe_Values.Look(ref animalAgeMult, "animalAgeMult", 1f);
            Scribe_Values.Look(ref generalHungerMult, "generalHungerMult", 0.5f);
            Scribe_Values.Look(ref herbivoreHungerMult, "herbivoreHungerMult", 1.6f);
            Scribe_Values.Look(ref carnivoreHungerMult, "carnivoreHungerMult", 1.8f);
            Scribe_Values.Look(ref omnivoreHungerMult, "omnivoreHungerMult", 1.4f);
            Scribe_Values.Look(ref generalOutputMult, "generalOutputMult", 1f);
            Scribe_Values.Look(ref milkOutputMult, "milkOutputMult", 1f);
            Scribe_Values.Look(ref woolOutputMult, "woolOutputMult", 1f);
        }

        public void DoWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);

            list.Label($"General Ageing: {baseAgeSpeed:F1}x");
            baseAgeSpeed = list.Slider(baseAgeSpeed, 0.01f, 100f);
            list.Label($"  Human Age Mult: {humanAgeMult:F2}x");
            humanAgeMult = list.Slider(humanAgeMult, 0.01f, 10f);
            list.Label($"  Animal Age Mult: {animalAgeMult:F2}x");
            animalAgeMult = list.Slider(animalAgeMult, 0.01f, 10f);

            list.GapLine();

            list.Label($"Global Hunger Mult: {generalHungerMult:F2}x");
            generalHungerMult = list.Slider(generalHungerMult, 0.1f, 5f);
            list.Label($"  Herbivore Activity (Diet): {herbivoreHungerMult:F2}x");
            herbivoreHungerMult = list.Slider(herbivoreHungerMult, 0.1f, 5f);
            list.Label($"  Carnivore Activity (Diet): {carnivoreHungerMult:F2}x");
            carnivoreHungerMult = list.Slider(carnivoreHungerMult, 0.1f, 5f);
            list.Label($"  Omnivore Activity (Diet): {omnivoreHungerMult:F2}x");
            omnivoreHungerMult = list.Slider(omnivoreHungerMult, 0.1f, 5f);

            list.GapLine();

            list.Label($"General Resource Output: {generalOutputMult:F2}x");
            generalOutputMult = list.Slider(generalOutputMult, 0.1f, 5f);
            list.Label($"  Milk Specific Mult: {milkOutputMult:F2}x");
            milkOutputMult = list.Slider(milkOutputMult, 0.1f, 5f);
            list.Label($"  Wool Specific Mult: {woolOutputMult:F2}x");
            woolOutputMult = list.Slider(woolOutputMult, 0.1f, 5f);

            list.End();
        }
    }
}