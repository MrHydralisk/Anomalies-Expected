﻿using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AnomaliesExpected
{
    public class Comp_CanDestroyedAfterStudy : CompInteractable
    {
        public new CompProperties_CanDestroyedAfterStudy Props => (CompProperties_CanDestroyedAfterStudy)props;

        protected CompAEStudyUnlocks StudyUnlocks => studyUnlocksCached ?? (studyUnlocksCached = parent.TryGetComp<CompAEStudyUnlocks>());
        private CompAEStudyUnlocks studyUnlocksCached;

        public override bool HideInteraction => (StudyUnlocks?.NextIndex ?? Props.minStudy) < Props.minStudy;

        public void DestroyAnomaly()
        {
            parent.Destroy();
        }

        public override void OrderForceTarget(LocalTargetInfo target)
        {
            if (ValidateTarget(target, showMessages: false))
            {
                Job job = JobMaker.MakeJob(Props.jobDef, parent);
                target.Pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }
        }

        protected override void OnInteracted(Pawn caster)
        {
            if (Props.fleckOnAnomaly != null)
            {
                FleckMaker.Static(parent.Position, parent.Map, Props.fleckOnAnomaly, Props.fleckOnAnomalyScale);
            }
            DestroyAnomaly();
        }

        public override string CompInspectStringExtra()
        {
            TaggedString taggedString;
            if (!Active && Props.maintainProgress && progress > 0f)
            {
                if (Props.remainingSecondsInInspectString)
                {
                    taggedString += "AnomaliesExpected.BeamTarget.DestroyedAfterStudy".Translate() + ": " + Mathf.FloorToInt((float)TicksToActivate * (1f - progress)).ToStringSecondsFromTicks("F0");
                }
                else
                {
                    taggedString += "AnomaliesExpected.BeamTarget.DestroyedAfterStudy".Translate() + ": " + progress.ToStringPercent();
                }
            }
            return taggedString.Resolve();
        }
    }
}
