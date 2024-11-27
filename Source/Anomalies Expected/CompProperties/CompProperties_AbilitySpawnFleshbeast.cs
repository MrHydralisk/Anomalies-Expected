using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_AbilitySpawnFleshbeast : CompProperties_AbilityEffect
    {
        public PawnKindCount pawnKindCount;
        public HediffDef addHediff;

        public CompProperties_AbilitySpawnFleshbeast()
        {
            compClass = typeof(CompAbilityEffect_SpawnFleshbeast);
        }
    }
}
