using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class Comp_BrokenStatue : CompInteractable, IThingHolder, IActivity
    {
        public new CompProperties_BrokenStatue Props => (CompProperties_BrokenStatue)props;

        private ThingOwner<Thing> innerContainer;
        public Pawn BrokenStatue => innerContainer.InnerListForReading.FirstOrDefault() as Pawn;
        public HediffComp_BrokenStatue BrokenStatueComp => BrokenStatueCompCached ?? (BrokenStatueCompCached = BrokenStatue.health.hediffSet.GetHediffComps<HediffComp_BrokenStatue>().FirstOrDefault());
        private HediffComp_BrokenStatue BrokenStatueCompCached;

        public CompAEStudyUnlocks StudyUnlocks => studyUnlocksCached ?? (studyUnlocksCached = parent.TryGetComp<CompAEStudyUnlocks>());
        private CompAEStudyUnlocks studyUnlocksCached;

        protected CompStudiable Studiable => studiableCached ?? (studiableCached = parent.TryGetComp<CompStudiable>());
        private CompStudiable studiableCached;

        public CompActivity ActivityComp => activityInt ?? (activityInt = parent.TryGetComp<CompActivity>());
        private CompActivity activityInt;

        public override bool HideInteraction => !Props.researchProjectDef.IsFinished;
        public bool isSuppressable = false;

        private IntVec3 sendLocation = IntVec3.Invalid;

        private Sustainer passiveSustainer;

        private static readonly SimpleCurve SustainerActivityCurve = new SimpleCurve
        {
            new CurvePoint(0.4f, 0.1f),
            new CurvePoint(0.9f, 1f)
        };

        private float TicksTillTransform;
        private float AssemblyProgress => 1 - TicksTillTransform / Props.ticksPerTransform;

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public Comp_BrokenStatue()
        {
            innerContainer = new ThingOwner<Thing>(this, LookMode.Deep, removeContentsIfDestroyed: false);
        }

        public override void PostPostMake()
        {
            base.PostPostMake();
            TicksTillTransform = Props.ticksPerTransform;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            isSuppressable = StudyUnlocks?.isStudyNoteManualUnlocked(2) ?? false;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (passiveSustainer == null || passiveSustainer.Ended)
            {
                passiveSustainer = Props.soundPassive.TrySpawnSustainer(SoundInfo.InMap(parent, MaintenanceType.PerTick));
            }
            passiveSustainer.info.volumeFactor = SustainerActivityCurve.Evaluate(ActivityComp.ActivityLevel);
            passiveSustainer.Maintain();
            TicksTillTransform -= 1 + ActivityComp.ActivityLevel * 3;
            if (TicksTillTransform <= 0)
            {
                Transform();
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                if (gizmo is Command_Action command_Action)
                {
                    command_Action.hotKey = KeyBindingDefOf.Misc6;
                }
                yield return gizmo;
            }
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        Transform();
                    },
                    defaultLabel = "Dev: Transform",
                    defaultDesc = "Transform into Brocken Statue"
                };
            }
        }

        public override void OrderForceTarget(LocalTargetInfo target)
        {
            if (ValidateTarget(target, showMessages: false))
            {
                TargetLocation(target.Pawn);
            }
        }

        private void TargetLocation(Pawn caster)
        {
            TargetingParameters targetingParameters = TargetingParameters.ForCell();
            targetingParameters.mapBoundsContractedBy = 1;
            targetingParameters.validator = (TargetInfo c) => c.Cell.InBounds(caster.Map) && !c.Cell.Fogged(caster.Map) && caster.Map.reachability.CanReach(parent.PositionHeld, c.Cell, PathEndMode.OnCell, TraverseMode.PassDoors);
            Find.Targeter.BeginTargeting(targetingParameters, delegate (LocalTargetInfo target)
            {
                sendLocation = target.Cell;
                base.OrderForceTarget(caster);
            }, delegate
            {
                Widgets.MouseAttachedLabel("AnomaliesExpected.BeamTarget.ChooseDest".Translate(parent.Label));
            });
        }

        protected override void OnInteracted(Pawn caster)
        {
            Job job = JobMaker.MakeJob(JobDefOfLocal.AE_BrokenStatueGoto, sendLocation);
            job.locomotionUrgency = LocomotionUrgency.Sprint;
            Transform()?.jobs.StartJob(job);
        }

        public Pawn Transform()
        {
            TargetInfo targetInfoFrom = new TargetInfo(parent.Position, parent.Map);
            Find.TickManager.slower.SignalForceNormalSpeedShort();
            if (AEMod.Settings.BrokenStatueLetter)
            {
                Find.LetterStack.ReceiveLetter("AnomaliesExpected.BeamTarget.LeftContainment".Translate(parent.LabelCap).RawText, "AnomaliesExpected.BeamTarget.LeftContainmentText".Translate(parent.LabelCap), LetterDefOf.ThreatSmall, targetInfoFrom);
            }
            else
            {
                Messages.Message("AnomaliesExpected.BeamTarget.LeftContainment".Translate(parent.LabelCap).RawText, targetInfoFrom, MessageTypeDefOf.NegativeEvent);
            }
            StudyUnlocks.UnlockStudyNoteManual(0);
            if (BrokenStatue.DestroyedOrNull())
            {
                innerContainer.TryAdd(PawnGenerator.GeneratePawn(Props.kindDef, Faction.OfEntities));
                BrokenStatue.TryGetComp<CompAEStudyUnlocks>()?.SetParentThing(parent);
            }
            IntVec3 intVec3 = parent.PositionHeld;
            Map map = parent.MapHeld;
            if (!Props.soundTransform.NullOrUndefined())
            {
                Props.soundTransform.PlayOneShotOnCamera();
            }
            BrokenStatue.Name = new NameSingle(parent.LabelCap);
            parent.DeSpawn();
            BrokenStatueComp.GetDirectlyHeldThings().TryAdd(parent);
            Pawn brokenStatue = BrokenStatue;
            if (!innerContainer.TryDrop(BrokenStatue, intVec3, map, ThingPlaceMode.Near, out var lastResultingThing))
            {
                if (!RCellFinder.TryFindRandomCellNearWith(intVec3, (IntVec3 c) => c.Standable(map), map, out var result, 1))
                {
                    Debug.LogError("Could not drop BrokenStatue!");
                }
                lastResultingThing = GenSpawn.Spawn(innerContainer.Take(BrokenStatue), result, map);
            }
            GetDirectlyHeldThings().RemoveAll(t => t == brokenStatue);
            return (Pawn)lastResultingThing;
        }

        public void OnTransform()
        {
            StudyUnlocks.UnlockStudyNoteManual(2);
            isSuppressable = true;
            ActivityComp.EnterPassiveState();
            TicksTillTransform = Props.ticksPerTransform;
        }

        public void OnActivityActivated()
        {
            Transform();
        }

        public void OnPassive()
        {
        }

        public bool ShouldGoPassive()
        {
            return false;
        }

        public bool CanBeSuppressed()
        {
            return isSuppressable;
        }

        public bool CanActivate()
        {
            return true;
        }

        public string ActivityTooltipExtra()
        {
            return "AnomaliesExpected.BrokenStatue.ActivityTooltipExtra".Translate();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && innerContainer.removeContentsIfDestroyed)
            {
                innerContainer.removeContentsIfDestroyed = false;
            }
            Scribe_Values.Look(ref TicksTillTransform, "TicksTillTransform", Find.TickManager.TicksGame);
        }

        public override string CompInspectStringExtra()
        {
            List<string> inspectStrings = new List<string>();
            if (isSuppressable)
            {
                inspectStrings.Add("AnomaliesExpected.BrokenStatue.AssemblyProgress".Translate(AssemblyProgress.ToStringPercent()).RawText);
            }
            inspectStrings.Add(base.CompInspectStringExtra());
            return String.Join("\n", inspectStrings);
        }
    }
}
