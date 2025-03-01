using Verse;

namespace AnomaliesExpected
{
    public class GenStep_ChristmasTreeTerrain : GenStep
    {
        public TerrainDef terrainDefDefault;
        public override int SeedPart => 262606459;

        public override void Generate(Map map, GenStepParams parms)
        {
            TerrainGrid terrainGrid = map.terrainGrid;
            foreach (IntVec3 cell in map.AllCells)
            {
                terrainGrid.SetTerrain(cell, terrainDefDefault);
            }
        }
    }
}

