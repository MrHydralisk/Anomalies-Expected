﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class Comp_MeatGrinder : CompInteractable
    {
        public new CompProperties_MeatGrinder Props => (CompProperties_MeatGrinder)props;

        public static readonly CachedTexture CreateCorpseStockpileIcon = new CachedTexture(ModsConfig.AnomalyActive ? "UI/Icons/CorpseStockpileZone" : "UI/Designators/ZoneCreate_Stockpile");

        public List<IntVec3> ConsumtionCells
        {
            get
            {
                if (consumtionCellsCached == null || parent.Position != lastPosition)
                {
                    consumtionCellsCached = GenRadial.RadialCellsAround(parent.Position, Props.consumptionRadius, useCenter: false).ToList();
                    lastPosition = parent.Position;
                }
                return consumtionCellsCached;
            }
        }
        private List<IntVec3> consumtionCellsCached;
        private IntVec3 lastPosition;

        private List<Corpse> Consumables => ConsumtionCells.SelectMany((IntVec3 iv3) => parent.Map.thingGrid.ThingsListAtFast(iv3)).Where((Thing t) => t is Corpse && Settings.AllowedToAccept(t)).Select((Thing t) => t as Corpse).ToList();

        public bool isHaveConsumables => (Consumables?.Count ?? 0) > 0;

        public bool isActive;

        public MeatGrinderMood currMood => Props.Moods.FirstOrDefault((MeatGrinderMood mgm) => TickDiff > mgm.tick);

        private int TickFrom;

        private int TickForced;

        private int TickDiff => Mathf.Max(Mathf.Min(Find.TickManager.TicksGame - TickFrom, Props.tickMax), Props.tickMin);

        public StorageSettings Settings => settingsCached ?? (settingsCached = createStorageSettings());
        public StorageSettings settingsCached;

        protected CompAEStudyUnlocks StudyUnlocks => studyUnlocksCached ?? (studyUnlocksCached = parent.TryGetComp<CompAEStudyUnlocks>());
        private CompAEStudyUnlocks studyUnlocksCached;

        public override void PostPostMake()
        {
            base.PostPostMake();
            TickFrom = Find.TickManager.TicksGame;
            TickForced = Find.TickManager.TicksGame + 180000;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (Find.TickManager.TicksGame % 250 == 0)
            {
                if (isActive)
                {
                    bool isConsumed = false;
                    if (isHaveConsumables)
                    {
                        foreach (Corpse consumable in Consumables)
                        {
                            Butcher(consumable);
                            isConsumed = true;
                            break;
                        }
                    }
                    if (!isConsumed)
                    {
                        isActive = false;
                        StartCooldown();
                    }
                }
                else
                {
                    if (Find.TickManager.TicksGame >= TickForced)
                    {
                        MeatGrinderMood mood = currMood;
                        float timeTillNext = Props.tickPerForced;
                        if (mood == null)
                        {
                            timeTillNext = timeTillNext * 2;
                        }
                        else
                        {
                            CallMeatGrinder(ref timeTillNext);
                        }
                        TickForced = Find.TickManager.TicksGame + (int)(timeTillNext * (0.5f + Rand.Value));
                    }
                }
            }
        }

        public StorageSettings createStorageSettings()
        {
            StorageSettings sSettings = new StorageSettings();
            sSettings.SetFromPreset(StorageSettingsPreset.CorpseStockpile);
            sSettings.filter.SetAllow(ThingCategoryDefOf.CorpsesMechanoid, allow: false);
            if (ModsConfig.AnomalyActive)
            {
                sSettings.filter.SetAllow(ThingCategoryDefOfLocal.CorpsesEntity, allow: false);
                sSettings.filter.SetAllow(SpecialThingFilterDefOf.AllowCorpsesUnnatural, allow: false);
            }
            sSettings.filter.SetAllow(SpecialThingFilterDefOfLocal.AllowRotten, allow: false);
            return sSettings;
        }

        public void AffectMoodTimer(int valueAdd)
        {
            TickFrom = Mathf.Min(Mathf.Max(TickFrom + valueAdd, Find.TickManager.TicksGame - Props.tickMax), Find.TickManager.TicksGame - Props.tickMin);
            FilthMaker.TryMakeFilth(parent.Position, parent.Map, ThingDefOf.Filth_Blood, (int)(valueAdd / 1000f));
            if (!Props.soundConsume.NullOrUndefined())
            {
                Props.soundConsume.PlayOneShot(SoundInfo.InMap(parent));
            }
        }

        protected override void StartCooldown()
        {
            if (!isActive)
            {
                cooldownTicks = Props.cooldownTicks;
            }
        }

        public void Butcher(Corpse corpse)
        {
            IEnumerable<Thing> products = corpse.InnerPawn.ButcherProducts(null, currMood?.butcherEfficiency ?? Props.butcherEfficiency);
            if (corpse.InnerPawn.RaceProps.BloodDef != null)
            {
                FilthMaker.TryMakeFilth(parent.Position, parent.Map, corpse.InnerPawn.RaceProps.BloodDef, corpse.InnerPawn.LabelIndefinite());
            }
            if (corpse.InnerPawn.RaceProps.Humanlike)
            {
                Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.ButcheredHuman, new SignalArgs(parent.Named(HistoryEventArgsNames.Doer), corpse.InnerPawn.Named(HistoryEventArgsNames.Victim))));
            }
            List<IntVec3> cleanCells = GenRadial.RadialCellsAround(parent.Position, 2, useCenter: true).ToList();
            foreach (Thing product in products)
            {
                GenPlace.TryPlaceThing(product, parent.Position, parent.Map, ThingPlaceMode.Near, null, delegate (IntVec3 newLoc)
                {
                    if (cleanCells.Contains(newLoc))
                    {
                        return false;
                    }
                    return true;
                });
            }
            Pawn innerPawn = corpse.InnerPawn;
            AffectMoodTimer((int)(Props.tickPerBody * (innerPawn?.BodySize ?? 1f) * ((innerPawn?.RaceProps?.Humanlike ?? false) ? 1f : Props.nonHumanMult)));
            corpse.Destroy();
        }

        public void CheckMood(Pawn caster)
        {
            MeatGrinderMood mood = currMood;
            if (mood != null)
            {
                bool isConsumed = false;
                if (mood.isDanger)
                {
                    Messages.Message("AnomaliesExpected.MeatGrinder.ConsumedFully".Translate(caster.LabelShortCap, this.parent.LabelCap).RawText, this.parent, MessageTypeDefOf.NegativeEvent);
                    caster.Kill(new DamageInfo(DamageDefOf.Cut, 500, instigator: this.parent));
                    caster.Corpse.Destroy();
                    isConsumed = true;
                }
                else
                {
                    IEnumerable<BodyPartRecord> bodyParts = caster.health.hediffSet.GetNotMissingParts().Where((BodyPartRecord bpr) => mood.bodyPartDefs.Contains(bpr.def));
                    if (bodyParts.Count() > 0)
                    {
                        DamageInfo dm = new DamageInfo(DamageDefOf.Cut, 400, hitPart: bodyParts.RandomElement(), instigator: this.parent);
                        dm.SetAllowDamagePropagation(false);
                        DamageWorker.DamageResult damageResult = caster.TakeDamage(dm);
                        if (caster.health.hediffSet.HasMissingPartFor(damageResult.LastHitPart))
                        {
                            Messages.Message("AnomaliesExpected.MeatGrinder.Consumed".Translate(caster.LabelShortCap, damageResult.LastHitPart.Label, this.parent.LabelCap).RawText, this.parent, MessageTypeDefOf.NegativeEvent);
                            isConsumed = true;
                        }
                        else
                        {
                            Messages.Message("AnomaliesExpected.MeatGrinder.EvadedConsumed".Translate(caster.LabelShortCap, damageResult.LastHitPart.Label, this.parent.LabelCap).RawText, this.parent, MessageTypeDefOf.PositiveEvent);
                        }
                    }
                }
                if (isConsumed)
                {
                    AffectMoodTimer(mood.tickConsumed);
                }
            }
        }

        public void CallMeatGrinder(ref float time)
        {
            time = time * (60f / (currMood?.noise ?? 30f));
            Job job = JobMaker.MakeJob(Props.jobDef, parent);
            Pawn called = (Pawn)GenClosest.ClosestThing_Global_Reachable(parent.Position, parent.Map, parent.Map.mapPawns.AllHumanlikeSpawned, PathEndMode.OnCell, TraverseParms.For(TraverseMode.PassDoors), 9999f, (Thing t) => t is Pawn p && !p.DeadOrDowned && GenClosest.ClosestThing_Global_Reachable(p.Position, p.Map, [parent], PathEndMode.OnCell, TraverseParms.For(p), 9999f) != null);
            if (called != null)
            {
                called.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }
            else
            {
                foreach (Pawn pawn in parent.Map.mapPawns.FreeColonistsAndPrisonersSpawned)
                {
                    Thought_AEMemory thought = (Thought_AEMemory)ThoughtMaker.MakeThought(Props.thoughtDefNoise);
                    thought.durationTicksOverride = (int)time;
                    thought.SetForcedStage(Props.Moods.FindIndex((MeatGrinderMood mgm) => TickDiff > mgm.tick) + 1);
                    thought.DescAddition = "AnomaliesExpected.MeatGrinder.NoiseSource".Translate(parent.LabelCap);
                    pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(thought);
                }
            }

            if (!Props.soundActivate.NullOrUndefined())
            {
                Props.soundActivate.PlayOneShot(SoundInfo.InMap(parent));
            }
            if (Props.fleckOnUsed != null)
            {
                FleckMaker.AttachedOverlay(this.parent, Props.fleckOnUsed, Vector3.zero, Props.fleckOnUsedScale * 1.5f);
            }
        }

        private void CreateCorpseStockpile()
        {
            Zone_Stockpile stockpile = new Zone_Stockpile(StorageSettingsPreset.CorpseStockpile, parent.Map.zoneManager);
            stockpile.settings.filter.SetAllow(ThingCategoryDefOf.CorpsesMechanoid, allow: false);
            if (ModsConfig.AnomalyActive)
            {
                stockpile.settings.filter.SetAllow(ThingCategoryDefOfLocal.CorpsesEntity, allow: false);
                stockpile.settings.filter.SetAllow(SpecialThingFilterDefOf.AllowCorpsesUnnatural, allow: false);
            }
            stockpile.settings.filter.SetAllow(SpecialThingFilterDefOfLocal.AllowRotten, allow: false);
            parent.Map.zoneManager.RegisterZone(stockpile);
            foreach (IntVec3 c in ConsumtionCells)
            {
                if (parent.Map.zoneManager.ZoneAt(c) == null && (bool)Designator_ZoneAdd.IsZoneableCell(c, parent.Map))
                {
                    stockpile.AddCell(c);
                }
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
            if (ConsumtionCells.Any((IntVec3 c) => (parent.Map.zoneManager.ZoneAt(c) == null)))
            {
                yield return new Command_Action
                {
                    defaultLabel = "CreateCorpseStockpile".Translate(),
                    defaultDesc = "CreateCorpseStockpileDesc".Translate(),
                    icon = CreateCorpseStockpileIcon.Texture,
                    hotKey = KeyBindingDefOf.Misc11,
                    action = CreateCorpseStockpile
                };
            }
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        float timeTillNext = Props.tickPerForced;
                        CallMeatGrinder(ref timeTillNext);
                    },
                    defaultLabel = "Dev: Call",
                    defaultDesc = "Call Pawn to press button"
                };
                yield return new Command_Action
                {
                    action = delegate
                    {
                        List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
                        floatMenuOptions.Add(new FloatMenuOption("Increase by 3d", delegate
                        {
                            TickFrom -= 180000;
                        }));
                        floatMenuOptions.Add(new FloatMenuOption("Increase by 1d", delegate
                        {
                            TickFrom -= 60000;
                        }));
                        floatMenuOptions.Add(new FloatMenuOption("Increase by 1h", delegate
                        {
                            TickFrom -= 2500;
                        }));
                        floatMenuOptions.Add(new FloatMenuOption("Decrease by 1h", delegate
                        {
                            TickFrom += 2500;
                        }));
                        floatMenuOptions.Add(new FloatMenuOption("Decrease by 1d", delegate
                        {
                            TickFrom += 60000;
                        }));
                        floatMenuOptions.Add(new FloatMenuOption("Decrease by 3d", delegate
                        {
                            TickFrom += 180000;
                        }));
                        Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                    },
                    defaultLabel = "Dev: Change Mood",
                    defaultDesc = $"Change timer for Mood: {TickDiff.ToStringTicksToDays()}"
                };
                yield return new Command_Action
                {
                    action = delegate
                    {
                        List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
                        floatMenuOptions.Add(new FloatMenuOption("Increase by 3d", delegate
                        {
                            TickForced += 180000;
                        }));
                        floatMenuOptions.Add(new FloatMenuOption("Increase by 1d", delegate
                        {
                            TickForced += 60000;
                        }));
                        floatMenuOptions.Add(new FloatMenuOption("Increase by 1h", delegate
                        {
                            TickForced += 2500;
                        }));
                        floatMenuOptions.Add(new FloatMenuOption("Decrease by 1h", delegate
                        {
                            TickForced -= 2500;
                        }));
                        floatMenuOptions.Add(new FloatMenuOption("Decrease by 1d", delegate
                        {
                            TickForced -= 60000;
                        }));
                        floatMenuOptions.Add(new FloatMenuOption("Decrease by 3d", delegate
                        {
                            TickForced -= 180000;
                        }));
                        Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                    },
                    defaultLabel = "Dev: Change Call",
                    defaultDesc = $"Change timer till call Pawn to press button: {(TickForced - Find.TickManager.TicksGame).ToStringTicksToDays()}"
                };
                yield return new Command_Action
                {
                    action = delegate
                    {
                        Log.Message($"MeatGrinder at {Find.TickManager.TicksGame}:\nisActive {isActive}\nTickFrom {TickFrom}\nTickForced {TickForced}");
                    },
                    defaultLabel = "Dev: Log",
                    defaultDesc = $"Log values: {Find.TickManager.TicksGame}:\nisActive {isActive}\nTickFrom {TickFrom}/{(Find.TickManager.TicksGame - TickFrom).ToStringTicksToDays()}\nTickForced {TickForced}/{(TickForced - Find.TickManager.TicksGame).ToStringTicksToDays()}"
                };
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isActive, "isActive", false);
            Scribe_Values.Look(ref TickFrom, "TickFrom", Find.TickManager.TicksGame);
            Scribe_Values.Look(ref TickForced, "TickForced", Find.TickManager.TicksGame);
        }

        protected override void OnInteracted(Pawn caster)
        {
            isActive = true;
            CheckMood(caster);
        }

        public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
        {
            AcceptanceReport result = base.CanInteract(activateBy, checkOptionalItems);
            if (!result.Accepted)
            {
                return result;
            }
            if (isActive)
            {
                return "AlreadyActive".Translate();
            }
            return true;
        }

        public override string CompInspectStringExtra()
        {
            List<string> inspectStrings = new List<string>();
            int study = StudyUnlocks?.NextIndex ?? 4;
            if (study > 1)
            {
                MeatGrinderMood mood = currMood;
                inspectStrings.Add("AnomaliesExpected.MeatGrinder.Noise".Translate(mood?.noise ?? 0).RawText);
                if (study > 2 && (mood?.bodyPartDefs?.Count() ?? 0) > 0)
                {
                    inspectStrings.Add("AnomaliesExpected.MeatGrinder.BodyParts".Translate(String.Join(", ", mood.bodyPartDefs.Select(b => b.LabelCap))).RawText);
                }
                if (study > 3 && (mood?.isDanger ?? false))
                {
                    inspectStrings.Add("AnomaliesExpected.MeatGrinder.Danger".Translate().RawText);
                }
            }
            inspectStrings.Add(base.CompInspectStringExtra());
            return String.Join("\n", inspectStrings);
        }
    }
}
