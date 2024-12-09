using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class GenStep_BloodLakeTerrain : GenStep
    {
        public TerrainDef terrainDefDefault;
        public override int SeedPart => 262606459;

        public override void Generate(Map map, GenStepParams parms)
        {
            MapGenFloatGrid elevation = MapGenerator.Elevation;
            MapGenFloatGrid caves = MapGenerator.Caves;
            TerrainGrid terrainGrid = map.terrainGrid;
            foreach (IntVec3 cell in map.AllCells)
            {
                Building edifice = cell.GetEdifice(map);
                TerrainDef terrainDef = null;
                terrainDef = ((edifice == null || edifice.def.Fillage != FillCategory.Full) && !(caves[cell] > 0f)) ? TerrainFrom(cell, map, elevation[cell], false) : TerrainFrom(cell, map, elevation[cell], true);
                terrainGrid.SetTerrain(cell, terrainDef);
                if (terrainDef == TerrainDefOf.Sand)
                {
                    terrainGrid.SetTerrain(cell, terrainDefDefault);
                }
            }
        }

        private TerrainDef TerrainFrom(IntVec3 c, Map map, float elevation, bool preferSolid)
        {
            if (preferSolid || elevation >= 0.61f)
            {
                return GenStep_RocksFromGrid.RockDefAt(c).building.naturalTerrain;
            }
            if (elevation > 0.55f && elevation < 0.61f)
            {
                return TerrainDefOf.Gravel;
            }
            return TerrainDefOf.Sand;
        }
    }
}

