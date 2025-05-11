using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AnomaliesExpected
{
    public class CompAbilityEffect_SpawnSnowArmy : CompAbilityEffect_SpawnSummon
    {
        public new CompProperties_AbilitySpawnSnowArmy Props => (CompProperties_AbilitySpawnSnowArmy)props;

        public override void ApplyPerEach(Pawn summon, LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.ApplyPerEach(summon, target, dest);
            List<IntVec3> cells = GenRadial.RadialCellsAround(target.Cell, Props.snowRadius, true).ToList();
            List<IntVec3> cellsAffected = new List<IntVec3>();
            foreach (IntVec3 cell in cells)
            {
                if (cell.InBounds(pawn.Map) && GenSight.LineOfSight(target.Cell, cell, pawn.Map, skipFirstCell: true))
                {
                    cellsAffected.Add(cell);
                }
            }
            foreach (IntVec3 cell in cellsAffected)
            {
                float lengthHorizontal = (target.Cell - cell).LengthHorizontal;
                float num2 = 1f - lengthHorizontal / Props.snowRadius;
                pawn.Map.snowGrid.AddDepth(cell, num2 * 1);
            }
        }
    }
}
