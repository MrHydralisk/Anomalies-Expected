using RimWorld.Planet;
using Verse;

namespace AnomaliesExpected
{
    public class ChristmasTreeMapComponent : CustomMapComponent
    {
        public Building_AEChristmasTree Entrance;

        public Building_AEChristmasTreeExit Exit;

        private int tickOnDestroy;
        public int TickTillDestroy => tickOnDestroy - Find.TickManager.TicksGame;

        public Map SourceMap => (map.Parent as PocketMapParent)?.sourceMap;

        public ChristmasTreeMapComponent(Map map) : base(map)
        {
        }

        public override void MapGenerated()
        {
            Entrance = PocketMapUtility.currentlyGeneratingPortal as Building_AEChristmasTree;
            Exit = Entrance.exitBuilding;
            if (Entrance == null)
            {
                Log.Warning("ChristmasTree not found");
                return;
            }
            if (Exit == null)
            {
                Log.Error("ChristmasTreeExit not found");
                return;
            }
            tickOnDestroy = Find.TickManager.TicksGame + 300000;
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (Find.TickManager.TicksGame >= tickOnDestroy)
            {
                Entrance.DestroyPocketMap();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref tickOnDestroy, "tickOnDestroy");
            Scribe_References.Look(ref Entrance, "Entrance");
            Scribe_References.Look(ref Exit, "Exit");
        }
    }
}
