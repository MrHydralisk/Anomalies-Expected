using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class Comp_MeatGrinder : CompInteractable
    {
        public new CompProperties_MeatGrinder Props => (CompProperties_MeatGrinder)props;

        public static readonly CachedTexture CreateCorpseStockpileIcon = new CachedTexture("UI/Icons/CorpseStockpileZone");

        public List<IntVec3> ConsumtionCells => consumtionCellsCached ?? (consumtionCellsCached = GenRadial.RadialCellsAround(parent.Position, 1.9f, useCenter: false).ToList());
        private List<IntVec3> consumtionCellsCached;

        private List<Corpse> Consumables => ConsumtionCells.SelectMany((IntVec3 iv3) => parent.Map.thingGrid.ThingsListAtFast(iv3)).Where((Thing t) => t is Corpse).Select((Thing t) => t as Corpse).ToList();

        public bool isHaveConsumables => (Consumables?.Count ?? 0) > 0;

        protected CompStudiable Studiable => studiableCached ?? (studiableCached = parent.TryGetComp<CompStudiable>());
        private CompStudiable studiableCached;

        public bool isActive;

        public MeatGrinderMood currMood => Props.Moods.FirstOrDefault((MeatGrinderMood mgm) => TickDiff > mgm.tick);

        private int TickFrom;

        private int TickDiff => Mathf.Max(Mathf.Min(Find.TickManager.TicksGame - TickFrom, Props.tickMax), Props.tickMin);

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                TickFrom = Find.TickManager.TicksGame;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (isActive && Find.TickManager.TicksGame % 250 == 0)
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
        }

        public void AffectMoodTimer(int valueAdd)
        {
            TickFrom = Mathf.Min(Mathf.Max(TickFrom + valueAdd, Find.TickManager.TicksGame - Props.tickMax), Find.TickManager.TicksGame - Props.tickMin);
            FilthMaker.TryMakeFilth(parent.Position, parent.Map, ThingDefOf.Filth_Blood, (int)(valueAdd / 1000f));
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
                    caster.Kill(new DamageInfo(DamageDefOf.Cut, 500, instigator: this.parent));
                    caster.Destroy();
                    isConsumed = true;
                }
                else
                {
                    IEnumerable<BodyPartRecord> bodyParts = caster.health.hediffSet.GetNotMissingParts().Where((BodyPartRecord bpr) => mood.bodyPartDefs.Contains(bpr.def));
                    if (bodyParts.Count() > 0)
                    {
                        DamageWorker.DamageResult damageResult = caster.TakeDamage(new DamageInfo(DamageDefOf.Cut, 500, hitPart: bodyParts.RandomElement(), instigator: this.parent));
                        isConsumed = true;
                    }
                }
                if (isConsumed)
                {
                    AffectMoodTimer(mood.tickConsumed);
                }
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
            inspectStrings.Add($"Noise: {mood?.Label ?? "0"} dB");
            return String.Join("\n", inspectStrings);
        }
    }
}
