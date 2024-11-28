using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class HediffComp_FleshbeastEmergeOnStage : HediffComp
    {
        public HediffCompProperties_FleshbeastEmergeOnStage Props => (HediffCompProperties_FleshbeastEmergeOnStage)props;

        private int TickNextEmerge;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (parent.CurStageIndex >= Props.initialStage && Find.TickManager.TicksGame >= TickNextEmerge)
            {
                TickNextEmerge = Find.TickManager.TicksGame + Props.ticksBetweenSpawn.RandomInRange;
                FleshbeastEmerge();
            }
        }

        private void FleshbeastEmerge()
        {
            if (ModsConfig.AnomalyActive)
            {
                Pawn pawn = parent.pawn;
                Pawn pawn2 = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Trispike, Faction.OfEntities, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, 0f, 0f));
                GenSpawn.Spawn(pawn2, CellFinder.StandableCellNear(pawn.Position, pawn.Map, 2f), pawn.Map);
                Messages.Message("AnomaliesExpected.Fleshmass.FleshbeastEmergeOnStage.Message".Translate(pawn.Label), pawn, MessageTypeDefOf.ThreatSmall);
            }
        }

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref TickNextEmerge, "TickNextEmerge", -1);
        }
    }
}
