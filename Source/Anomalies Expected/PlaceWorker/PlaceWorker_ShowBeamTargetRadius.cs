using System.Linq;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class PlaceWorker_ShowBeamTargetRadius : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            Map currentMap = Find.CurrentMap;
            GenDraw.DrawFieldEdges(GenRadial.RadialCellsAround(center, 10, true).ToList());
        }
    }
}
