using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class Comp_MeatGrinder : CompInteractable
    {
        public new CompProperties_MeatGrinder Props => (CompProperties_MeatGrinder)props;

        public static readonly CachedTexture CreateCorpseStockpileIcon = new CachedTexture("UI/Icons/CorpseStockpileZone");

        public List<IntVec3> ConsumtionCells => consumtionCellsCached ?? (consumtionCellsCached = GenRadial.RadialCellsAround(parent.Position, 1.9f, useCenter: false).ToList());
        private List<IntVec3> consumtionCellsCached;

        private List<Corpse> Consumables => ConsumtionCells.SelectMany((IntVec3 iv3) => parent.Map.thingGrid.ThingsListAtFast(iv3)).Where((Thing t) => t is Corpse && Settings.AllowedToAccept(t)).Select((Thing t) => t as Corpse).ToList();

        public bool isHaveConsumables => (Consumables?.Count ?? 0) > 0;

        public bool isActive;

        public MeatGrinderMood currMood => Props.Moods.FirstOrDefault((MeatGrinderMood mgm) => TickDiff > mgm.tick);

        private int TickFrom;

        private int TickForced;

        private int TickDiff => Mathf.Max(Mathf.Min(Find.TickManager.TicksGame - TickFrom, Props.tickMax), Props.tickMin);

        public StorageSettings Settings => settingsCached ?? (settingsCached = createStorageSettings());
        public StorageSettings settingsCached;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                TickFrom = Find.TickManager.TicksGame;
                TickForced = Find.TickManager.TicksGame + 180000;
            }
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
                            CallMeatGrinder();
                            timeTillNext = timeTillNext * (60f / mood.noise);
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
            sSettings.filter.SetAllow(ThingCategoryDefOfLocal.CorpsesEntity, allow: false);
            sSettings.filter.SetAllow(SpecialThingFilterDefOf.AllowCorpsesUnnatural, allow: false);
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
            IEnumerable<Thing> products = corpse.ButcherProducts(parent.Map?.mapPawns?.FreeColonists?.RandomElement(), currMood?.butcherEfficiency ?? Props.butcherEfficiency);
            List<IntVec3> cleanCells = GenRadial.RadialCellsAround(parent.Position, 1, useCenter: true).ToList();
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
                    Messages.Message("AnomaliesExpected.MeatGrinder.ConsumedFully".Translate(caster.LabelCap, this.parent.LabelCap).RawText, this.parent, MessageTypeDefOf.NegativeEvent);
                    caster.Kill(new DamageInfo(DamageDefOf.Cut, 500, instigator: this.parent));
                    caster.Corpse.Destroy();
                    isConsumed = true;
                }
                else
                {
                    IEnumerable<BodyPartRecord> bodyParts = caster.health.hediffSet.GetNotMissingParts().Where((BodyPartRecord bpr) => mood.bodyPartDefs.Contains(bpr.def));
                    if (bodyParts.Count() > 0)
                    {
                        DamageInfo dm = new DamageInfo(DamageDefOf.Cut, 500, hitPart: bodyParts.RandomElement(), instigator: this.parent);
                        dm.SetAllowDamagePropagation(false);
                        DamageWorker.DamageResult damageResult = caster.TakeDamage(dm);
                        Messages.Message("AnomaliesExpected.MeatGrinder.Consumed".Translate(caster.LabelCap, damageResult.LastHitPart.Label, this.parent.LabelCap).RawText, this.parent, MessageTypeDefOf.NegativeEvent);
                        isConsumed = true;
                    }
                }
                if (isConsumed)
                {
                    AffectMoodTimer(mood.tickConsumed);
                }
            }
        }

        public void CallMeatGrinder()
        {
            Job job = JobMaker.MakeJob(Props.jobDef, parent);
            parent.Map?.mapPawns?.FreeColonists?.RandomElement().jobs.TryTakeOrderedJob(job, JobTag.Misc);

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
            stockpile.settings.filter.SetAllow(ThingCategoryDefOfLocal.CorpsesEntity, allow: false);
            stockpile.settings.filter.SetAllow(SpecialThingFilterDefOf.AllowCorpsesUnnatural, allow: false);
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
                yield return gizmo;
            }
            if (ConsumtionCells.Any((IntVec3 c) => (parent.Map.zoneManager.ZoneAt(c) == null)))
            {
                yield return new Command_Action
                {
                    defaultLabel = "CreateCorpseStockpile".Translate(),
                    defaultDesc = "CreateCorpseStockpileDesc".Translate(),
                    icon = CreateCorpseStockpileIcon.Texture,
                    action = CreateCorpseStockpile
                };
            }
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        CallMeatGrinder();
                    },
                    defaultLabel = "Dev: Call",
                    defaultDesc = "Call Pawn to press button"
                }; 
                yield return new Command_Action
                {
                    action = delegate
                    {
                        TickForced -= 2500;
                    },
                    defaultLabel = "Dev: Decrease Call",
                    defaultDesc = $"Decrease timer till call Pawn to press button: {(TickForced - Find.TickManager.TicksGame).ToStringTicksToDays()}"
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
            MeatGrinderMood mood = currMood;
            inspectStrings.Add("AnomaliesExpected.MeatGrinder.Noise".Translate(mood?.noise ?? 0).RawText);
            return String.Join("\n", inspectStrings);
        }
    }
}
