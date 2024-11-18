using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class PlaceWorker_ShowBloodPumpConnection : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            Comp_BloodSource bloodSource = Comp_BloodPump.NearbyBloodSource(loc, map, checkingDef.specialDisplayRadius);
            if (bloodSource != null)
            {
                return AcceptanceReport.WasAccepted;
            }
            return AcceptanceReport.WasRejected;
        }

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            Map currentMap = Find.CurrentMap;
            GenDraw.DrawFieldEdges(GenRadial.RadialCellsAround(center, def.specialDisplayRadius, true).ToList());
            Comp_BloodSource bloodSource = Comp_BloodPump.NearbyBloodSource(center, currentMap, def.specialDisplayRadius);
            if (bloodSource != null)
            {
                GenDraw.DrawLineBetween(GenThing.TrueCenter(center, rot, def.size, def.Altitude), bloodSource.parent.TrueCenter());
            }
        }
    }
}
