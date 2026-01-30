using UnityEngine;
using Verse;

namespace RanchWorld
{
    public class RanchWorldSettings : ModSettings
    {
        public float hungerMultiplier = 0.5f;
        public float ageingMultiplier = 10f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref hungerMultiplier, "hungerMultiplier", 0.5f);
            Scribe_Values.Look(ref ageingMultiplier, "ageingMultiplier", 10f);
        }

        public void DoWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);
            list.Label($"Hunger Multiplier: {hungerMultiplier:F2}x");
            hungerMultiplier = list.Slider(hungerMultiplier, 0.1f, 5f);
            list.Label($"Ageing Multiplier: {ageingMultiplier:F2}x");
            ageingMultiplier = list.Slider(ageingMultiplier, 1f, 100f);
            list.End();
        }
    }
}