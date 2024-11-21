using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class JobDriver_ApplyUsableTo : JobDriver
    {
        private const TargetIndex ItemInd = TargetIndex.A;

        private const TargetIndex PawnTargetInd = TargetIndex.B;

        private Mote warmupMote;

        private Pawn PawnTarget => (Pawn)job.GetTarget(PawnTargetInd).Thing;

        private Thing Item => job.GetTarget(ItemInd).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.Reserve(PawnTarget, job, 1, -1, null, errorOnFailed))
            {
                return pawn.Reserve(Item, job, 1, -1, null, errorOnFailed);
            }
            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
            CompUsable compUsable = Item.TryGetComp<CompUsable>();
            CompUseEffect compUseEffect = Item.TryGetComp<CompUseEffect>();
            this.FailOn(() => !compUseEffect.CanBeUsedBy(PawnTarget));
            yield return Toils_Goto.GotoThing(ItemInd, PathEndMode.Touch).FailOnDespawnedOrNull(ItemInd).FailOnDespawnedOrNull(PawnTargetInd);
            yield return Toils_Haul.StartCarryThing(ItemInd);
            yield return Toils_Goto.GotoThing(PawnTargetInd, PathEndMode.Touch).FailOnDespawnedOrNull(PawnTargetInd);
            Toil wait = Toils_General.Wait(compUsable.Props.useDuration);
            wait.initAction = delegate
            {
                PawnUtility.ForceWait(PawnTarget, 15000, null, maintainPosture: true);
            };
            wait.WithProgressBarToilDelay(PawnTargetInd);
            wait.FailOnDespawnedOrNull(PawnTargetInd);
            wait.FailOnCannotTouch(PawnTargetInd, PathEndMode.Touch);
            wait.tickAction = delegate
            {
                CompUsable compUsable = Item.TryGetComp<CompUsable>();
                if (compUsable != null && warmupMote == null && compUsable.Props.warmupMote != null)
                {
                    warmupMote = MoteMaker.MakeAttachedOverlay(PawnTarget, compUsable.Props.warmupMote, Vector3.zero);
                }
                warmupMote?.Maintain();
            };
            wait.AddFinishAction(delegate
            {
                if (PawnTarget != null && PawnTarget.CurJobDef == JobDefOf.Wait_MaintainPosture)
                {
                    PawnTarget.jobs.EndCurrentJob(JobCondition.InterruptForced);
                }
            });
            yield return wait;
            yield return Toils_General.Do(ApplyUsable);
        }

        private void ApplyUsable()
        {
            CompUseEffect compUseEffect = Item.TryGetComp<CompUseEffect>();
            if (compUseEffect == null)
            {
                Log.Error($"CompUseEffect for {Item.Label} is missing");
                return;
            }
            SoundDefOf.MechSerumUsed.PlayOneShot(SoundInfo.InMap(PawnTarget));
            Messages.Message("AnomaliesExpected.Jobs.ApplyUsableTo.Applied".Translate(Item.Label, PawnTarget.Label), PawnTarget, MessageTypeDefOf.PositiveEvent);
            compUseEffect.DoEffect(PawnTarget);
            Item.SplitOff(1).Destroy();
        }
    }
}
