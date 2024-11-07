using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AnomaliesExpected
{

    public class BloodLakeSummonPattern
    {
        public IntRange intervalRange = new IntRange(60000, 180000);
        public bool assaultColony = false;
        public int resourcesAvailable = 1;
        public List<float> resourcesAvailableMult = new List<float>();
        public List<PawnKindDefCount> pawnKinds = new List<PawnKindDefCount>();
    }
}
