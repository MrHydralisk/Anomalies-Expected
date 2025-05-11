using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class HediffCompProperties_SpawnPawnOnDeath : HediffCompProperties_LetterOnDeath
    {
        public PawnKindCount pawnKindCount;
        public FactionDef factionDef;

        public HediffCompProperties_SpawnPawnOnDeath()
        {
            compClass = typeof(HediffComp_SpawnPawnOnDeath);
        }
    }
}
