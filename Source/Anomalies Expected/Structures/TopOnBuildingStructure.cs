using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class TopOnBuildingStructure
    {
        public TopOnBuildingStructureTypes type;
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
    }
}
