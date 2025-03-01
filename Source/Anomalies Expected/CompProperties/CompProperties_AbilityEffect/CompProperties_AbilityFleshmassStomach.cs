using RimWorld;

namespace AnomaliesExpected
{
    public class CompProperties_AbilityFleshmassStomach : CompProperties_AbilityEffect
    {
        public float damAmount;
        public float nutAmount;

        public CompProperties_AbilityFleshmassStomach()
        {
            compClass = typeof(CompAbilityEffect_FleshmassStomach);
        }
    }
}
