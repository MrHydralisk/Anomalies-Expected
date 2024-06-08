using UnityEngine;
using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class AEMod : Mod
    {
        public static AESettings Settings { get; private set; }

        public AEMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<AESettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Listing_Standard options = new Listing_Standard();
            options.Begin(inRect);
            options.CheckboxLabeled("AnomaliesExpected.Settings.PoweBeamRequireBeamTarget".Translate().RawText, ref Settings.PoweBeamRequireBeamTarget);
            options.End();
        }

        public override string SettingsCategory()
        {
            return "AnomaliesExpected.Settings.Title".Translate().RawText;
        }
    }
}
