using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AnomaliesExpected
{
    public class JobDriver_CalledMeatGrinder : JobDriver
    {
        private Thing MeatGrinder => base.TargetThingA;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.Reserve(MeatGrinder, job, 1, -1, null, errorOnFailed))
            {
                return true;
            }
            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedOrNull(TargetIndex.A);
            yield return WaitForActivate(2500);
            yield return new Toil
            {
                initAction = delegate
                {
                    Comp_MeatGrinder comp = MeatGrinder.TryGetComp<Comp_MeatGrinder>();
                    comp?.CheckMood(pawn);
                }
            };
        }

        private Toil WaitForActivate(int remainingTicks)
        {
            Toil toil = ToilMaker.MakeToil("WaitForActivate");
            toil.WithProgressBarToilDelay(TargetIndex.A, remainingTicks);
            Toil toil2 = toil;
            toil2.initAction = (Action)Delegate.Combine(toil2.initAction, (Action)delegate
            {
                toil.actor.pather.StopDead();
            });
            Toil toil3 = toil;
            toil3.tickAction = (Action)Delegate.Combine(toil3.tickAction, (Action)delegate
            {
                pawn.rotationTracker.FaceTarget(base.TargetA);
            });
            toil.handlingFacing = true;
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.socialMode = RandomSocialMode.Off;
            toil.defaultDuration = remainingTicks;
            return toil;
        }
    }
}
