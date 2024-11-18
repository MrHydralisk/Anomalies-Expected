using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_BloodPump : CompProperties
    {
        public int TickPerSpawn;
        public SoundDef soundWorking;

        public CompProperties_BloodPump()
        {
            compClass = typeof(Comp_BloodPump);
        }
    }
}
