using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_MeatGrinder : CompProperties_Interactable
    {
        public float nonHumanMult = 0.1f;
        public int tickPerBody = 90000;
        public int tickPerForced = 60000;
        public int tickMax = 1080000;
        public int tickMin = -60000;
        public float butcherEfficiency = 0.75f;
        public List<MeatGrinderMood> Moods;
        public JobDef jobDef;
        public SoundDef soundConsume;
        public CompProperties_MeatGrinder()
        {
            compClass = typeof(Comp_MeatGrinder);
        }
    }

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
