using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class Comp_MeatGrinder : ThingComp
    {
        public CompProperties_MeatGrinder Props => (CompProperties_MeatGrinder)props;

        public StorageSettings defaultStorageSettings => Props.defaultStorageSettings;

        public static readonly CachedTexture CreateCorpseStockpileIcon = new CachedTexture("UI/Icons/CorpseStockpileZone");

        public List<IntVec3> ConsumtionCells => consumtionCellsCached ?? (consumtionCellsCached = GenRadial.RadialCellsAround(parent.Position, 1.9f, useCenter: false).ToList());
        private List<IntVec3> consumtionCellsCached;

        protected CompStudiable Studiable => studiableCached ?? (studiableCached = parent.TryGetComp<CompStudiable>());
        private CompStudiable studiableCached;

        public bool isActive = true;

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (isActive)
            {
                List<Thing> consumables = ConsumtionCells.SelectMany((IntVec3 iv3) => parent.Map.thingGrid.ThingsListAtFast(iv3)).ToList();
                bool isConsumed = false;
                if (consumables.Count > 0)
                {
                    foreach (Thing consumable in consumables)
                    {
                        if (Butcher(consumable))
                        {
                            isConsumed = true;
                            break;
                        }
                    }
                }
                if (!isConsumed)
                {
                    //isActive = false;
                }
            }
        }

        public bool Butcher(Thing thing)
        {
            if (thing is Corpse corpse)
            {
                IEnumerable<Thing> products = corpse.ButcherProducts(parent.Map?.mapPawns?.FreeColonists?.RandomElement(), 1);
                foreach (Thing product in products)
                {
                    GenPlace.TryPlaceThing(product, parent.Position, parent.Map, ThingPlaceMode.Near, null, delegate (IntVec3 newLoc)
                    {
                        foreach (Thing item in parent.Map.thingGrid.ThingsListAtFast(newLoc))
                        {
                            if (item == parent)
                            {
                                return false;
                            }
                        }
                        return true;
                    });
                }
                corpse.Destroy();
                return true;
            }
            return false;
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
    }
}
