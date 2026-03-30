using Verse;

namespace AnomaliesExpected
{
    public class AESettings : ModSettings
    {
        public int ReplicationLimit = 0;
        public bool DespawnPiecesOnDestroy = false;
        public bool SpawnPiePieceNotification = true;
        public bool PoweBeamRequireBeamTarget = true;
        public bool BeamTargetLetter = true;
        public float UndergroundFleshmassNestMult = 0.5f;
        public float UndergroundFleshmassNestRestore = 0.1f;
        public float UndergroundFleshmassNestFrequencyMult = 1f;
        public int BloodLakeSubMapMaxSize = 200;
        public int VoidProvactionCooldown = 120;
        public bool NotifyClockworkHandSecond = true;
        public bool NotifyClockworkHandMinute = true;
        public bool NotifyClockworkHandHour = true;
        public bool NotifyClockworkHandDay = true;
        public bool DevDisableClockworkObelisk = false;
        public bool DevModeInfo = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ReplicationLimit, "ReplicationLimit", defaultValue: 0);
            Scribe_Values.Look(ref DespawnPiecesOnDestroy, "DespawnPiecesOnDestroy", defaultValue: false);
            Scribe_Values.Look(ref SpawnPiePieceNotification, "SpawnPiePieceNotification", defaultValue: true);
            Scribe_Values.Look(ref PoweBeamRequireBeamTarget, "PoweBeamRequireBeamTarget", defaultValue: true);
            Scribe_Values.Look(ref BeamTargetLetter, "BeamTargetLetter", defaultValue: true);
            Scribe_Values.Look(ref UndergroundFleshmassNestMult, "UndergroundFleshmassNestMult", defaultValue: 0.5f);
            Scribe_Values.Look(ref UndergroundFleshmassNestRestore, "UndergroundFleshmassNestRestore", defaultValue: 0.1f);
            Scribe_Values.Look(ref UndergroundFleshmassNestFrequencyMult, "UndergroundFleshmassNestFrequencyMult", defaultValue: 1f);
            Scribe_Values.Look(ref BloodLakeSubMapMaxSize, "BloodLakeSubMapMaxSize", defaultValue: 200);
            Scribe_Values.Look(ref VoidProvactionCooldown, "VoidProvactionCooldown", defaultValue: 120);
            Scribe_Values.Look(ref NotifyClockworkHandSecond, "NotifyClockworkHandSecond", defaultValue: true);
            Scribe_Values.Look(ref NotifyClockworkHandMinute, "NotifyClockworkHandMinute", defaultValue: true);
            Scribe_Values.Look(ref NotifyClockworkHandHour, "NotifyClockworkHandHour", defaultValue: true);
            Scribe_Values.Look(ref NotifyClockworkHandDay, "NotifyClockworkHandDay", defaultValue: true);
            Scribe_Values.Look(ref DevDisableClockworkObelisk, "DevDisableClockworkObelisk", defaultValue: false);
            Scribe_Values.Look(ref DevModeInfo, "DevModeInfo", defaultValue: false);
        }
    }
}

