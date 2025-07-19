using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class Building_AEChristmasTreeExit : Building_AEPocketMapExit
    {
        public ChristmasTreeMapComponent mapComponent => mapComponentCached ?? (mapComponentCached = Map?.GetComponent<ChristmasTreeMapComponent>() ?? null);
        private ChristmasTreeMapComponent mapComponentCached;
        public Building_AEChristmasTree entranceBuilding => entrance as Building_AEChristmasTree;

        public override string EnterString => "AnomaliesExpected.ChristmasStockings.Exit".Translate(Label);
        public override string EnteringString => "AnomaliesExpected.ChristmasStockings.Entering".Translate(Label);

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            Alert_ChristmasTreeUnstable.AddTarget(this);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            Alert_ChristmasTreeUnstable.RemoveTarget(this);
            base.Destroy(mode);
        }
    }
}
