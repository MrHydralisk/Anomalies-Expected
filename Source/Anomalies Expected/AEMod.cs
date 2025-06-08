﻿using RimWorld;
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
            Settings.BloodLakeSubMapMaxSize = Mathf.RoundToInt(options.Slider(Settings.BloodLakeSubMapMaxSize, 75, 325));
            options.GapLine();
            options.Label("AnomaliesExpected.Settings.Patch.VoidProvactionCooldown".Translate(Settings.VoidProvactionCooldown.ToString()));
            Settings.VoidProvactionCooldown = Mathf.RoundToInt(options.Slider(Settings.VoidProvactionCooldown, 12, 120));
            options.GapLine();
            if (Current.Game != null && options.ButtonText("AnomaliesExpected.Settings.ResearchTab.Unlock".Translate().RawText))
            {
                Find.ResearchManager.Notify_MonolithLevelChanged(Find.Anomaly.Level);
            }
            if (Prefs.DevMode)
            {
                options.GapLine();
                options.CheckboxLabeled("AnomaliesExpected.Settings.DevMode.Info".Translate().RawText, ref Settings.DevModeInfo);
            }
            options.End();
        }

        public override string SettingsCategory()
        {
            return "AnomaliesExpected.Settings.Title".Translate().RawText;
        }
    }
}
