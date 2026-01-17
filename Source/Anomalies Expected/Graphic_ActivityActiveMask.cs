using RimWorld;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class Graphic_ActivityActiveMask : Graphic_WithPropertyBlock
    {
        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            CompActivity compActivity = thing.TryGetComp<CompActivity>();
            if (compActivity == null)
            {
                Log.ErrorOnce(thingDef.defName + ": Graphic_ActivityActiveMask requires CompActivity.", 6134621);
                return;
            }
            Color value = colorTwo;
            value.a = compActivity.IsActive ? 1 : 0;
            propertyBlock.SetColor(ShaderPropertyIDs.ColorTwo, value);
            base.DrawWorker(loc, rot, thingDef, thing, extraRotation);
        }
    }
}
