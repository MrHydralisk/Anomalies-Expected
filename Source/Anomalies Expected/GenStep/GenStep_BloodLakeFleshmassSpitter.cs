using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class GenStep_BloodLakeFleshmassSpitter : GenStep
    {
        private int numFleshSpitterPerTiles = 1000;

        private ThingDef spitterDef;
        private bool isEasyAccess;

        public override int SeedPart => 1234731256;

        public override void Generate(Map map, GenStepParams parms)
        {
            if (spitterDef == null)
            {
                spitterDef = ThingDefOf.FleshmassSpitter;
            }
            int num = Mathf.RoundToInt(map.Size.ToIntVec2.Area / (float)numFleshSpitterPerTiles);
            for (int i = 0; i < num; i++)
            {
                if (!CellFinder.TryFindRandomCell(map, (IntVec3 c) => Validator(c, map), out IntVec3 result))
                {
                    continue;
                }
                Building building = ThingMaker.MakeThing(spitterDef) as Building;
                GenSpawn.Spawn(building, result, map);
                building.SetFaction(Faction.OfEntities);
            }
        }

        private bool Validator(IntVec3 c, Map map)
        {
            if (c.DistanceToEdge(map) <= 2)
            {
                return false;
            }
            foreach (IntVec3 pos in GenAdj.CellsOccupiedBy(c, Rot4.North, spitterDef.size))
            {
                if (GenGrid.InBounds(pos, map) && (pos.GetEdifice(map)?.def != ThingDefOf.Fleshmass || pos.Roofed(map)))
                {
                    return false;
                }
            }
            if (isEasyAccess && !GenAdj.CellsAdjacentCardinal(c, Rot4.North, spitterDef.size).Any((IntVec3 pos) => GenGrid.InBounds(pos, map) && pos.Standable(map)))
            {
                return false;
            }
            foreach (IntVec3 pos in GenRadial.RadialCellsAround(c, 4, true))
            {
                Building building;
                if (GenGrid.InBounds(pos, map) && (building = pos.GetEdifice(map)) != null && building.def == spitterDef)
                {
                    return false;
                }
            }
            return true;
        }
    }
}

