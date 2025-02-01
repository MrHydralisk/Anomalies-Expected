using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_UseEffectBloodLiquidInjector : CompProperties_UseEffect
    {
        public int amountOfInjuries = 3;
        public FloatRange tendQualityRange;
        public float bloodLossOffset = 0.35f;
        public HediffDef sideEffectHediff;
        public float sideEffectHediffSeverityAdd = 0.1f;

        public CompProperties_UseEffectBloodLiquidInjector()
        {
            compClass = typeof(CompUseEffect_BloodLiquidInjector);
        }
    }
}
