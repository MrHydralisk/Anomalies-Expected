using RimWorld;
using UnityEngine;
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
            options.Label("AnomaliesExpected.Settings.BakingPies.ReplicationLimit".Translate(Settings.ReplicationLimit.ToString()));
            Settings.ReplicationLimit = (int)options.Slider(Settings.ReplicationLimit, 0, 5000);
            options.CheckboxLabeled("AnomaliesExpected.Settings.BakingPies.DespawnPiecesOnDestroy".Translate().RawText, ref Settings.DespawnPiecesOnDestroy);
            options.CheckboxLabeled("AnomaliesExpected.Settings.BakingPies.SpawnPiePieceNotification".Translate().RawText, ref Settings.SpawnPiePieceNotification);
            options.GapLine();
            options.CheckboxLabeled("AnomaliesExpected.Settings.BeamTarget.PoweBeamRequireBeamTarget".Translate().RawText, ref Settings.PoweBeamRequireBeamTarget);
            options.CheckboxLabeled("AnomaliesExpected.Settings.BeamTarget.BeamTargetLetter".Translate().RawText, ref Settings.BeamTargetLetter);
            options.GapLine();
            options.Label("AnomaliesExpected.Settings.BloodLake.UndergroundFleshmassNestMult".Translate(Settings.UndergroundFleshmassNestMult.ToString()));
            Settings.UndergroundFleshmassNestMult = Mathf.Round(options.Slider(Settings.UndergroundFleshmassNestMult, 0.1f, 2f) * 10f) / 10f;
            options.Label("AnomaliesExpected.Settings.BloodLake.UndergroundFleshmassNestFrequencyMult".Translate(Settings.UndergroundFleshmassNestFrequencyMult.ToString()));
            Settings.UndergroundFleshmassNestFrequencyMult = Mathf.Round(options.Slider(Settings.UndergroundFleshmassNestFrequencyMult, 0.1f, 2f) * 10f) / 10f;
            options.Label("AnomaliesExpected.Settings.BloodLake.BloodLakeSubMapMaxSize".Translate(Settings.BloodLakeSubMapMaxSize.ToString()));
            Settings.BloodLakeSubMapMaxSize = Mathf.RoundToInt(options.Slider(Settings.BloodLakeSubMapMaxSize, 50, 325));
            options.GapLine();
            if (Current.Game != null && options.ButtonText("AnomaliesExpected.Settings.ResearchTab.Unlock".Translate().RawText))
            {
                Find.ResearchManager.Notify_MonolithLevelChanged(Find.Anomaly.Level);
            }
            options.End();
        }

        public override string SettingsCategory()
        {
            return "AnomaliesExpected.Settings.Title".Translate().RawText;
        }
    }
}
