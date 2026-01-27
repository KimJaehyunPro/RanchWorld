using UnityEngine;
using Verse;

namespace RealisticRanching
{
    public class RealisticRanchingMod : Mod
    {
        // Adding = null!; tells the compiler this starts null but will be assigned.
        public static RealisticRanchingSettings settings = null!;

        public RealisticRanchingMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<RealisticRanchingSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings.DoWindowContents(inRect);
        }

        public override string SettingsCategory() => "Realistic Ranching";
    }
}