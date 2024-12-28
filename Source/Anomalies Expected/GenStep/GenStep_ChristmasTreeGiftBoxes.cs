using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class GenStep_ChristmasTreeGiftBoxes : GenStep
    {
        private int amountOfGiftBoxes = 50;
        private ThingDef giftBoxDef;

        public override int SeedPart => 1234731256;

        public override void Generate(Map map, GenStepParams parms)
        {
            for (int i = 0; i < amountOfGiftBoxes; i++)
            {
                if (!CellFinder.TryFindRandomCell(map, (IntVec3 c) => Validator(c, map), out IntVec3 result))
                {
                    continue;
                }
                List<Thing> list;
                ThingSetMakerParams parms1 = default(ThingSetMakerParams);
                parms1.qualityGenerator = QualityGenerator.Reward;
                parms1.makingFaction = Faction.OfEntities;
                list = ThingSetMakerDefOf.MapGen_FleshSackLoot.root.Generate(parms1);
                Building_Casket building_Casket = ThingMaker.MakeThing(giftBoxDef) as Building_Casket;
                GenSpawn.Spawn(building_Casket, result, map);
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

