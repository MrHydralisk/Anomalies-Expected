using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AnomaliesExpected
{
    public class IncidentDefExtension : DefModExtension
    {
        public ThingDef DeployableObjectDef;
        public List<ThingDef> DeployableThingDefs;
        public ThingDef ActiveDropPodDef;
        public ThingDef SkyfallerDef;
        public float ChanceFactorPowPerBuilding;
        public List<ThingDefCountClass> ChanceFactorPowPerOtherBuildings;
        public bool isHaveArrow = true;
        public List<EntityCodexEntryDef> entityCodexEntryDefsRequired;
    }
}
