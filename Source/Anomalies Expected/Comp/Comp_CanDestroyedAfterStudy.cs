using RimWorld;
using System.Collections.Generic;
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

        public override bool HideInteraction => (StudyUnlocks?.NextIndex ?? Props.minStudy) < Props.minStudy && !isCanDestroyEarly;
        protected bool isCanDestroyEarly;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                isCanDestroyEarly = Props.DestroyUnlockResearchDef?.IsFinished ?? false;
            }
        }

        public virtual void DestroyAnomaly(Pawn caster = null)
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

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                if (gizmo is Command_Action command_Action)
                {
                    command_Action.hotKey = KeyBindingDefOf.Misc8;
                }
                yield return gizmo;
            }
        }

        protected override void OnInteracted(Pawn caster)
        {
            if (Props.fleckOnAnomaly != null)
            {
                FleckMaker.Static(parent.Position, parent.Map, Props.fleckOnAnomaly, Props.fleckOnAnomalyScale);
            }
            DestroyAnomaly(caster);
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref isCanDestroyEarly, "isCanDestroyEarly", false);
        }

        public override string CompInspectStringExtra()
        {
            TaggedString taggedString;
            if (!Active && Props.maintainProgress && progress > 0f)
            {
                if (Props.remainingSecondsInInspectString)
                {
                    taggedString += Props.interactionProgressString + ": " + Mathf.FloorToInt((float)TicksToActivate * (1f - progress)).ToStringSecondsFromTicks("F0");
                }
                else
                {
                    taggedString += Props.interactionProgressString + ": " + progress.ToStringPercent();
                }
            }
            return taggedString.Resolve();
        }
    }
}
