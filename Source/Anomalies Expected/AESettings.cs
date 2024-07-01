using Verse;

namespace AnomaliesExpected
{
    public class AESettings : ModSettings
    {
        public int ReplicationLimit = 0;
        public bool DespawnPiecesOnDestroy = false;
        public bool PoweBeamRequireBeamTarget = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ReplicationLimit, "ReplicationLimit", defaultValue: 0);
            Scribe_Values.Look(ref DespawnPiecesOnDestroy, "DespawnPiecesOnDestroy", defaultValue: false);
            Scribe_Values.Look(ref PoweBeamRequireBeamTarget, "PoweBeamRequireBeamTarget", defaultValue: true);
        }
    }
}

