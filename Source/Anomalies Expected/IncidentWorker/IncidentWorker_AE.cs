using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class IncidentWorker_AE : IncidentWorker
    {
        public ThingDef DeployableObjectDef => Ext?.DeployableObjectDef;
        public List<ThingDef> DeployableThingDefs => Ext?.DeployableThingDefs;
        public ThingDef ActiveDropPodDef => Ext?.ActiveDropPodDef;
        public ThingDef SkyfallerDef => Ext?.SkyfallerDef;
        public float ChanceFactorPowPerBuilding => Ext?.ChanceFactorPowPerBuilding ?? 1f;
        public List<ThingDefCountClass> ChanceFactorPowPerOtherBuildings => Ext?.ChanceFactorPowPerOtherBuildings;
        public List<HediffDefFactor> ChanceFactorPowPerHediffDefs => Ext?.ChanceFactorPowPerHediffDefs;
        public bool isHaveArrow => Ext?.isHaveArrow ?? true;
        public List<EntityCodexEntryDef> entityCodexEntryDefsRequired => Ext?.entityCodexEntryDefsRequired ?? null;

        public IncidentDefExtension Ext => def.GetModExtension<IncidentDefExtension>();

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (ModsConfig.AnomalyActive && !parms.forced)
            {
                if (Ext.maxAnomalyThreatLevel > -1 && (Find.Anomaly.LevelDef.anomalyThreatTier > Ext.maxAnomalyThreatLevel || !Find.Anomaly.GenerateMonolith))
                {
                    return false;
                }
                if (entityCodexEntryDefsRequired?.Any(eced => !Find.EntityCodex.Discovered(eced)) ?? false)
                {
                    return false;
                }
            }
            return base.CanFireNowSub(parms);
        }

        public override float ChanceFactorNow(IIncidentTarget target)
        {
            float chance = base.ChanceFactorNow(target);
            if (!(target is Map map))
            {
                return chance;
            }
            if (ChanceFactorPowPerBuilding != 1)
            {
                if (DeployableObjectDef != null)
                {
                    int num = map.listerThings.AllThings.Count((Thing t) => t.def == DeployableObjectDef);
                    if (num > 0)
                    {
                        chance *= Mathf.Pow(ChanceFactorPowPerBuilding, num);
                    }
                }
                if (!DeployableThingDefs.NullOrEmpty())
                {
                    int num = map.listerThings.AllThings.Where((Thing t) => DeployableThingDefs.Contains(t.def)).Count();
                    if (num > 0)
                    {
                        chance *= Mathf.Pow(ChanceFactorPowPerBuilding, num);
                    }
                }
            }
            if (!ChanceFactorPowPerOtherBuildings.NullOrEmpty() && ChanceFactorPowPerOtherBuildings.Any(tdcc => tdcc.DropChance != 1))
            {
                foreach (ThingDefCountClass tdcc in ChanceFactorPowPerOtherBuildings)
                {
                    int num = map.listerThings.AllThings.Count((Thing t) => t.def == tdcc.thingDef);
                    if (num > 0)
                    {
                        chance *= Mathf.Pow(tdcc.DropChance, num);
                    }
                }
            }
            if (!ChanceFactorPowPerHediffDefs.NullOrEmpty() && ChanceFactorPowPerHediffDefs.Any(hdf => hdf.Factor != 1))
            {
                foreach (HediffDefFactor hdf in ChanceFactorPowPerHediffDefs)
                {
                    int num = map.mapPawns.AllHumanlikeSpawned.Count((Pawn p) => p.health.hediffSet.GetFirstHediffOfDef(hdf.HediffDef) != null);
                    if (num > 0)
                    {
                        chance *= Mathf.Pow(hdf.Factor, num);

                    }
                }
            }
            return chance;
        }
    }
}
