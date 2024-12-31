using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class GenStep_ChristmasTreeSnowMan : GenStep
    {
        private int decoSnowManCount = 15;
        private PawnKindCount pawnKindCount;

        public override int SeedPart => 26098423;

        public override void Generate(Map map, GenStepParams parms)
        {
            for (int i = 0; i < decoSnowManCount; i++)
            {
                if (!CellFinder.TryFindRandomCell(map, (IntVec3 c) => Validator(c, map), out IntVec3 result))
                {
                    continue;
                }
                GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Snowman), result, map);
            }
            for (int i = 0; i < pawnKindCount.count; i++)
            {
                if (!CellFinder.TryFindRandomCell(map, (IntVec3 c) => Validator(c, map), out IntVec3 result))
                {
                    continue;
                }
                Pawn newThing = PawnGenerator.GeneratePawn(pawnKindCount.kindDef, Faction.OfEntities);
                GenSpawn.Spawn(newThing, result, map);
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

