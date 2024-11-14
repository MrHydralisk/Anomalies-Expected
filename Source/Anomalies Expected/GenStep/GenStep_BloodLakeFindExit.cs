using RimWorld;
using System.Linq;
using Verse;

namespace AnomaliesExpected
{
    public class GenStep_BloodLakeFindExit : GenStep
    {
        public int InitialRadius = 25;
        public ThingDef BloodLakeExit;

        public override int SeedPart => 12412314;

        public override void Generate(Map map, GenStepParams parms)
        {
            IntVec3 pos = IntVec3.Invalid;
            IntVec3 center = map.Center;
            int tries = 0;
            while (pos == IntVec3.Invalid && tries < 9)
            {
                tries++;
                CellFinder.TryFindRandomCellNear(center, map, InitialRadius * tries, (IntVec3 c) => Validator(c, map), out pos, 50);
            }
            foreach (IntVec3 item in GenAdj.CellsOccupiedBy(pos, Rot4.North, BloodLakeExit.size + IntVec2.Two))
            {
                foreach (Thing item2 in from t in item.GetThingList(map).ToList()
                                        where t.def.destroyable
                                        select t)
                {
                    item2.Destroy();
                }
                MapGenerator.rootsToUnfog.Add(item);
            }
            GenSpawn.Spawn(ThingMaker.MakeThing(BloodLakeExit), pos, map);
            MapGenerator.PlayerStartSpot = pos;
        }

        private bool Validator(IntVec3 c, Map map)
        {
            if (!GenGrid.InBounds(c, map) || !c.Standable(map))
            {
                return false;
            }
            foreach (IntVec3 pos in GenAdj.CellsOccupiedBy(c, Rot4.North, BloodLakeExit.size + IntVec2.Two))
            {
                if (!GenGrid.InBounds(pos, map) || pos.DistanceToEdge(map) <= 2 || !pos.Standable(map))
                {
                    return false;
                }
            }
            return true;
        }
    }
}

