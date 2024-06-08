using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class IncidentWorker_DeployThingFromOrbit : IncidentWorker
    {
        public List<ThingDef> DeployableThingDefs => Ext?.DeployableThingDefs;
        public ThingDef ActiveDropPodDef => Ext?.ActiveDropPodDef;
        public ThingDef SkyfallerDef => Ext?.SkyfallerDef;
        public float ChanceFactorPowPerThing => Ext?.ChanceFactorPowPerBuilding ?? 1f;

        public IncidentDefExtension Ext => def.GetModExtension<IncidentDefExtension>();

        public override float ChanceFactorNow(IIncidentTarget target)
        {
            if (!(target is Map map) || ChanceFactorPowPerThing == 1)
            {
                return base.ChanceFactorNow(target);
            }
            int num = map.listerThings.AllThings.Where((Thing t) => DeployableThingDefs.Contains(t.def)).Count();
            return ((num > 0) ? Mathf.Pow(ChanceFactorPowPerThing, num) : 1f) * base.ChanceFactorNow(target);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            List<Thing> things = new List<Thing>();
            foreach (ThingDef deployThingDef in DeployableThingDefs)
            {
                things.Add(ThingMaker.MakeThing(deployThingDef));
            }
            IntVec3 intVec = DropCellFinder.RandomDropSpot(map);
            ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
            foreach (Thing item2 in things)
            {
                activeDropPodInfo.innerContainer.TryAdd(item2);
            }
            activeDropPodInfo.openDelay = 110;
            activeDropPodInfo.leaveSlag = false;
            ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(ActiveDropPodDef);
            activeDropPod.Contents = activeDropPodInfo;
            SkyfallerMaker.SpawnSkyfaller(SkyfallerDef, activeDropPod, intVec, map);
            SendStandardLetter(def.letterLabel, def.letterText, def.letterDef ?? LetterDefOf.NeutralEvent, parms, new TargetInfo(intVec, map));
            return true;
        }
    }
}
