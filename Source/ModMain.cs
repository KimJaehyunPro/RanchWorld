using UnityEngine;
using Verse;

namespace RanchWorld
{
    public class RanchWorldMod : Mod
    {
        public static RanchWorldSettings settings;
        public RanchWorldMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<RanchWorldSettings>();
            Log.Message("[RanchWorld] Initialized successfully.");
        }
        public override void DoSettingsWindowContents(Rect inRect) => settings.DoWindowContents(inRect);
        public override string SettingsCategory() => "RanchWorld";
    }
}