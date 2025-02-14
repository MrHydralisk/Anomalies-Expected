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
        public List<ThingDefCountClass> ChanceFactorPowPerOtherBuildings = new List<ThingDefCountClass>();
        public bool isHaveArrow = true;
        public List<EntityCodexEntryDef> entityCodexEntryDefsRequired = new List<EntityCodexEntryDef>();
        public FactionDef factionDef;
        public List<PawnsArrivalModeDef> pawnsArrivalModeDef = new List<PawnsArrivalModeDef>();
        public List<PawnGroupKindDef> pawnGroupKindDef = new List<PawnGroupKindDef>();
        public int maxAnomalyThreatLevel = int.MaxValue;
    }
}
