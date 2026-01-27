using UnityEngine;
using Verse;
using RimWorld;

namespace RealisticRanching
{
    public class RealisticRanchingSettings : ModSettings
    {
        // Ageing
        public float ageingSpeedMultiplier = 10f;
        public float humanAgeingSpeedMultiplier = 1f;
        public float animalAgeingSpeedMultiplier = 1f;

        // Hunger Multipliers by Vanilla Diet Categories
        public float hungerHerbivorous = 0.5f;
        public float hungerDendrivorous = 0.5f; // Tree eaters (Alphabeavers)
        public float hungerOmnivorous = 0.5f;
        public float hungerCarnivorous = 0.5f;

        // Output Multipliers
        public float animalOutputMultiplier = 1f; // General
        public float milkOutputMultiplier = 1f;
        public float woolOutputMultiplier = 1f;
        public float leatherOutputMultiplier = 1f;
        public float chemfuelOutputMultiplier = 1f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ageingSpeedMultiplier, "ageingSpeedMultiplier", 10f);
            Scribe_Values.Look(ref humanAgeingSpeedMultiplier, "humanAgeingSpeedMultiplier", 1f);
            Scribe_Values.Look(ref animalAgeingSpeedMultiplier, "animalAgeingSpeedMultiplier", 1f);

            Scribe_Values.Look(ref hungerHerbivorous, "hungerHerbivorous", 0.5f);
            Scribe_Values.Look(ref hungerDendrivorous, "hungerDendrivorous", 0.5f);
            Scribe_Values.Look(ref hungerOmnivorous, "hungerOmnivorous", 0.5f);
            Scribe_Values.Look(ref hungerCarnivorous, "hungerCarnivorous", 0.5f);

            Scribe_Values.Look(ref animalOutputMultiplier, "animalOutputMultiplier", 1f);
            Scribe_Values.Look(ref milkOutputMultiplier, "milkOutputMultiplier", 1f);
            Scribe_Values.Look(ref woolOutputMultiplier, "woolOutputMultiplier", 1f);
            Scribe_Values.Look(ref leatherOutputMultiplier, "leatherOutputMultiplier", 1f);
            Scribe_Values.Look(ref chemfuelOutputMultiplier, "chemfuelOutputMultiplier", 1f);
        }

        public void DoWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            // --- Ageing Section ---
            listing.Label($"General Ageing Speed: {ageingSpeedMultiplier:F2}x");
            ageingSpeedMultiplier = listing.Slider(ageingSpeedMultiplier, 0.01f, 100f);
            listing.Label($"  - Human Ageing Multiplier: {humanAgeingSpeedMultiplier:F2}x");
            humanAgeingSpeedMultiplier = listing.Slider(humanAgeingSpeedMultiplier, 0.01f, 100f);
            listing.Label($"  - Animal Ageing Multiplier: {animalAgeingSpeedMultiplier:F2}x");
            animalAgeingSpeedMultiplier = listing.Slider(animalAgeingSpeedMultiplier, 0.01f, 100f);

            listing.GapLine();

            // --- Hunger Section (Vanilla Diets) ---
            listing.Label("Hunger Rate by Diet Category:");
            listing.Label($"  - Herbivorous: {hungerHerbivorous:F2}x");
            hungerHerbivorous = listing.Slider(hungerHerbivorous, 0.1f, 10f);
            listing.Label($"  - Dendrivorous: {hungerDendrivorous:F2}x");
            hungerDendrivorous = listing.Slider(hungerDendrivorous, 0.1f, 10f);
            listing.Label($"  - Omnivorous: {hungerOmnivorous:F2}x");
            hungerOmnivorous = listing.Slider(hungerOmnivorous, 0.1f, 10f);
            listing.Label($"  - Carnivorous: {hungerCarnivorous:F2}x");
            hungerCarnivorous = listing.Slider(hungerCarnivorous, 0.1f, 10f);

            listing.GapLine();

            // --- Output Section ---
            listing.Label($"General Animal Output: {animalOutputMultiplier:F2}x");
            animalOutputMultiplier = listing.Slider(animalOutputMultiplier, 0.1f, 10f);

            listing.Label($"  - Milk Multiplier: {milkOutputMultiplier:F2}x");
            milkOutputMultiplier = listing.Slider(milkOutputMultiplier, 0.1f, 10f);

            listing.Label($"  - Wool Multiplier: {woolOutputMultiplier:F2}x");
            woolOutputMultiplier = listing.Slider(woolOutputMultiplier, 0.1f, 10f);

            listing.Label($"  - Leather Multiplier: {leatherOutputMultiplier:F2}x");
            leatherOutputMultiplier = listing.Slider(leatherOutputMultiplier, 0.1f, 10f);

            listing.Label($"  - Chemfuel Multiplier: {chemfuelOutputMultiplier:F2}x");
            chemfuelOutputMultiplier = listing.Slider(chemfuelOutputMultiplier, 0.1f, 10f);

            listing.End();
        }
    }
}