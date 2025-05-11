using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_AbilitySpawnSummon : CompProperties_AbilityEffect
    {
        public PawnKindCount pawnKindCount;

        public int maxAmount;

        public HediffDef addHediff;

        public CompProperties_AbilitySpawnSummon()
        {
            compClass = typeof(CompAbilityEffect_SpawnSummon);
        }
    }
}
