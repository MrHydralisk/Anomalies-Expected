using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_BakingPies : CompProperties_Interactable
    {
        public int tickPerSpawn = 60000;
        public ThingDef piePiece;
        public HediffDef ConsumptionHediffDef;
        public CompProperties_BakingPies()
        {
            compClass = typeof(Comp_BakingPies);
        }
    }
}
