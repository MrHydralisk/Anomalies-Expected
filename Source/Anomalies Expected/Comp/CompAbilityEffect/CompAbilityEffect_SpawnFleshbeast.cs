using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class CompAbilityEffect_SpawnFleshbeast : CompAbilityEffect_SpawnSummon
    {
        public new CompProperties_AbilitySpawnFleshbeast Props => (CompProperties_AbilitySpawnFleshbeast)props;

        public override void ApplyPerEach(Pawn summon, LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.ApplyPerEach(summon, target, dest);
            FilthMaker.TryMakeFilth(target.Cell, pawn.Map, ThingDefOf.Filth_CorpseBile);
        }
    }
}
