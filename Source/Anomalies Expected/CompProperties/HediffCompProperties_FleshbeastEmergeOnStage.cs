using Verse;

namespace AnomaliesExpected
{
    public class HediffCompProperties_FleshbeastEmergeOnStage : HediffCompProperties
    {
        public IntRange ticksBetweenSpawn;
        public int initialStage;

        public HediffCompProperties_FleshbeastEmergeOnStage()
        {
            compClass = typeof(HediffComp_FleshbeastEmergeOnStage);
        }
    }
}
