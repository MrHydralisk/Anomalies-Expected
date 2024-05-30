using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_BakingPies : CompProperties
    {
        public int tickPerSpawn = 60000;
        public ThingDef piePiece;
        public CompProperties_BakingPies()
        {
            compClass = typeof(Comp_BakingPies);
        }
    }
}
