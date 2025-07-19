using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class GenStep_BloodLakeFleshSacks : GenStep
    {
        private int sackClumpSize = 50;

        private int numFleshSacksPerTiles = 2000;

        private int forceAtLeastShards = 2;

        private bool spawnSurroundingFleshmass = true;

        public override int SeedPart => 1234731256;

        public override void Generate(Map map, GenStepParams parms)
        {
            Building Exit = map.listerThings.ThingsOfDef(ThingDefOfLocal.AE_BloodLakeExit).FirstOrDefault() as Building;
            int num = Mathf.RoundToInt(map.Size.ToIntVec2.Area / (float)numFleshSacksPerTiles);
            for (int i = 0; i < num; i++)
            {
                if (!CellFinder.TryFindRandomCell(map, (IntVec3 c) => Validator(c, map, ThingDefOf.FleshSack), out var result))
                {
                    continue;
                }
                if (spawnSurroundingFleshmass)
                {
                    foreach (IntVec3 pos in GridShapeMaker.IrregularLump(result, map, sackClumpSize))
                    {
                        if (Validator(pos, map, ThingDefOf.Fleshmass) && (Exit == null || pos.DistanceTo(Exit.Position) > 5))
                        {
                            GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Fleshmass), pos, map, Rot4.Random).SetFaction(Faction.OfEntities);
                        }
                        map.terrainGrid.SetTerrain(pos, TerrainDefOf.Flesh);
                    }
                }
                List<Thing> list;
                if (i < forceAtLeastShards)
                {
                    list = Gen.YieldSingle(ThingMaker.MakeThing(ThingDefOf.Shard)).ToList();
                }
                else
                {
                    ThingSetMakerParams parms2 = default(ThingSetMakerParams);
                    parms2.qualityGenerator = QualityGenerator.Reward;
                    parms2.makingFaction = Faction.OfEntities;
                    list = ThingSetMakerDefOf.MapGen_FleshSackLoot.root.Generate(parms2);
                }
                Building_Casket building_Casket = ThingMaker.MakeThing(ThingDefOf.FleshSack) as Building_Casket;
                GenSpawn.Spawn(building_Casket, result, map);
                building_Casket.SetFaction(Faction.OfEntities);
                for (int num2 = list.Count - 1; num2 >= 0; num2--)
                {
                    Thing thing = list[num2];
                    if (!building_Casket.TryAcceptThing(thing, allowSpecialEffects: false))
                    {
                        thing.Destroy();
                    }
                }
            }
        }

        private bool Validator(IntVec3 c, Map map, ThingDef thingDef)
        {
            foreach (IntVec3 pos in GenAdj.CellsOccupiedBy(c, Rot4.North, thingDef.size))
            {
                if (!GenGrid.InBounds(pos, map) || pos.DistanceToEdge(map) <= 2 || pos.Roofed(map) || pos.GetEdifice(map)?.def != ThingDefOf.Fleshmass)
                {
                    return false;
                }
            }
            return true;
        }
    }
}

