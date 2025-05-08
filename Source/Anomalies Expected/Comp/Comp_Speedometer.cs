using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class Comp_Speedometer : CompInteractable
    {
        public new CompProperties_Speedometer Props => (CompProperties_Speedometer)props;
        protected CompAEStudyUnlocks StudyUnlocks => studyUnlocksCached ?? (studyUnlocksCached = parent.TryGetComp<CompAEStudyUnlocks>());
        private CompAEStudyUnlocks studyUnlocksCached;

        public override bool HideInteraction => !isDeceleratedOnce;

        public List<Pawn> deceleratedPawns = new List<Pawn>();
        public int TickNextAction = 0;
        public int TickCanRemove = 0;
        public int TickNextDeceleration = 0;
        public int UnlockedLevel = 1;
        public bool isDeceleratedOnce;
        public bool isExplodedOnce;
        public bool isDestabilizedOnce;

        public override void PostPostMake()
        {
            base.PostPostMake();
            TickNextDeceleration = Find.TickManager.TicksGame + (int)Props.DecelerationIntervalRange.Average;
            parent.overrideGraphicIndex = 0;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (StudyUnlocks.isStudyNoteManualUnlocked(0))
            {
                isDeceleratedOnce = true;
            }
            if (StudyUnlocks.isStudyNoteManualUnlocked(7))
            {
                isExplodedOnce = true;
            }
            if (StudyUnlocks.isStudyNoteManualUnlocked(8))
            {
                isDestabilizedOnce = true;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (Find.TickManager.TicksGame % 250 == 0 && Find.TickManager.TicksGame >= TickNextDeceleration)
            {
                DeceleratePawns();
            }
        }

        public void TryUpdateUnlockedLevel(int targetLevel, Pawn carrier)
        {
            if (UnlockedLevel == targetLevel && targetLevel >= 0 && targetLevel < 7)
            {
                StudyUnlocks.UnlockStudyNoteManual(targetLevel, carrier);
            }
            UnlockedLevel = Mathf.Max(UnlockedLevel, targetLevel + 1);
            parent.overrideGraphicIndex = targetLevel;
        }

        private void DeceleratePawns()
        {
            for (int i = deceleratedPawns.Count() - 1; i >= 0; i--)
            {
                Pawn DeceleratedPawn = deceleratedPawns[i];
                if (DeceleratedPawn.DestroyedOrNull())
                {
                    deceleratedPawns.RemoveAt(i);
                }
            }
            Pawn[] AvailablePawns = parent.Map.mapPawns.AllHumanlikeSpawned.Where((Pawn p1) => !deceleratedPawns.Any((Pawn p2) => p1 == p2) && !p1.health.hediffSet.HasHediff(Props.DecelerationHediffDef)).ToArray();
            if (AvailablePawns.Count() > 0)
            {
                Pawn DeceleratedPawn = Rand.Element(AvailablePawns.ToArray());
                GiveHediff(DeceleratedPawn, Props.DecelerationHediffDef);
                Notify_Decelerated(DeceleratedPawn);
                if (!Props.soundActivate.NullOrUndefined())
                {
                    Props.soundActivate.PlayOneShot(SoundInfo.InMap(parent));
                }
                if (Props.fleckOnUsed != null)
                {
                    FleckMaker.AttachedOverlay(DeceleratedPawn, Props.fleckOnUsed, Vector3.zero, Props.fleckOnUsedScale / 2);
                }
                deceleratedPawns.Add(DeceleratedPawn);
                TickNextDeceleration = Find.TickManager.TicksGame + Props.DecelerationIntervalRange.RandomInRange;
                parent.overrideGraphicIndex = 7;
            }
            else
            {
                TickNextDeceleration = 2500;
            }
        }

        public Hediff GiveHediff(Pawn pawn, HediffDef hediffDef)
        {
            Hediff AddedHediff = HediffMaker.MakeHediff(hediffDef, pawn);
            pawn.health.AddHediff(AddedHediff);
            return AddedHediff;
        }

        public void Notify_Decelerated(Pawn pawn)
        {
            if (!isDeceleratedOnce)
            {
                StudyUnlocks.UnlockStudyNoteManual(0, pawn);
                isDeceleratedOnce = true;
            }
        }

        public void Notify_Exploded(Pawn pawn)
        {
            if (!isExplodedOnce)
            {
                StudyUnlocks.UnlockStudyNoteManual(7, pawn);
                isExplodedOnce = true;
            }
        }

        public void Notify_Destabilized(Pawn pawn)
        {
            if (!isDestabilizedOnce)
            {
                StudyUnlocks.UnlockStudyNoteManual(8, pawn);
                isDestabilizedOnce = true;
            }
        }

        public void Notify_RemovedEarly(Pawn pawn, int ticksDuration)
        {
            HediffWithComps hediffWithComps = GiveHediff(pawn, Props.ChronoComaHediffDef) as HediffWithComps;
            HediffComp_Disappears hediffComp_Disappears = hediffWithComps.GetComp<HediffComp_Disappears>();
            if (hediffComp_Disappears != null)
            {
                hediffComp_Disappears.SetDuration(ticksDuration);
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
                    defaultLabel = "Dev: Decelerate pawns",
                    action = delegate
                    {
                        DeceleratePawns();
                    }
                };
            }
        }

        protected override void OnInteracted(Pawn caster)
        {
            Hediff_SpeedometerLevel hediff_SpeedometerLevel = GiveHediff(caster, Props.AccelerationHediffDef) as Hediff_SpeedometerLevel;
            hediff_SpeedometerLevel.Speedometer = parent;
            hediff_SpeedometerLevel.SetLevelTo(1);
            TickNextAction = Find.TickManager.TicksGame + Props.tickPerAction;
            TickCanRemove = Find.TickManager.TicksGame + Props.tickPerAction;
            for (int i = deceleratedPawns.Count() - 1; i >= 0; i--)
            {
                deceleratedPawns[i]?.health?.hediffSet?.hediffs?.RemoveAll((Hediff h) => h.def == Props.DecelerationHediffDef);
            }
            deceleratedPawns.Clear();
            parent.DeSpawn();
        }

        public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
        {
            AcceptanceReport result = base.CanInteract(activateBy, checkOptionalItems);
            if (!result.Accepted)
            {
                return result;
            }
            if (activateBy != null)
            {
                if (activateBy.health.hediffSet.HasHediff(Props.AccelerationHediffDef))
                {
                    return "AlreadyHasHediff".Translate(Props.AccelerationHediffDef.label);
                }
            }
            return true;
        }

        public void CooldownsStart()
        {
            StartCooldown();
            TickNextDeceleration = Find.TickManager.TicksGame + Props.DecelerationIntervalRange.RandomInRange;
            parent.overrideGraphicIndex = 0;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref UnlockedLevel, "UnlockedLevel", 1);
            Scribe_Values.Look(ref TickNextAction, "TickNextAction", 0);
            Scribe_Values.Look(ref TickCanRemove, "TickCanRemove", 0);
            Scribe_Values.Look(ref TickNextDeceleration, "TickNextDeceleration", 0);
            Scribe_Values.Look(ref isDeceleratedOnce, "isDeceleratedOnce", false);
            Scribe_Values.Look(ref isExplodedOnce, "isExplodedOnce", false);
            Scribe_Values.Look(ref isDestabilizedOnce, "isDestabilizedOnce", false);
            Scribe_Collections.Look(ref deceleratedPawns, "deceleratedPawns", LookMode.Reference);
        }
    }
}
