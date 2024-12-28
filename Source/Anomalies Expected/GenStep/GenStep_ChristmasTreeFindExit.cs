using Verse;

namespace AnomaliesExpected
{
    public class GenStep_ChristmasTreeFindExit : GenStep
    {
        public ThingDef ChristmasTreeExit;

        public override int SeedPart => 12412314;

        public override void Generate(Map map, GenStepParams parms)
        {
            IntVec3 center = map.Center;
            IntVec3 pos = center - new IntVec3(1, 0, 1);
            GenSpawn.Spawn(ThingMaker.MakeThing(ChristmasTreeExit), pos, map);
            MapGenerator.PlayerStartSpot = pos;
        }
    }
}

