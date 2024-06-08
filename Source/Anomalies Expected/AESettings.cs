using Verse;

namespace AnomaliesExpected
{
    public class AESettings : ModSettings
    {
        public bool PoweBeamRequireBeamTarget = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref PoweBeamRequireBeamTarget, "PoweBeamRequireBeamTarget", defaultValue: true);
        }
    }
}

