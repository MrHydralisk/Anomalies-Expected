using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AnomaliesExpected
{
    public class IncidentWorker_DeployThingFromOrbit : IncidentWorker_AE
    {
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
