using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class Comp_BloodLake : CompInteractable
    {
        public new CompProperties_BloodLake Props => (CompProperties_BloodLake)props;

        protected CompAEStudyUnlocks StudyUnlocks => studyUnlocksCached ?? (studyUnlocksCached = parent.TryGetComp<CompAEStudyUnlocks>());
        private CompAEStudyUnlocks studyUnlocksCached;

        public override bool HideInteraction => true;//(StudyUnlocks?.NextIndex ?? 4) < 4;

        private List<BloodLakeSummonHistory> SummonHistories = new List<BloodLakeSummonHistory>();
        private BloodLakeSummonHistory mainSummonHistory => mainSummonHistoryCached ?? (mainSummonHistoryCached = SummonHistories.FirstOrDefault());
        private BloodLakeSummonHistory mainSummonHistoryCached;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            List<BloodLakeSummonHistory> summonHistoriesNew = new List<BloodLakeSummonHistory>();
            for (int i = 0; i < Props.summonPatterns.Count; i++)
            {
                BloodLakeSummonPattern summonPattern = Props.summonPatterns[i];
                BloodLakeSummonHistory summonHistory = SummonHistories.FirstOrDefault((BloodLakeSummonHistory blsh) => blsh.patternName == summonPattern.name);
                if (summonHistory == null)
                {
                    summonHistory = new BloodLakeSummonHistory(summonPattern, Find.TickManager.TicksGame + summonPattern.initialInterval, 0);
                }
                else
                {
                    summonHistory.summonPattern = summonPattern;
                }
                summonHistoriesNew.Add(summonHistory);
            }
            SummonHistories = summonHistoriesNew;
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (mainSummonHistory != null)
            {
                float progress = (float)(mainSummonHistory.tickNextSummon - Find.TickManager.TicksGame) / mainSummonHistory.currentStage.intervalRange.min;
                parent.DrawColor = Color.Lerp(Props.activeColor, Props.inactiveColor, progress);
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            for (int i = 0; i < SummonHistories.Count(); i++)
            {
                BloodLakeSummonHistory summonHistory = SummonHistories[i];
                if (Find.TickManager.TicksGame >= summonHistory.tickNextSummon)
                {
                    Summon(summonHistory);
                    summonHistory.tickNextSummon = Find.TickManager.TicksGame + summonHistory.currentStage.intervalRange.RandomInRange;
                }
            }
        }

        protected void Summon(BloodLakeSummonHistory summonHistory)
        {
            List<Pawn> emergingFleshbeasts = new List<Pawn>();
            BloodLakeSummonPattern summonPattern = summonHistory.summonPattern;
            BloodLakeSummonPatternStage summonPatternStage = summonHistory.currentStage;
            if (summonPattern.isRaid)
            {
                emergingFleshbeasts = FleshbeastUtility.GetFleshbeastsForPoints(StorytellerUtility.DefaultThreatPointsNow(parent.Map) * summonPatternStage.resourcesAvailableMult, parent.Map);
            }
            if (summonPatternStage.pawnKindsWeighted != null)
            {
                int resourcesAvailable = Mathf.CeilToInt(summonPatternStage.resourcesAvailable * summonPatternStage.resourcesAvailableMult);
                while (resourcesAvailable > 0)
                {
                    PawnKindCount pawnKindCount = summonPatternStage.pawnKindsWeighted.Where((PawnKindCount pkdc) => pkdc.count <= resourcesAvailable).RandomElement();
                    if (pawnKindCount != null)
                    {
                        Pawn item = PawnGenerator.GeneratePawn(pawnKindCount.kindDef, FactionUtility.DefaultFactionFrom(pawnKindCount.kindDef.defaultFactionType));
                        emergingFleshbeasts.Add(item);
                        resourcesAvailable -= pawnKindCount.count;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (summonPatternStage.pawnKindsForcedCount != null)
            {
                foreach (PawnKindCount pawnKindCount in summonPatternStage.pawnKindsForcedCount)
                {
                    for (int i = 0; i < pawnKindCount.count; i++)
                    {
                        Pawn item = PawnGenerator.GeneratePawn(pawnKindCount.kindDef, FactionUtility.DefaultFactionFrom(pawnKindCount.kindDef.defaultFactionType));
                        emergingFleshbeasts.Add(item);
                    }
                }
            }

            CellRect cellRect = GenAdj.OccupiedRect(parent.Position, Rot4.North, parent.def.Size);
            List<PawnFlyer> list = new List<PawnFlyer>();
            List<IntVec3> list2 = new List<IntVec3>();
            foreach (Pawn emergingFleshbeast in emergingFleshbeasts)
            {
                IntVec3 randomCell = cellRect.RandomCell;
                GenSpawn.Spawn(emergingFleshbeast, randomCell, parent.Map);
                if (CellFinder.TryFindRandomCellNear(parent.Position, parent.Map, Mathf.CeilToInt(parent.def.size.x / 2), (IntVec3 c) => !c.Fogged(parent.Map) && c.Walkable(parent.Map) && !c.Impassable(parent.Map), out var result))
                {
                    emergingFleshbeast.rotationTracker.FaceCell(result);
                    list.Add(PawnFlyer.MakeFlyer(ThingDefOf.PawnFlyer_Stun, emergingFleshbeast, result, null, null, flyWithCarriedThing: false));
                    list2.Add(randomCell);
                }
            }
            if (list2.Count != 0)
            {
                SpawnRequest spawnRequest = new SpawnRequest(list.Cast<Thing>().ToList(), list2, 1, 1f);
                spawnRequest.initialDelay = 0;
                if (summonPattern.assaultColony)
                {
                    spawnRequest.lord = LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_FleshbeastAssault(), parent.Map);
                }
                parent.Map.deferredSpawner.AddRequest(spawnRequest);
                SoundDefOf.Pawn_Fleshbeast_EmergeFromPitGate.PlayOneShot(parent);
                emergingFleshbeasts.Clear();
            }
            summonHistory.summonedTimes++;
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            foreach (IntVec3 item in GenAdj.OccupiedRect(parent.Position, Rot4.North, parent.def.Size))
            {
                if (GenGrid.InBounds(item, previousMap))
                {
                    FilthMaker.TryMakeFilth(item, previousMap, Props.filthDef, Props.filthThickness);
                }
            }
            foreach (IntVec3 item in GenAdj.OccupiedRect(parent.Position, Rot4.North, parent.def.Size + IntVec2.Two))
            {
                if (GenGrid.InBounds(item, previousMap))
                {
                    FilthMaker.TryMakeFilth(item, previousMap, ThingDefOf.Filth_Blood, 1);
                }
            }
        }

        //public void ManualActivation()
        //{
        //    beamTargetState = BeamTargetState.Activating;
        //    TickNextState = Find.TickManager.TicksGame + Props.ticksWhenCarried;
        //}

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
                //    yield return new Command_Action
                //    {
                //        action = delegate
                //        {
                //            List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
                //            floatMenuOptions.Add(new FloatMenuOption("Increase by 3d", delegate
                //            {
                //                TickNextState += 180000;
                //            }));
                //            floatMenuOptions.Add(new FloatMenuOption("Increase by 1d", delegate
                //            {
                //                TickNextState += 60000;
                //            }));
                //            floatMenuOptions.Add(new FloatMenuOption("Increase by 1h", delegate
                //            {
                //                TickNextState += 2500;
                //            }));
                //            floatMenuOptions.Add(new FloatMenuOption("Decrease by 1h", delegate
                //            {
                //                TickNextState -= 2500;
                //            }));
                //            floatMenuOptions.Add(new FloatMenuOption("Decrease by 1d", delegate
                //            {
                //                TickNextState -= 60000;
                //            }));
                //            floatMenuOptions.Add(new FloatMenuOption("Decrease by 3d", delegate
                //            {
                //                TickNextState -= 180000;
                //            }));
                //            Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                //        },
                //        defaultLabel = "Dev: Change TickNextState",
                //        defaultDesc = $"Change timer for State: {(TickNextState - Find.TickManager.TicksGame).ToStringTicksToDays()}"
                //    };
                yield return new Command_Action
                {
                    action = delegate
                    {
                        List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
                        foreach (BloodLakeSummonHistory summonHistory in SummonHistories)
                        {
                            BloodLakeSummonPattern summonPattern = summonHistory.summonPattern;
                            string summonName = $"{summonPattern.name} {StorytellerUtility.DefaultThreatPointsNow(parent.Map)}";
                            floatMenuOptions.Add(new FloatMenuOption($"Summon {summonName}", delegate
                            {
                                Summon(summonHistory);
                                summonHistory.tickNextSummon = Find.TickManager.TicksGame + summonHistory.currentStage.intervalRange.min;
                            }));
                        }
                        Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                    },
                    defaultLabel = "Dev: Summon now",
                    defaultDesc = "Summon pattern"
                };
                yield return new Command_Action
                {
                    action = delegate
                    {
                        List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
                        foreach (BloodLakeSummonHistory summonHistory in SummonHistories)
                        {
                            BloodLakeSummonPattern summonPattern = summonHistory.summonPattern;
                            string summonName = summonPattern.name;
                            if (summonPattern.isRaid)
                            {
                                summonName += $" {StorytellerUtility.DefaultThreatPointsNow(parent.Map)}";
                            }
                            int tickLeft = summonHistory.tickNextSummon - Find.TickManager.TicksGame;
                            floatMenuOptions.Add(new FloatMenuOption($"{summonHistory.summonedTimes} Summon {summonName}:\n{tickLeft}/{summonHistory.tickNextSummon}\n{tickLeft.ToStringTicksToPeriod()}", delegate
                            {
                                Log.Message($"{summonHistory.summonedTimes} Summoned {summonName}:\n{summonHistory.tickNextSummon - Find.TickManager.TicksGame}/{summonHistory.tickNextSummon}\n{(float)(summonHistory.tickNextSummon - Find.TickManager.TicksGame) / summonHistory.currentStage.intervalRange.min}");
                            }));
                        }
                        Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                    },
                    defaultLabel = "Dev: Log",
                    defaultDesc = "Log data"
                };
                //    yield return new Command_Action
                //    {
                //        action = delegate
                //        {
                //            SkipToRandom();
                //        },
                //        defaultLabel = "Dev: Skip now",
                //        defaultDesc = "Skip to random location"
                //    };
            }
        }

        protected override void OnInteracted(Pawn caster)
        {
            //ManualActivation();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref SummonHistories, "SummonHistories", LookMode.Deep);
            //Scribe_Values.Look(ref beamNextCount, "beamNextCount", 1);
            //Scribe_Values.Look(ref beamMaxCount, "beamMaxCount", 1);
            //Scribe_Values.Look(ref TickNextState, "TickNextState", Find.TickManager.TicksGame + Props.beamIntervalRange.min);
            //Scribe_Values.Look(ref isActive, "isActive", true);
            //Scribe_Values.Look(ref beamTargetState, "beamTargetState", BeamTargetState.Searching);
        }

        public override string CompInspectStringExtra()
        {
            List<string> inspectStrings = new List<string>();
            //int study = StudyUnlocks?.NextIndex ?? 4;
            BloodLakeSummonHistory summonHistory = SummonHistories.FirstOrDefault();
            if (summonHistory != null)
            {
                int ticksLeft = summonHistory.tickNextSummon - Find.TickManager.TicksGame;
                if (ticksLeft < 2500)
                {
                    inspectStrings.Add("Less than hour");
                }
                else if (ticksLeft < 15000)
                {
                    inspectStrings.Add("Low density");
                }
                else if (ticksLeft < 60000)
                {
                    inspectStrings.Add("High density");
                }
                else
                {
                    inspectStrings.Add("Almost solid");
                }
            }
            //if (study > 0)
            //{
            //    inspectStrings.Add("AnomaliesExpected.BeamTarget.Indicator".Translate(beamNextCount, Props.beamMaxCount).RawText);
            //    if (study > 1)
            //    {
            //        inspectStrings.Add("AnomaliesExpected.BeamTarget.State".Translate(beamTargetState == BeamTargetState.Searching ? "AnomaliesExpected.BeamTarget.StateSearching".Translate() : "AnomaliesExpected.BeamTarget.StateActivating".Translate()).RawText);
            //    }
            //    if (beamTargetState == BeamTargetState.Activating)
            //    {
            //        if (parent.ParentHolder is MinifiedThing)
            //        {
            //            inspectStrings.Add("AnomaliesExpected.BeamTarget.TimeTillBeam".Translate(Math.Max(TickNextState + Props.ticksWhenCarried - Find.TickManager.TicksGame, Props.ticksWhenCarried).ToStringTicksToPeriodVerbose()).RawText);
            //            inspectStrings.Add("AnomaliesExpected.BeamTarget.ButtonHold".Translate().RawText);
            //        }
            //        else
            //        {
            //            inspectStrings.Add("AnomaliesExpected.BeamTarget.TimeTillBeam".Translate(Math.Max(TickNextState - Find.TickManager.TicksGame, 0).ToStringTicksToPeriodVerbose()).RawText);
            //        }
            //    }
            //}
            inspectStrings.Add(base.CompInspectStringExtra());
            return String.Join("\n", inspectStrings);
        }
    }
}
