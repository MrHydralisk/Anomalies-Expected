using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class Building_AEBloodLake : Building_AEMapPortal
    {
        private static readonly CachedTexture EnterPitGateTex = new CachedTexture("UI/Commands/EnterPitGate");
        protected override Texture2D EnterTex => EnterPitGateTex.Texture;

        private static readonly CachedTexture ViewUndercaveTex = new CachedTexture("UI/Commands/ViewUndercave");

        public AE_BloodLakeExtension ExtBloodLake => extBloodLakeCached ?? (extBloodLakeCached = def.GetModExtension<AE_BloodLakeExtension>());
        private AE_BloodLakeExtension extBloodLakeCached;

        private List<BloodLakeSummonHistory> SummonHistories = new List<BloodLakeSummonHistory>();
        private BloodLakeSummonHistory mainSummonHistory => mainSummonHistoryCached ?? (mainSummonHistoryCached = SummonHistories.FirstOrDefault());
        private BloodLakeSummonHistory mainSummonHistoryCached;

        public Map subMap;
        private BloodLakeMapComponent mapComponent => mapComponentCached ?? (mapComponentCached = subMap?.GetComponent<BloodLakeMapComponent>() ?? null);
        private BloodLakeMapComponent mapComponentCached;
        public Building_AEBloodLakeExit exitBuilding => mapComponent?.Exit;

        private bool beenEntered;

        public override string EnterCommandString => "EnterPitGate".Translate();

        public override bool AutoDraftOnEnter => true;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            List<BloodLakeSummonHistory> summonHistoriesNew = new List<BloodLakeSummonHistory>();
            for (int i = 0; i < ExtBloodLake.summonPatterns.Count; i++)
            {
                BloodLakeSummonPattern summonPattern = ExtBloodLake.summonPatterns[i];
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

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            if (mainSummonHistory != null)
            {
                float progress = (float)(mainSummonHistory.tickNextSummon - Find.TickManager.TicksGame) / mainSummonHistory.currentStage.intervalRange.min;
                DrawColor = Color.Lerp(ExtBloodLake.activeColor, ExtBloodLake.inactiveColor, progress);
            }
        }

        public override void Tick()
        {
            base.Tick();
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
                emergingFleshbeasts = FleshbeastUtility.GetFleshbeastsForPoints(StorytellerUtility.DefaultThreatPointsNow(Map) * summonPatternStage.resourcesAvailableMult, Map);
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

            CellRect cellRect = GenAdj.OccupiedRect(Position, Rot4.North, def.Size);
            List<PawnFlyer> list = new List<PawnFlyer>();
            List<IntVec3> list2 = new List<IntVec3>();
            foreach (Pawn emergingFleshbeast in emergingFleshbeasts)
            {
                IntVec3 randomCell = cellRect.RandomCell;
                GenSpawn.Spawn(emergingFleshbeast, randomCell, Map);
                if (CellFinder.TryFindRandomCellNear(Position, Map, Mathf.CeilToInt(def.size.x / 2), (IntVec3 c) => !c.Fogged(Map) && c.Walkable(Map) && !c.Impassable(Map), out var result))
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
                    spawnRequest.lord = LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_FleshbeastAssault(), Map);
                }
                Map.deferredSpawner.AddRequest(spawnRequest);
                SoundDefOf.Pawn_Fleshbeast_EmergeFromPitGate.PlayOneShot(this);
                emergingFleshbeasts.Clear();
            }
            summonHistory.summonedTimes++;
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            foreach (IntVec3 item in GenAdj.OccupiedRect(Position, Rot4.North, def.Size))
            {
                if (GenGrid.InBounds(item, Map))
                {
                    FilthMaker.TryMakeFilth(item, Map, ExtBloodLake.filthDef, ExtBloodLake.filthThickness);
                }
            }
            foreach (IntVec3 item in GenAdj.OccupiedRect(Position, Rot4.North, def.Size + IntVec2.Two))
            {
                if (GenGrid.InBounds(item, Map))
                {
                    FilthMaker.TryMakeFilth(item, Map, ThingDefOf.Filth_Blood, 1);
                }
            }
            base.Destroy(mode);
        }

        public void GenerateSubMap()
        {
            if (ExtBloodLake.mapGeneratorDef != null)
            {
                PocketMapParent pocketMapParent = WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.PocketMap) as PocketMapParent;
                pocketMapParent.sourceMap = Map;
                subMap = MapGenerator.GenerateMap(Map.Size, pocketMapParent, ExtBloodLake.mapGeneratorDef, isPocketMap: true);
                Find.World.pocketMaps.Add(pocketMapParent);
            }
        }

        public override void OnEntered(Pawn pawn)
        {
            base.OnEntered(pawn);
            if (!beenEntered)
            {
                beenEntered = true;
                Find.LetterStack.ReceiveLetter("LetterLabelGateEntered".Translate(), "LetterGateEntered".Translate(), LetterDefOf.ThreatBig, new TargetInfo(exitBuilding));
            }
            if (Find.CurrentMap == base.Map)
            {
                SoundDefOf.TraversePitGate.PlayOneShot(this);
            }
            else if (Find.CurrentMap == exitBuilding.Map)
            {
                SoundDefOf.TraversePitGate.PlayOneShot(exitBuilding);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                //if (gizmo is Command_Action command_Action)
                //{
                //    command_Action.hotKey = KeyBindingDefOf.Misc6;
                //}
                yield return gizmo;
            }
            if (subMap != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "ViewUndercave".Translate(),
                    defaultDesc = "ViewUndercaveDesc".Translate(),
                    icon = ViewUndercaveTex.Texture,
                    action = delegate
                    {
                        CameraJumper.TryJumpAndSelect(exitBuilding);
                    }
                };
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
                            string summonName = $"{summonPattern.name} {StorytellerUtility.DefaultThreatPointsNow(Map)}";
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
                                summonName += $" {StorytellerUtility.DefaultThreatPointsNow(Map)}";
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
                yield return new Command_Action
                {
                    action = delegate
                    {
                        GenerateSubMap();
                    },
                    defaultLabel = "Dev: Generate Undercave",
                    defaultDesc = "Generate Undercave"
                };
            }
        }

        //protected override void OnInteracted(Pawn caster)
        //{
        //    //ManualActivation();
        //}

        public override Map GetOtherMap()
        {
            if (subMap == null)
            {
                GenerateSubMap();
            }
            return subMap;
        }

        public override IntVec3 GetDestinationLocation()
        {
            return exitBuilding?.Position ?? IntVec3.Invalid;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref SummonHistories, "SummonHistories", LookMode.Deep);
            //Scribe_Values.Look(ref beamNextCount, "beamNextCount", 1);
            //Scribe_Values.Look(ref beamMaxCount, "beamMaxCount", 1);
            //Scribe_Values.Look(ref TickNextState, "TickNextState", Find.TickManager.TicksGame + ExtBloodLake.beamIntervalRange.min);
            //Scribe_Values.Look(ref isActive, "isActive", true);
            //Scribe_Values.Look(ref beamTargetState, "beamTargetState", BeamTargetState.Searching);
            Scribe_References.Look(ref subMap, "subMap");
            Scribe_Values.Look(ref beenEntered, "beenEntered", defaultValue: false);
        }

        public override string GetInspectString()
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
            //    inspectStrings.Add("AnomaliesExpected.BeamTarget.Indicator".Translate(beamNextCount, ExtBloodLake.beamMaxCount).RawText);
            //    if (study > 1)
            //    {
            //        inspectStrings.Add("AnomaliesExpected.BeamTarget.State".Translate(beamTargetState == BeamTargetState.Searching ? "AnomaliesExpected.BeamTarget.StateSearching".Translate() : "AnomaliesExpected.BeamTarget.StateActivating".Translate()).RawText);
            //    }
            //    if (beamTargetState == BeamTargetState.Activating)
            //    {
            //        if (ParentHolder is MinifiedThing)
            //        {
            //            inspectStrings.Add("AnomaliesExpected.BeamTarget.TimeTillBeam".Translate(Math.Max(TickNextState + ExtBloodLake.ticksWhenCarried - Find.TickManager.TicksGame, ExtBloodLake.ticksWhenCarried).ToStringTicksToPeriodVerbose()).RawText);
            //            inspectStrings.Add("AnomaliesExpected.BeamTarget.ButtonHold".Translate().RawText);
            //        }
            //        else
            //        {
            //            inspectStrings.Add("AnomaliesExpected.BeamTarget.TimeTillBeam".Translate(Math.Max(TickNextState - Find.TickManager.TicksGame, 0).ToStringTicksToPeriodVerbose()).RawText);
            //        }
            //    }
            //}
            inspectStrings.Add(base.GetInspectString());
            return String.Join("\n", inspectStrings);
        }
    }
}
