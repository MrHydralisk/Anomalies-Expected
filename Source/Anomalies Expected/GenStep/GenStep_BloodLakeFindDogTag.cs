using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace AnomaliesExpected
{
    public class GenStep_BloodLakeFindDogTag : GenStep
    {
        public override int SeedPart => 12412314;

        public override void Generate(Map map, GenStepParams parms)
        {
            Map sourceMap = (map.Parent as PocketMapParent)?.sourceMap;
            List<Pawn> colonists = sourceMap?.mapPawns?.FreeColonistsSpawned;
            if (!colonists.NullOrEmpty())
            {
                foreach (Pawn pawn in colonists)
                {
                    IntVec3 pos = IntVec3.Invalid;
                    CellFinder.TryFindRandomCell(map, (IntVec3 c) => Validator(c, map), out pos);
                    if (pos != IntVec3.Invalid)
                    {
                        ThingWithComps thing = GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOfLocal.AE_DogTag), pos, map) as ThingWithComps;
                        Comp_DogTag dogTag = thing.GetComp<Comp_DogTag>();
                        dogTag.SetOwner(pawn);
                    }
                }
            }
            else
            {
                Log.Warning($"Failed to generate Dog Tags: {colonists == null} {colonists.NullOrEmpty()}");
            }
        }

        private bool Validator(IntVec3 c, Map map)
        {
            if (!GenGrid.InBounds(c, map, 2) || c.x < 0 || c.z < 0 || !c.Standable(map))
            {
                return false;
            }
            return true;
        }
    }
}

