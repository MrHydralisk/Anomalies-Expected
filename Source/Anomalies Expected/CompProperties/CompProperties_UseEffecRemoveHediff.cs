using RimWorld;

namespace AnomaliesExpected
{
    public class CompProperties_UseEffecRemoveHediff : CompProperties_UseEffectAddHediff
    {
        public float severity = 0.5f;

        public bool requireHediffToUse;

        public CompProperties_UseEffecRemoveHediff()
        {
            compClass = typeof(CompUseEffect_RemoveHediff);
        }
    }
}
