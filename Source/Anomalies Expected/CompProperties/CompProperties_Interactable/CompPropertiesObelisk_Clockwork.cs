using RimWorld;
using System.Collections.Generic;

namespace AnomaliesExpected
{
    public class CompPropertiesObelisk_Clockwork : CompProperties_Interactable
    {
        public List<TopOnBuildingStructure> topOnBuildingStructures;

        public CompPropertiesObelisk_Clockwork()
        {
            compClass = typeof(CompObelisk_Clockwork);
        }
    }
}
