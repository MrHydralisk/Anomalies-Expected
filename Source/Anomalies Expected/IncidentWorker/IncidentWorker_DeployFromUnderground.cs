using Mono.Unix.Native;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class IncidentWorker_DeployFromUnderground : IncidentWorker
    {
        public ThingDef DeployableObjectDef => Ext?.DeployableObjectDef;
        public ThingDef SkyfallerDef => Ext?.SkyfallerDef;
        public float ChanceFactorPowPerBuilding => Ext?.ChanceFactorPowPerBuilding ?? 1f;
        public List<ThingDefCountClass> ChanceFactorPowPerOtherBuildings => Ext?.ChanceFactorPowPerOtherBuildings;
        public bool isHaveArrow => Ext?.isHaveArrow ?? true;

        public IncidentDefExtension Ext => def.GetModExtension<IncidentDefExtension>();

        public static readonly LargeBuildingSpawnParms AnomalySpawnParms = new LargeBuildingSpawnParms
        {
            ignoreTerrainAffordance = true
        };

        public override float ChanceFactorNow(IIncidentTarget target)
        {
            if (!(target is Map map) || (ChanceFactorPowPerBuilding == 1 && (ChanceFactorPowPerOtherBuildings?.All(tdcc => tdcc.DropChance == 1) ?? true)))
            {
                return base.ChanceFactorNow(target);
            }
            float chance = base.ChanceFactorNow(target);
            int num = map.listerBuildings.allBuildingsNonColonist.Count((Building b) => b.def == DeployableObjectDef);
            if (num > 0)
            {
                chance *= Mathf.Pow(ChanceFactorPowPerBuilding, num);
            }
            foreach (ThingDefCountClass tdcc in ChanceFactorPowPerOtherBuildings)
            {
                int num1 = map.listerBuildings.allBuildingsNonColonist.Count((Building b) => b.def == tdcc.thingDef);
                if (num1 > 0)
                {
                    chance *= Mathf.Pow(tdcc.DropChance, num1);
                }
            }
            return chance;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (!LargeBuildingCellFinder.TryFindCell(out var cell, map, AnomalySpawnParms.ForThing(DeployableObjectDef)))
            {
                return false;
            }
            BuildingGroundSpawner buildingGroundSpawner = (BuildingGroundSpawner)ThingMaker.MakeThing(SkyfallerDef);
            Thing obj = buildingGroundSpawner.ThingToSpawn;
            obj.SetFaction(Faction.OfEntities);
            GenSpawn.Spawn(buildingGroundSpawner, cell, map);
            SendStandardLetter(def.letterLabel, def.letterText, def.letterDef ?? LetterDefOf.NeutralEvent, parms, isHaveArrow ? buildingGroundSpawner : null);
            return true;
        }

        //protected override bool TryExecuteWorker(IncidentParms parms)
        //{
        //    Map map = (Map)parms.target;
        //    List<Thing> things = new List<Thing>();
        //    foreach (ThingDef deployThingDef in DeployableThingDefs)
        //    {
        //        things.Add(ThingMaker.MakeThing(deployThingDef));
        //    }
        //    IntVec3 intVec = DropCellFinder.RandomDropSpot(map);
        //    ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
        //    foreach (Thing item2 in things)
        //    {
        //        activeDropPodInfo.innerContainer.TryAdd(item2);
        //    }
        //    activeDropPodInfo.openDelay = 110;
        //    activeDropPodInfo.leaveSlag = false;
        //    ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(ActiveDropPodDef);
        //    activeDropPod.Contents = activeDropPodInfo;
        //    SkyfallerMaker.SpawnSkyfaller(SkyfallerDef, activeDropPod, intVec, map);
        //    SendStandardLetter(def.letterLabel, def.letterText, def.letterDef ?? LetterDefOf.NeutralEvent, parms, new TargetInfo(intVec, map));
        //    return true;
        //}
    }
}
