using RimWorld;
using System.Linq;
using Verse;

namespace AnomaliesExpected
{
    public class IncidentWorker_DeployFromOrbit : IncidentWorker
    {
        public ThingDef DeployableObjectDef => Ext?.DeployableObjectDef;

        public IncidentDefExtension Ext => def.GetModExtension<IncidentDefExtension>();

        public override float ChanceFactorNow(IIncidentTarget target)
        {
            if (!(target is Map map))
            {
                return base.ChanceFactorNow(target);
            }
            int num = map.listerBuildings.allBuildingsNonColonist.Count((Building b) => b.def.GetCompProperties<CompProperties_Obelisk>() != null);
            return ((num > 0) ? ((float)num * 0.7f) : 1f) * base.ChanceFactorNow(target);
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            IntVec3 cell;
            return TryFindCell(out cell, map, DeployableObjectDef);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            Skyfaller skyfaller = SpawnObjectIncoming(map);
            if (skyfaller == null)
            {
                return false;
            }
            skyfaller.impactLetter = LetterMaker.MakeLetter(def.letterLabel, def.letterText, def.letterDef ?? LetterDefOf.NeutralEvent, new TargetInfo(skyfaller.Position, map));
            return true;
        }

        private Skyfaller SpawnObjectIncoming(Map map)
        {
            if (!TryFindCell(out var cell, map, DeployableObjectDef))
            {
                return null;
            }
            return SkyfallerMaker.SpawnSkyfaller(ThingDefOfLocal.AE_AnomalyIncoming, ThingMaker.MakeThing(DeployableObjectDef, DeployableObjectDef.defaultStuff), cell, map);
        }

        private static bool TryFindCell(out IntVec3 cell, Map map, ThingDef deployableObjectDef)
        {
            return CellFinderLoose.TryFindSkyfallerCell(ThingDefOfLocal.AE_AnomalyIncoming, map, out cell, 10, default(IntVec3), -1, allowRoofedCells: true, allowCellsWithItems: false, allowCellsWithBuildings: false, colonyReachable: false, avoidColonistsIfExplosive: true, alwaysAvoidColonists: true, delegate (IntVec3 x)
            {
                if ((float)x.DistanceToEdge(map) < 20f + (float)map.Size.x * 0.1f)
                {
                    return false;
                }
                foreach (IntVec3 item in CellRect.CenteredOn(x, deployableObjectDef.Size.x, deployableObjectDef.Size.z))
                {
                    if (!item.InBounds(map) || !item.Standable(map) || !item.GetTerrain(map).affordances.Contains(deployableObjectDef.terrainAffordanceNeeded))
                    {
                        return false;
                    }
                }
                return true;
            });
        }
    }
}
