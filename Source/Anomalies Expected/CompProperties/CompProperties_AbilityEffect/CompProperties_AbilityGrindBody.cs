using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_AbilityGrindBody : CompProperties_AbilityEffect
    {
        public float butcherEfficiency = 2f;

        public CompProperties_AbilityGrindBody()
        {
            compClass = typeof(CompAbilityEffect_GrindBody);
        }
    }
}
