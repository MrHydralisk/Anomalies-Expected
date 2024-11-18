using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_BloodSource : CompProperties
    {
        public ThingDef ResourceDef;
        public int ResourceAmount;
        public int MaxPumps = 1;
        public ResearchProjectDef UnlockedByResearchDef;

        public CompProperties_BloodSource()
        {
            compClass = typeof(Comp_BloodSource);
        }
    }
}
