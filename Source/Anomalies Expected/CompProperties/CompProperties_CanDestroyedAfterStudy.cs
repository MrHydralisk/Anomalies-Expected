using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_CanDestroyedAfterStudy : CompProperties_Interactable
    {
        public int minStudy = 5;
        public JobDef jobDef;

        public FleckDef fleckOnAnomaly;
        public float fleckOnAnomalyScale = 1;

        public CompProperties_CanDestroyedAfterStudy()
        {
            compClass = typeof(Comp_CanDestroyedAfterStudy);
        }
    }
}
