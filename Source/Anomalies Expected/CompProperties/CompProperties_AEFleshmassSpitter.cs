using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_AEFleshmassSpitter : CompProperties
    {
        public IntRange SpitIntervalInitRangeTicks = new IntRange(5000, 7500);
        public IntRange SpitIntervalRangeTicks = new IntRange(5000, 7500);
        public bool isAbleToFireThroughRoof;

        public CompProperties_AEFleshmassSpitter()
        {
            compClass = typeof(Comp_AEFleshmassSpitter);
        }
    }
}
