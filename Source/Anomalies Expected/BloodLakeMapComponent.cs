using RimWorld.Planet;
using System.Linq;
using Verse;

namespace AnomaliesExpected
{
    public class BloodLakeMapComponent : CustomMapComponent
    {
        private const int MinSpawnDistFromGateExit = 10;

        public Building_AEBloodLake Entrance;

        public Building_AEBloodLakeExit Exit;

        public Map SourceMap => (map.Parent as PocketMapParent)?.sourceMap;

        public BloodLakeMapComponent(Map map) : base(map)
        {
        }

        public override void MapGenerated()
        {
            Entrance = SourceMap?.listerThings?.ThingsOfDef(ThingDefOfLocal.AE_BloodLake).FirstOrDefault() as Building_AEBloodLake;
            Exit = map.listerThings.ThingsOfDef(ThingDefOfLocal.AE_BloodLakeExit).FirstOrDefault() as Building_AEBloodLakeExit;
            if (Entrance == null)
            {
                Log.Warning("BloodLake not found");
                return;
            }
            if (Exit == null)
            {
                Log.Error("BloodLakeExit not found");
                return;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref Entrance, "Entrance");
            Scribe_References.Look(ref Exit, "Exit");
        }
    }
}
