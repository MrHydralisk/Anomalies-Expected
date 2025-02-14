using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class HediffCompProperties_SnowBlockCompressor : HediffCompProperties_LetterOnDeath
    {
        public float radius = 6.9f;
        public int ticksBetweenExtinguish = 2500;
        public Color color = new Color(0.31f, 0.69f, 0.835f, 0.5f);
        public FleckDef fleckDef;
        public SoundDef soundDef;
        public ThingDef ProjectileDef;
        public HediffDef ignoreWithHediffDef;

        public HediffCompProperties_SnowBlockCompressor()
        {
            compClass = typeof(HediffComp_SnowBlockCompressor);
        }
    }
}
