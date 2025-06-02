using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AnomaliesExpected
{
    public class IncidentDefExtension : DefModExtension
    {
        public ThingDef DeployableObjectDef;
        public List<ThingDef> DeployableThingDefs = new List<ThingDef>();
        public ThingDef ActiveDropPodDef;
        public ThingDef SkyfallerDef;
        public float ChanceFactorPowPerBuilding;
        public List<ThingDefCountClass> ChanceFactorPowPerOtherBuildings = new List<ThingDefCountClass>();
        public List<HediffDefFactor> ChanceFactorPowPerHediffDefs = new List<HediffDefFactor>();
        public bool isHaveArrow = true;
        public List<EntityCodexEntryDef> entityCodexEntryDefsRequired = new List<EntityCodexEntryDef>();
        public FactionDef factionDef;
        public int maxAnomalyThreatLevel = -1;
        public List<AssaultSummonPattern> AssaultSummonPattern = new List<AssaultSummonPattern>();
        public float FleckScale = 1;
    }
}
