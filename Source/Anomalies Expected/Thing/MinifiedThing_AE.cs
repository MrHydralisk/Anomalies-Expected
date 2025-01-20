using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class MinifiedThing_AE : MinifiedThing
    {

        public ThingOwner innerContainer => innerContainerCached ?? (innerContainerCached = GetDirectlyHeldThings());
        public ThingOwner innerContainerCached;

        public override void Tick()
        {
            if (Spawned)
            {
                InstallBlueprintUtility.CancelBlueprintsFor(this);
                innerContainer.TryDropAll(Position, Map, ThingPlaceMode.Near);
                this.Destroy();
            }
        }
    }
}
