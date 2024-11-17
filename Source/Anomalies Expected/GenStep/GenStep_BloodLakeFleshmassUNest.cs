using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class GenStep_BloodLakeFleshmassUNest : GenStep
    {
        public int OnePerTiles = 50;
        public ThingDef BloodLakeUndergroundNest;
        private IntVec3 BloodLakeExitPos;
        private IntVec3 BloodLakeTerminalPos;

        public override int SeedPart => 12412314;

        public override void Generate(Map map, GenStepParams parms)
        {
            IntVec3 BloodLakeExitPos = map.listerThings.ThingsOfDef(ThingDefOfLocal.AE_BloodLakeExit).FirstOrDefault()?.Position ?? map.Center;
            IntVec3 BloodLakeTerminalPos = map.listerThings.ThingsOfDef(ThingDefOfLocal.AE_BloodLakeTerminal).FirstOrDefault()?.Position ?? map.Center;
            int num1 = Mathf.FloorToInt(map.Size.x / (float)OnePerTiles);
            int num2 = map.Size.x / num1;
            int num3 = num2 / 2;
            List<IntVec3> initPositions = new List<IntVec3>();
            for (int i = 0; i < num1; i++)
            {
                for (int j = 0; j < num1; j++)
                {
                    initPositions.Add(new IntVec3(num3 + i * num2, 0, num3 + j * num2));
                }
            }
            TerrainGrid terrainGrid = map.terrainGrid;
            foreach (IntVec3 initPos in initPositions)
            {
                IntVec3 pos = IntVec3.Invalid;
                CellFinder.TryFindRandomCellNear(initPos, map, num3, (IntVec3 c) => Validator(c, map), out pos, 100);
                if (pos != IntVec3.Invalid)
                {
                    foreach (IntVec3 item in GenAdj.CellsOccupiedBy(pos, Rot4.North, BloodLakeUndergroundNest.size))
                    {
                        foreach (Thing item2 in from t in item.GetThingList(map).ToList()
                                                where t.def.destroyable
                                                select t)
                        {
                            item2.Destroy();
                        }
                        terrainGrid.SetTerrain(item, TerrainDefOf.MetalTile);
                        MapGenerator.rootsToUnfog.Add(item);
                    }
                    GenSpawn.Spawn(ThingMaker.MakeThing(BloodLakeUndergroundNest), pos, map);
                }
            }
        }

        private bool Validator(IntVec3 c, Map map)
        {
            if (!GenGrid.InBounds(c, map, 2) || c.x < 0 || c.z < 0 || !c.Standable(map))
            {
                return false;
            }
            if (c.DistanceTo(BloodLakeExitPos) < 10 || c.DistanceTo(BloodLakeTerminalPos) < 10)
            {
                return false;
            }
            foreach (IntVec3 pos in GenAdj.CellsOccupiedBy(c, Rot4.North, BloodLakeUndergroundNest.size + IntVec2.Two + IntVec2.Two))
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

