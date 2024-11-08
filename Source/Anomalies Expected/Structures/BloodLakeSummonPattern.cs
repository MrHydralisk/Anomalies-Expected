using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AnomaliesExpected
{

    public class BloodLakeSummonPattern
    {
        public string name;
        public IntRange intervalRange = new IntRange(60000, 180000);
        public bool assaultColony = false;
        public bool isRaid = false;
        public int resourcesAvailable = 1;
        public List<float> resourcesAvailableMult = new List<float>();
        public List<int> resourcesAvailableSummonsRequired = new List<int>();
        public List<PawnKindCount> pawnKindsWeighted = new List<PawnKindCount>();
        public List<PawnKindCount> pawnKindsForcedCount = new List<PawnKindCount>();
    }
}
