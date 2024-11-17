using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AnomaliesExpected
{
    public class GenStep_BloodLakeFindTerminal : GenStep
    {
        public int InitialRadius = 25;
        public ThingDef BloodLakeTerminal;

        public override int SeedPart => 12412314;

        public override void Generate(Map map, GenStepParams parms)
        {
            IntVec3 pos = IntVec3.Invalid;
            IntVec3 BloodLakeExitPos = map.listerThings.ThingsOfDef(ThingDefOfLocal.AE_BloodLakeExit).FirstOrDefault()?.Position ?? map.Center;
            List<IntVec3> initPositions = new List<IntVec3>() { new IntVec3(0, 0, 0), new IntVec3(map.Size.x - 1, 0, 0), new IntVec3(0, 0, map.Size.z - 1), new IntVec3(map.Size.x - 1, 0, map.Size.z - 1) };
            IntVec3 initPos = initPositions.RandomElement();
            initPositions.Remove(initPos);
            List<IntVec3> possiblePos = new List<IntVec3>();
            while (possiblePos.Count() < 10 && initPositions.Count() > 0)
            {
                CellFinder.TryFindRandomCellNear(initPos, map, map.Size.x / 2, (IntVec3 c) => Validator(c, map), out pos, 100);
                if (pos == IntVec3.Invalid)
                {
                    initPos = initPositions.RandomElement();
                    initPositions.Remove(initPos);
                }
                else
                {
                    possiblePos.Add(pos);
                }
            }
            pos = possiblePos.OrderByDescending((IntVec3 c) => c.DistanceTo(BloodLakeExitPos)).FirstOrDefault();
            TerrainGrid terrainGrid = map.terrainGrid;
            foreach (IntVec3 item in GenAdj.CellsOccupiedBy(pos, Rot4.North, BloodLakeTerminal.size + IntVec2.Two))
            {
                foreach (Thing item2 in from t in item.GetThingList(map).ToList()
                                        where t.def.destroyable
                                        select t)
                {
                    item2.Destroy();
                }
                terrainGrid.SetTerrain(item, TerrainDefOf.MetalTile);
                GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Fleshmass), item, map, Rot4.Random).SetFaction(Faction.OfEntities);
                MapGenerator.rootsToUnfog.Add(item);
            }
            GenSpawn.Spawn(ThingMaker.MakeThing(BloodLakeTerminal), pos, map);
        }

        private bool Validator(IntVec3 c, Map map)
        {
            if (!GenGrid.InBounds(c, map, 2) || c.x < 0 || c.z < 0 || c.DistanceToEdge(map) > 10 || !c.Standable(map))
            {
                return false;
            }
            foreach (IntVec3 pos in GenAdj.CellsOccupiedBy(c, Rot4.North, BloodLakeTerminal.size + IntVec2.Two))
            {
                if (!GenGrid.InBounds(pos, map) || pos.x < 0 || pos.z < 0 || pos.DistanceToEdge(map) <= 2 || !pos.Standable(map))
                {
                    return false;
                }
            }
            return true;
        }
    }
}

