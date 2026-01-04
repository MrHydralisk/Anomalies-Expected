using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_TransformAfterStudy : CompProperties_Interactable
    {
        public ThingDef transformedDef;

        public CompProperties_TransformAfterStudy()
        {
            compClass = typeof(Comp_TransformAfterStudy);
        }
    }
}
