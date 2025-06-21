using RimWorld;

namespace AnomaliesExpected
{
    public class CompProperties_AbilityGrindBody : CompProperties_AbilityEffect
    {
        public float butcherEfficiency = 2f;
        public float butcherEfficiencyRotten = 0.5f;

        public CompProperties_AbilityGrindBody()
        {
            compClass = typeof(CompAbilityEffect_GrindBody);
        }
    }
}
