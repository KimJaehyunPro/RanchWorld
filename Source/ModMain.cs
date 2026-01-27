using UnityEngine;
using Verse;

namespace RealisticRanching
{
    public class RealisticRanchingMod : Mod
    {
        public static RealisticRanchingSettings settings;

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