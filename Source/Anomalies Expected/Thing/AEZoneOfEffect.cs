using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class AEZoneOfEffect : ThingWithComps
    {
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            Comps_PostDraw();
        }
    }
}
