﻿using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class Comp_BeamTarget : CompInteractable
    {
        public new CompProperties_BeamTarget Props => (CompProperties_BeamTarget)props;

        private int beamNextCount = 1;
        private int beamMaxCount = 1;
        private int TickNextState = 20000;
        private int TickUnlockStudy = -1;
        private bool studyMustBeEnabled;

        private IntVec3 sendLocation = IntVec3.Invalid;

        private BeamTargetState beamTargetState = BeamTargetState.Searching;

        protected CompAEStudyUnlocks StudyUnlocks => studyUnlocksCached ?? (studyUnlocksCached = parent.TryGetComp<CompAEStudyUnlocks>());
        private CompAEStudyUnlocks studyUnlocksCached;

        protected CompStudiable Studiable => studiableCached ?? (studiableCached = parent.TryGetComp<CompStudiable>());
        private CompStudiable studiableCached;

        public override bool HideInteraction => (StudyUnlocks?.NextIndex ?? 4) < 4;

        public override void PostPostMake()
        {
            base.PostPostMake();
            TickNextState = Find.TickManager.TicksGame + Props.beamIntervalRange.max;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                if (beamTargetState == BeamTargetState.Activating)
                {
                    TickNextState = Find.TickManager.TicksGame + Math.Max(TickNextState - Find.TickManager.TicksGame + Props.ticksWhenCarried, Props.ticksWhenCarried);
                }
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (Find.TickManager.TicksGame >= TickNextState && parent.Spawned)
            {
                switch (beamTargetState)
                {
                    case BeamTargetState.Searching:
                        {
                            beamTargetState = BeamTargetState.Activating;
                            SkipToRandom();
                            TickNextState = Find.TickManager.TicksGame + Props.ticksPerBeamActivationPreparation;
                            break;
                        }
                    case BeamTargetState.Activating:
                        {
                            beamTargetState = BeamTargetState.Searching;
                            SpawnBeams();
                            TickNextState = Find.TickManager.TicksGame + Props.beamIntervalRange.RandomInRange;
                            break;
                        }
                }
            }
            if (studyMustBeEnabled && !Studiable.studyEnabled && TickUnlockStudy > -1 && Find.TickManager.TicksGame >= TickUnlockStudy)
            {
                Studiable.SetStudyEnabled(studyMustBeEnabled);
                studyMustBeEnabled = false;
                TickUnlockStudy = -1;
            }
        }

        public void ManualActivation()
        {
            if (sendLocation == IntVec3.Invalid || !sendLocation.InBounds(parent.Map) || sendLocation.Fogged(parent.Map))
            {
                sendLocation = parent.Position;
            }
            TargetInfo targetInfoFrom = new TargetInfo(parent.Position, parent.Map);
            SoundDefOfLocal.Psycast_Skip_Exit.PlayOneShot(targetInfoFrom);
            FleckMaker.Static(targetInfoFrom.Cell, targetInfoFrom.Map, FleckDefOf.PsycastSkipInnerExit, Props.teleportationFleckRadius);
            TargetInfo targetInfoTo = new TargetInfo(sendLocation, parent.Map);
            SoundDefOf.Psycast_Skip_Entry.PlayOneShot(targetInfoTo);
            FleckMaker.Static(targetInfoTo.Cell, targetInfoFrom.Map, FleckDefOf.PsycastSkipFlashEntry, Props.teleportationFleckRadius);
            parent.Position = sendLocation;
            beamTargetState = BeamTargetState.Activating;
            TickNextState = Find.TickManager.TicksGame + Props.ticksWhenCarried;
        }

        public void SpawnBeams()
        {
            SpawnBeam(parent.Position);
            for (int i = 1; i < beamNextCount; i++)
            {
                if (CellFinder.TryFindRandomCellNear(parent.Position, parent.Map, Props.beamSubRadius, delegate (IntVec3 newLoc)
                {
                    return true;
                }, out var result))
                {
                    SpawnBeam(result);
                }
                else
                {
                    SpawnBeam(parent.Position);
                }
            }
            beamNextCount = Rand.RangeInclusive(1, beamMaxCount);
            if (beamNextCount == beamMaxCount && beamNextCount < Props.beamMaxCount)
            {
                beamMaxCount = Mathf.Min(beamMaxCount + 1, Props.beamMaxCount);
            }
            if (studyMustBeEnabled)
            {
                TickUnlockStudy = Find.TickManager.TicksGame + Props.beamDuration;
            }
        }

        public void SpawnBeam(IntVec3 position)
        {
            PowerBeam obj = (PowerBeam)GenSpawn.Spawn(ThingDefOf.PowerBeam, position, parent.Map);
            obj.duration = Props.beamDuration;
            obj.instigator = parent;
            obj.weaponDef = parent.def;
            obj.StartStrike();
        }

        public void SkipToRandom()
        {
            Map map = parent.Map;
            IntVec3 result = IntVec3.Invalid;
            if (CellFinder.TryFindRandomCell(map, delegate (IntVec3 newLoc)
            {
                return newLoc.Walkable(map) && !newLoc.Fogged(map) && newLoc.GetFirstPawn(map) == null && newLoc.GetRoom(map) != parent.Position.GetRoom(map);
            }, out result) || CellFinder.TryFindRandomCell(map, delegate (IntVec3 newLoc)
            {
                return newLoc.Walkable(map) && !newLoc.Fogged(map) && newLoc.GetFirstPawn(map) == null;
            }, out result))
            {
                TargetInfo targetInfoFrom = new TargetInfo(parent.Position, parent.Map);
                SoundDefOfLocal.Psycast_Skip_Exit.PlayOneShot(targetInfoFrom);
                FleckMaker.Static(targetInfoFrom.Cell, targetInfoFrom.Map, FleckDefOf.PsycastSkipInnerExit, Props.teleportationFleckRadius);
                TargetInfo targetInfoTo = new TargetInfo(result, parent.Map);
                SoundDefOf.Psycast_Skip_Entry.PlayOneShot(targetInfoTo);
                FleckMaker.Static(targetInfoTo.Cell, targetInfoFrom.Map, FleckDefOf.PsycastSkipFlashEntry, Props.teleportationFleckRadius);
                if (AEMod.Settings.BeamTargetLetter)
                {
                    Find.LetterStack.ReceiveLetter("AnomaliesExpected.BeamTarget.LeftContainment".Translate(parent.LabelCap).RawText, "AnomaliesExpected.BeamTarget.LeftContainmentText".Translate(parent.LabelCap), LetterDefOf.ThreatSmall, targetInfoFrom);
                }
                else
                {
                    Messages.Message("AnomaliesExpected.BeamTarget.LeftContainment".Translate(parent.LabelCap).RawText, targetInfoFrom, MessageTypeDefOf.NegativeEvent);
                }
                parent.Position = result;
            }
            if (Studiable.studyEnabled)
            {
                Studiable.SetStudyEnabled(false);
                studyMustBeEnabled = true;
                TickUnlockStudy = -1;
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
                        List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
                        floatMenuOptions.Add(new FloatMenuOption("Increase by 3d", delegate
                        {
                            TickNextState += 180000;
                        }));
                        floatMenuOptions.Add(new FloatMenuOption("Increase by 1d", delegate
                        {
                            TickNextState += 60000;
                        }));
                        floatMenuOptions.Add(new FloatMenuOption("Increase by 1h", delegate
                        {
                            TickNextState += 2500;
                        }));
                        floatMenuOptions.Add(new FloatMenuOption("Decrease by 1h", delegate
                        {
                            TickNextState -= 2500;
                        }));
                        floatMenuOptions.Add(new FloatMenuOption("Decrease by 1d", delegate
                        {
                            TickNextState -= 60000;
                        }));
                        floatMenuOptions.Add(new FloatMenuOption("Decrease by 3d", delegate
                        {
                            TickNextState -= 180000;
                        }));
                        Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                    },
                    defaultLabel = "Dev: Change TickNextState",
                    defaultDesc = $"Change timer for State: {(TickNextState - Find.TickManager.TicksGame).ToStringTicksToDays()}"
                };
                yield return new Command_Action
                {
                    action = delegate
                    {
                        SpawnBeams();
                    },
                    defaultLabel = "Dev: Beam now",
                    defaultDesc = "Activate Beam"
                };
                yield return new Command_Action
                {
                    action = delegate
                    {
                        SkipToRandom();
                    },
                    defaultLabel = "Dev: Skip now",
                    defaultDesc = "Skip to random location"
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
            targetingParameters.validator = (TargetInfo c) => c.Cell.InBounds(caster.Map) && !c.Cell.Fogged(caster.Map);
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
            ManualActivation();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref beamNextCount, "beamNextCount", 1);
            Scribe_Values.Look(ref beamMaxCount, "beamMaxCount", 1);
            Scribe_Values.Look(ref TickNextState, "TickNextState", Find.TickManager.TicksGame + Props.beamIntervalRange.min);
            Scribe_Values.Look(ref TickUnlockStudy, "TickUnlockStudy");
            Scribe_Values.Look(ref studyMustBeEnabled, "studyMustBeEnabled");
            Scribe_Values.Look(ref beamTargetState, "beamTargetState", BeamTargetState.Searching);
            Scribe_Values.Look(ref sendLocation, "sendLocation", IntVec3.Invalid);
        }

        public override string CompInspectStringExtra()
        {
            List<string> inspectStrings = new List<string>();
            int study = StudyUnlocks?.NextIndex ?? 4;
            if (study > 0)
            {
                inspectStrings.Add("AnomaliesExpected.BeamTarget.Indicator".Translate(beamNextCount, Props.beamMaxCount).RawText);
                if (study > 1)
                {
                    inspectStrings.Add("AnomaliesExpected.BeamTarget.State".Translate(beamTargetState == BeamTargetState.Searching ? "AnomaliesExpected.BeamTarget.StateSearching".Translate() : "AnomaliesExpected.BeamTarget.StateActivating".Translate()).RawText);
                }
                if (beamTargetState == BeamTargetState.Activating)
                {
                    if (parent.ParentHolder is MinifiedThing)
                    {
                        inspectStrings.Add("AnomaliesExpected.BeamTarget.TimeTillBeam".Translate(Math.Max(TickNextState + Props.ticksWhenCarried - Find.TickManager.TicksGame, Props.ticksWhenCarried).ToStringTicksToPeriodVerbose()).RawText);
                        inspectStrings.Add("AnomaliesExpected.BeamTarget.ButtonHold".Translate().RawText);
                    }
                    else
                    {
                        inspectStrings.Add("AnomaliesExpected.BeamTarget.TimeTillBeam".Translate(Math.Max(TickNextState - Find.TickManager.TicksGame, 0).ToStringTicksToPeriodVerbose()).RawText);
                    }
                }
            }
            inspectStrings.Add(base.CompInspectStringExtra());
            return String.Join("\n", inspectStrings);
        }
    }
}
