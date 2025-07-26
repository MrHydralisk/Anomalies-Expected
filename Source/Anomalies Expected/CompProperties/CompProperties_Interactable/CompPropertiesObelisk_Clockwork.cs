using RimWorld;
using System.Collections.Generic;

namespace AnomaliesExpected
{
    public class CompPropertiesObelisk_Clockwork : CompProperties_Interactable
    {
        public List<TopOnBuildingStructure> topOnBuildingStructures;

        public int radius = 3;
        public int sizeLocation = 60;
        public float ticksFullRotationPerActiveTick = 2;

        public CompPropertiesObelisk_Clockwork()
        {
            compClass = typeof(CompObelisk_Clockwork);
        }
    }
}
