using RimWorld;
using System.Collections.Generic;
using System.Linq;
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

        public override bool HideInteraction => (StudyUnlocks?.NextIndex ?? Props.minStudy) < Props.minStudy && !isCanDestroyEarly && !isCanDestroyForced;
        protected bool isCanDestroyEarly;
        public bool isCanDestroyForced;

        public ThingDefCountClass requiredThing => Props.requiredThings.FirstOrDefault();

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
            parent.Destroy((DestroyMode)Props.DestroyMode);
        }

        public override void OrderForceTarget(LocalTargetInfo target)
        {
            if (ValidateTarget(target, showMessages: false))
            {
                Job job;
                if (requiredThing == null)
                {
                    job = JobMaker.MakeJob(Props.jobDef, parent);
                    job.playerForced = true;
                    target.Pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                }
                else
                {
                    List<Thing> list = HaulAIUtility.FindFixedIngredientCount(target.Pawn, requiredThing.thingDef, requiredThing.count);
                    if (!list.NullOrEmpty())
                    {
                        job = JobMaker.MakeJob(Props.jobDef, parent, list[0]);
                        job.targetQueueB = (from i in list.Skip(1)
                                            select new LocalTargetInfo(i)).ToList();
                        job.count = requiredThing.count;
                        job.playerForced = true;
                        target.Pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    }
                }
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
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        OnInteracted(null);
                    },
                    defaultLabel = "Dev: Force activate",
                    defaultDesc = "Force activate destroy after study component"
                };
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

        public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
        {
            if (HideInteraction)
            {
                return false;
            }
            if (requiredThing != null)
            {
                if (activateBy != null)
                {
                    if (checkOptionalItems && !activateBy.HasReserved(requiredThing.thingDef) && !ReservationUtility.ExistsUnreservedAmountOfDef(parent.MapHeld, requiredThing.thingDef, Faction.OfPlayer, requiredThing.count, (Thing t) => activateBy.CanReserveAndReach(t, PathEndMode.Touch, Danger.None)))
                    {
                        return "ObeliskDeactivateMissingShards".Translate(requiredThing.Label);
                    }
                }
                else if (checkOptionalItems && !ReservationUtility.ExistsUnreservedAmountOfDef(parent.MapHeld, requiredThing.thingDef, Faction.OfPlayer, requiredThing.count))
                {
                    return "ObeliskDeactivateMissingShards".Translate(requiredThing.Label);
                }
            }
            return base.CanInteract(activateBy, checkOptionalItems);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isCanDestroyEarly, "isCanDestroyEarly", false);
            Scribe_Values.Look(ref isCanDestroyForced, "isCanDestroyForced", false);
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
