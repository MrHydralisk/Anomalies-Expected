using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_ChristmasStockings : CompProperties_Interactable
    {
        public ThoughtDef thoughtDefPositive;
        public ThoughtDef thoughtDefNegative;
        public ThingDef lastGift;

        public int giftAmount = 15;

        public float goodPerKillsHumanlikes = -1;
        public float goodPerKillsAnimals = -0.2f;
        public float goodPerKillsMechanoids = 0.5f;
        public float goodPerKillsEntities = 0.5f;
        public float goodPerTimesTendedOther = 0.05f;

        public CompProperties_ChristmasStockings()
        {
            compClass = typeof(Comp_ChristmasStockings);
        }
    }
}
