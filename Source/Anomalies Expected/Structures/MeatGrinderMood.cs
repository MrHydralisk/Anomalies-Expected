using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AnomaliesExpected
{
    public class MeatGrinderMood
    {
        public int tick;
        public int tickConsumed;
        public List<BodyPartDef> bodyPartDefs;
        public float butcherEfficiency;
        public bool isDanger;
        public int noise;
    }
}
