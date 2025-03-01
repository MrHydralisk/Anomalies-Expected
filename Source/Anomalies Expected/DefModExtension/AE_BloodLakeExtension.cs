using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class AE_BloodLakeExtension : DefModExtension
    {
        public List<BloodLakeSummonPattern> summonPatterns = new List<BloodLakeSummonPattern>();
        public Color inactiveColor = new Color(0.392f, 0.117f, 0.117f);
        public Color activeColor = new Color(1, 0, 0);
        public int filthThickness;
        public ThingDef filthDef;
        public MapGeneratorDef mapGeneratorDef;
        public float aggressionRadius = 1;
    }
}
