using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class IncidentWorker_DeployThingWithSkip : IncidentWorker_AE
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            List<Thing> things = new List<Thing>();
            if (DeployableObjectDef != null)
            {
                things.Add(ThingMaker.MakeThing(DeployableObjectDef));
            }
            foreach (ThingDef deployThingDef in DeployableThingDefs)
            {
                things.Add(ThingMaker.MakeThing(deployThingDef));
            }
            IntVec3 intVec = DropCellFinder.RandomDropSpot(map);
            foreach (Thing thing in things)
            {
                bool isPlaced = GenPlace.TryPlaceThing(thing, intVec, map, ThingPlaceMode.Near, null); if (isPlaced)
                {
                    TargetInfo targetInfo = new TargetInfo(thing.Position, thing.Map);
                    SoundDefOf.Psycast_Skip_Entry.PlayOneShot(targetInfo);
                    FleckMaker.Static(targetInfo.Cell, targetInfo.Map, FleckDefOf.PsycastSkipFlashEntry, Ext.FleckScale);
                }
            }
            SendStandardLetter(def.letterLabel, def.letterText, def.letterDef ?? LetterDefOf.NeutralEvent, parms, new TargetInfo(things.FirstOrDefault()?.Position ?? intVec, map));
            return true;
        }
    }
}
