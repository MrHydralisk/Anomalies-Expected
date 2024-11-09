using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AnomaliesExpected
{

    public class BloodLakeSummonPatternStage
    {
        public int SummonsRequired = 0;
        public IntRange intervalRange = new IntRange(60000, 180000);
        public int resourcesAvailable = 1;
        public float resourcesAvailableMult = 1;
        public List<PawnKindCount> pawnKindsWeighted = new List<PawnKindCount>();
        public List<PawnKindCount> pawnKindsForcedCount = new List<PawnKindCount>();
    }
}
