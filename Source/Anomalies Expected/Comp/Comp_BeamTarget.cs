using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AnomaliesExpected
{
    public class Comp_BeamTarget : ThingComp
    {
        public CompProperties_BeamTarget Props => (CompProperties_BeamTarget)props;

        private int beamNextCount = 1;

        private int beamMaxCount = 1;

        private int TickNextState;

        private bool isActive;

        private BeamTargetState beamTargetState = BeamTargetState.Searching;

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
                    TickNextState = Find.TickManager.TicksGame + Props.beamIntervalRange.min;
                    isActive = true;
                }
            }
        }

        public override void CompTick()
        {
            base.CompTick(); 
            if (Find.TickManager.TicksGame >= TickNextState)
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
            if (CellFinder.TryFindRandomCell(map, delegate (IntVec3 newLoc)
            {
                return newLoc.Walkable(map) && !newLoc.Fogged(map) && newLoc.GetFirstPawn(map) == null && newLoc.GetRoom(map) != parent.Position.GetRoom(map);
            }, out var result))
            {
                FleckMaker.Static(parent.Position, map, FleckDefOf.PsycastSkipInnerExit, 0.3f);
                FleckMaker.Static(result, map, FleckDefOf.PsycastSkipFlashEntry, 0.3f);
                Messages.Message("AnomaliesExpected.BeamTarget.LeftContainment".Translate(parent.LabelCap).RawText, this.parent, MessageTypeDefOf.NegativeEvent);
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

        //public override void PostExposeData()
        //{
        //    base.PostExposeData();
        //    Scribe_Collections.Look(ref storedSeverityList, "storedSeverityList", LookMode.Value);
        //}

        public override string CompInspectStringExtra()
        {
            List<string> inspectStrings = new List<string>();
            //int study = StudyUnlocks?.NextIndex ?? 4;
            //if (study > 1)
            //{
            //    MeatGrinderMood mood = currMood;
            //    inspectStrings.Add("AnomaliesExpected.MeatGrinder.Noise".Translate(mood?.noise ?? 0).RawText);
            //    if (study > 2 && (mood?.bodyPartDefs?.Count() ?? 0) > 0)
            //    {
            //        inspectStrings.Add("AnomaliesExpected.MeatGrinder.BodyParts".Translate(String.Join(", ", mood.bodyPartDefs.Select(b => b.LabelCap))).RawText);
            //    }
            //    if (study > 3 && (mood?.isDanger ?? false))
            //    {
            //        inspectStrings.Add("AnomaliesExpected.MeatGrinder.Danger".Translate().RawText);
            //    }
            //}
            inspectStrings.Add($"{beamNextCount}/{Props.beamMaxCount} Lights");
            inspectStrings.Add($"{beamTargetState.ToString()} State");
            if (parent.ParentHolder is MinifiedThing && beamTargetState == BeamTargetState.Activating)
            {
                inspectStrings.Add($"{Math.Max(TickNextState + Props.ticksWhenCarried - Find.TickManager.TicksGame, Props.ticksWhenCarried)} Time");
                inspectStrings.Add($"Button pressed");
            }
            else
            {
                inspectStrings.Add($"{Math.Max(TickNextState - Find.TickManager.TicksGame, 0)} Time");
            }
            //inspectStrings.Add(base.CompInspectStringExtra());
            return String.Join("\n", inspectStrings);
        }

        public enum BeamTargetState
        {
            Searching,
            Activating
        }
    }
}
