using System;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class TopOnBuildingStructure
    {
        public TopOnBuildingStructureTypes type;
        [TranslationHandle]
        public Type topOnBuildingClass = typeof(TopOnBuilding);
        [NoTranslate]
        public string texPath;
        public Vector3 drawOffset;
        public Material Material
        {
            get
            {
                if (material.NullOrBad())
                {
                    material = MaterialPool.MatFrom(texPath);
                }
                return material;
            }
        }
        private Material material = BaseContent.BadMat;
        public float drawSize = 1;

        public int tickPerFullRotation = 500;
        public int tickPerWarmup = -1;
    }
}
