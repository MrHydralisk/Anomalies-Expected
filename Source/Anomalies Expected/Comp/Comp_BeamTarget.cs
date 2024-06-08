using System;
using System.Collections.Generic;
using RimWorld;
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

        private bool isActive;

        private BeamTargetState beamTargetState = BeamTargetState.Searching;

        protected CompAEStudyUnlocks StudyUnlocks => studyUnlocksCached ?? (studyUnlocksCached = parent.TryGetComp<CompAEStudyUnlocks>());
        private CompAEStudyUnlocks studyUnlocksCached;

        public override bool HideInteraction => (StudyUnlocks?.NextIndex ?? 4) < 4;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                if (isActive)
                {
                    if (beamTargetState == BeamTargetState.Activating)
                    {
                        TickNextState = Find.TickManager.TicksGame + Math.Max(TickNextState - Find.TickManager.TicksGame + Props.ticksWhenCarried, Props.ticksWhenCarried);
                    }
                }
                else
                {
                    TickNextState = Find.TickManager.TicksGame + Props.beamIntervalRange.max;
                    isActive = true;
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
        }

        public void ManualActivation()
        {
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
                SoundDefOfLocal.Psycast_Skip_Exit.PlayOneShot(new TargetInfo(parent.Position, map));
                FleckMaker.Static(parent.Position, map, FleckDefOf.PsycastSkipInnerExit, Props.teleportationFleckRadius);
                SoundDefOf.Psycast_Skip_Entry.PlayOneShot(new TargetInfo(parent.Position, map));
                FleckMaker.Static(result, map, FleckDefOf.PsycastSkipFlashEntry, Props.teleportationFleckRadius);
                LookTargets lookTarget = new LookTargets(parent.Position, map);
                Messages.Message("AnomaliesExpected.BeamTarget.LeftContainment".Translate(parent.LabelCap).RawText, lookTarget, MessageTypeDefOf.NegativeEvent);
                parent.Position = result;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
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
            Scribe_Values.Look(ref isActive, "isActive", true);
            Scribe_Values.Look(ref beamTargetState, "beamTargetState", BeamTargetState.Searching);
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

        public enum BeamTargetState
        {
            Searching,
            Activating
        }
    }
}
