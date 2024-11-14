using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class GenStep_BloodLakeFleshbeasts : GenStep
    {
        public List<PawnKindCount> pawnKindsPerTiles = new List<PawnKindCount>();

        public override int SeedPart => 26098423;

        public override void Generate(Map map, GenStepParams parms)
        {
            foreach (PawnKindCount pawnKindCount in pawnKindsPerTiles)
            {
                int num = Mathf.RoundToInt(map.Size.ToIntVec2.Area / (float)pawnKindCount.count);
                for (int i = 0; i < num; i++)
                {
                    if (!CellFinder.TryFindRandomCell(map, (IntVec3 c) => Validator(c, map), out IntVec3 result))
                    {
                        continue;
                    }
                    Pawn newThing = PawnGenerator.GeneratePawn(pawnKindCount.kindDef, Faction.OfEntities);
                    GenSpawn.Spawn(newThing, result, map).TryGetComp(out CompCanBeDormant comp);
                    comp?.ToSleep();
                }
            }
        }

        private bool Validator(IntVec3 c, Map map)
        {
            if (!c.Standable(map))
            {
                return false;
            }
            if (c.DistanceToEdge(map) <= 2)
            {
                return false;
            }
            return true;
        }
    }
}

