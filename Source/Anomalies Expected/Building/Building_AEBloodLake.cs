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

        private bool isBeenEntered;
        private bool isReadyToEnter => (StudyUnlocks?.NextIndex ?? 4) >= 4;

        public override string EnterCommandString => "AnomaliesExpected.BloodLake.Enter".Translate();

        public override bool AutoDraftOnEnter => true;
        public bool isDestroyedMap;

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
                if (Find.TickManager.TicksGame >= summonHistory.tickNextSummon && (!summonHistory.summonPattern.isRaid || !(isDestroyedMap)))
                {
                    if (subMap?.mapPawns?.FreeColonistsAndPrisonersSpawned?.NullOrEmpty() ?? true)
                    {
                        Summon(summonHistory);
                        summonHistory.tickNextSummon = Find.TickManager.TicksGame + summonHistory.currentStage.intervalRange.RandomInRange;
                        continue;
                    }
                    else
                    {
                        summonHistory.tickNextSummon = Find.TickManager.TicksGame + 2500;
                    }
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
                if (summonPattern.assaultColony || GenRadial.RadialCellsAround(Position, ExtBloodLake.aggressionRadius, true).Any((IntVec3 c) => c.GetThingList(Map).Any((Thing t) => t != this && Faction.OfEntities.HostileTo(t.Faction))))
                {
                    spawnRequest.lord = LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_FleshbeastAssault(), Map);
                }
                Map.deferredSpawner.AddRequest(spawnRequest);
                SoundDefOf.Pawn_Fleshbeast_EmergeFromPitGate.PlayOneShot(this);
                emergingFleshbeasts.Clear();
            }
            summonHistory.summonedTimes++;
            if (summonPattern.assaultColony)
            {
                Find.LetterStack.ReceiveLetter("AnomaliesExpected.BloodLake.LetterRaid.Label".Translate(), "AnomaliesExpected.BloodLake.LetterRaid.Text".Translate(), LetterDefOf.ThreatBig, this);
            }
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
            if (subMap != null && !isDestroyedMap)
            {
                mapComponent.DestroySubMap();
            }
            base.Destroy(mode);
        }

        public void GenerateSubMap()
        {
            if (ExtBloodLake.mapGeneratorDef != null)
            {
                PocketMapParent pocketMapParent = WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.PocketMap) as PocketMapParent;
                pocketMapParent.sourceMap = Map;
                IntVec3 SubMapSize = new IntVec3(Mathf.Min(AEMod.Settings.BloodLakeSubMapMaxSize, Map.Size.x), Mathf.Min(AEMod.Settings.BloodLakeSubMapMaxSize, Map.Size.y), Mathf.Min(AEMod.Settings.BloodLakeSubMapMaxSize, Map.Size.z));
                subMap = MapGenerator.GenerateMap(SubMapSize, pocketMapParent, ExtBloodLake.mapGeneratorDef, isPocketMap: true);
                Find.World.pocketMaps.Add(pocketMapParent);
                exitBuilding.StudyUnlocks.SetParentThing(this);
                mapComponent?.Terminal?.StudyUnlocks.SetParentThing(this);
                StudyUnlocks.UnlockStudyNoteManual(0);
            }
        }

        public override bool IsEnterable(out string reason)
        {
            reason = "";
            if ((StudyUnlocks?.NextIndex ?? 3) < 3)
            {
                reason = "AnomaliesExpected.BloodLake.Reason.CantEnterA".Translate();
                return false;
            }
            if (!isReadyToEnter)
            {
                reason = "AnomaliesExpected.BloodLake.Reason.CantEnterB".Translate();
                return false;
            }
            else if (isDestroyedMap)
            {
                reason = "AnomaliesExpected.BloodLake.Reason.CantEnterC".Translate();
                return false;
            }
            return true;
        }

        public override void OnEntered(Pawn pawn)
        {
            base.OnEntered(pawn);
            if (!isBeenEntered)
            {
                isBeenEntered = true;
                Find.LetterStack.ReceiveLetter("AnomaliesExpected.BloodLake.LetterEnter.Label".Translate(), "AnomaliesExpected.BloodLake.LetterEnter.Text".Translate(), LetterDefOf.ThreatBig, new TargetInfo(exitBuilding));
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
                if (gizmo is Command_Action command_Action && command_Action.icon == EnterTex && (!isReadyToEnter || isDestroyedMap))
                {
                    continue;
                }
                yield return gizmo;
            }
            if (subMap != null && !isDestroyedMap)
            {
                yield return new Command_Action
                {
                    defaultLabel = "AnomaliesExpected.BloodLake.ViewSubMap.Label".Translate(),
                    defaultDesc = "AnomaliesExpected.BloodLake.ViewSubMap.Desc".Translate(),
                    icon = ViewUndercaveTex.Texture,
                    action = delegate
                    {
                        CameraJumper.TryJumpAndSelect(exitBuilding);
                    }
                };
            }
            if (DebugSettings.ShowDevGizmos)
            {
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
                                List<FloatMenuOption> floatMenuOptionsS = new List<FloatMenuOption>();
                                floatMenuOptionsS.Add(new FloatMenuOption("Increase by 3d", delegate
                                {
                                    summonHistory.tickNextSummon += 180000;
                                }));
                                floatMenuOptionsS.Add(new FloatMenuOption("Increase by 1d", delegate
                                {
                                    summonHistory.tickNextSummon += 60000;
                                }));
                                floatMenuOptionsS.Add(new FloatMenuOption("Increase by 1h", delegate
                                {
                                    summonHistory.tickNextSummon += 2500;
                                }));
                                floatMenuOptionsS.Add(new FloatMenuOption("Decrease by 1h", delegate
                                {
                                    summonHistory.tickNextSummon -= 2500;
                                }));
                                floatMenuOptionsS.Add(new FloatMenuOption("Decrease by 1d", delegate
                                {
                                    summonHistory.tickNextSummon -= 60000;
                                }));
                                floatMenuOptionsS.Add(new FloatMenuOption("Decrease by 3d", delegate
                                {
                                    summonHistory.tickNextSummon -= 180000;
                                }));
                                Find.WindowStack.Add(new FloatMenu(floatMenuOptionsS));
                            }));
                        }
                        Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                    },
                    defaultLabel = "Dev: Change Wave Timer",
                    defaultDesc = $"Change timer till next wave"
                };
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
                    defaultLabel = "Dev: Generate Sub Map",
                    defaultDesc = "Generate sub map for Blood Lake"
                };
                if (subMap != null && !isDestroyedMap)
                {
                    yield return new Command_Action
                    {
                        action = delegate
                        {
                            List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
                            floatMenuOptions.Add(new FloatMenuOption($"Destroy false", delegate
                            {
                                mapComponent.DestroySubMap();
                            }));
                            floatMenuOptions.Add(new FloatMenuOption($"Destroy true", delegate
                            {
                                mapComponent.DestroySubMap(true);
                            }));
                            Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                        },
                        defaultLabel = "Dev: Destroy sub map",
                        defaultDesc = "Destroy sub map"
                    };
                }
            }
        }

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
            Scribe_References.Look(ref subMap, "subMap");
            Scribe_Values.Look(ref isBeenEntered, "beenEntered", defaultValue: false);
            Scribe_Values.Look(ref isDestroyedMap, "isDestroyedMap");
        }

        public override string GetInspectString()
        {
            List<string> inspectStrings = new List<string>();
            int study = StudyUnlocks?.NextIndex ?? 4;
            if (study >= 1)
            {
                BloodLakeSummonHistory summonHistory = SummonHistories.FirstOrDefault();
                if (isDestroyedMap)
                {
                    summonHistory = SummonHistories.LastOrDefault();
                }
                if (summonHistory != null)
                {
                    int ticksLeft = summonHistory.tickNextSummon - Find.TickManager.TicksGame;
                    inspectStrings.Add("AnomaliesExpected.BloodLake.Density".Translate(summonHistory.summonPattern.DensityCurve.Evaluate(ticksLeft)));
                }
            }
            inspectStrings.Add(base.GetInspectString());
            return String.Join("\n", inspectStrings);
        }
    }
}
