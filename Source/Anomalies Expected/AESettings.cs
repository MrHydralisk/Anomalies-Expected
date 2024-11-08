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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ReplicationLimit, "ReplicationLimit", defaultValue: 0);
            Scribe_Values.Look(ref DespawnPiecesOnDestroy, "DespawnPiecesOnDestroy", defaultValue: false);
            Scribe_Values.Look(ref SpawnPiePieceNotification, "SpawnPiePieceNotification", defaultValue: true);
            Scribe_Values.Look(ref PoweBeamRequireBeamTarget, "PoweBeamRequireBeamTarget", defaultValue: true);
            Scribe_Values.Look(ref BeamTargetLetter, "BeamTargetLetter", defaultValue: true);
        }
    }
}

