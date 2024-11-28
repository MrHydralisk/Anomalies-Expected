using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_AbilityFleshbeastCommand : CompProperties_AbilityEffect
    {
        public float gatherRadius;
        public bool isCall;

        public CompProperties_AbilityFleshbeastCommand()
        {
            compClass = typeof(CompAbilityEffect_FleshbeastCommand);
        }
    }
}
