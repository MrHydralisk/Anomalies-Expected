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
            ActiveTransporterInfo activeTransporterInfo = new ActiveTransporterInfo();
            foreach (Thing item2 in things)
            {
                activeTransporterInfo.innerContainer.TryAdd(item2);
            }
            activeTransporterInfo.openDelay = 110;
            activeTransporterInfo.leaveSlag = false;
            ActiveTransporter activeTransporter = (ActiveTransporter)ThingMaker.MakeThing(ActiveDropPodDef);
            activeTransporter.Contents = activeTransporterInfo;
            SkyfallerMaker.SpawnSkyfaller(SkyfallerDef, activeTransporter, intVec, map);
            SendStandardLetter(def.letterLabel, def.letterText, def.letterDef ?? LetterDefOf.NeutralEvent, parms, new TargetInfo(intVec, map));
            return true;
        }
    }
}
