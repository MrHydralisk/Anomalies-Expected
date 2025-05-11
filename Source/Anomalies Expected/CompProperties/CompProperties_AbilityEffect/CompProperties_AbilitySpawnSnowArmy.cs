using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_AbilitySpawnSnowArmy : CompProperties_AbilityEffect
    {
        public PawnKindCount pawnKindCount;

        public int maxAmount;

        public HediffDef addHediff;

        public float snowRadius = 2.9f;

        public CompProperties_AbilitySpawnSnowArmy()
        {
            compClass = typeof(CompAbilityEffect_SpawnSnowArmy);
        }
    }
}
