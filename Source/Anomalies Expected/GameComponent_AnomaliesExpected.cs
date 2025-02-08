using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class GameComponent_AnomaliesExpected : GameComponent
    {
        public static GameComponent_AnomaliesExpected instance;

        public List<AEEntityEntry> EntityEntries = new List<AEEntityEntry>();

        public GameComponent_AnomaliesExpected(Game game)
        {
            instance = this;
        }

        public override void LoadedGame()
        {
            base.LoadedGame();
            SyncWithEntityCodex();
            foreach (Map map in Find.Maps)
            {
                foreach (Thing thing in map.listerThings.AllThings)
                {
                    CompAEStudyUnlocks compAEStudyUnlocks = thing.TryGetComp<CompAEStudyUnlocks>();
                    if (compAEStudyUnlocks != null)
                    {
                        SyncEntityEntry(compAEStudyUnlocks);
                    }
                }
            }
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            SyncWithEntityCodex();
        }

        public void SyncWithEntityCodex()
        {
            foreach (EntityCodexEntryDef eced in DefDatabase<EntityCodexEntryDef>.AllDefs)
            {
                AddFromEntityCodex(eced);
            }
            foreach (AEEntityEntry aeee in EntityEntries)
            {
                if (!aeee.parentEntityEntryRef.NullOrEmpty())
                {
                    aeee.parentEntityEntry = EntityEntries.FirstOrDefault((AEEntityEntry ee) => ee.ToString() == aeee.parentEntityEntryRef);
                }
            }
        }

        public void AddFromEntityCodex(EntityCodexEntryDef entityCodexEntryDef)
        {
            AEEntityEntry entityEntry = EntityEntries.FirstOrDefault((AEEntityEntry aeee) => aeee.EntityCodexEntryDef == entityCodexEntryDef);
            if (entityEntry == null)
            {
                entityEntry = new AEEntityEntry()
                {
                    AnomalyLabel = entityCodexEntryDef.LabelCap,
                    AnomalyDesc = entityCodexEntryDef.Description,
                    EntityCodexEntryDef = entityCodexEntryDef
                };
                EntityEntries.Add(entityEntry);
            }
        }

        public void SyncEntityEntry(CompAEStudyUnlocks compAEStudyUnlocks)
        {
            EntityCodexEntryDef entityCodexEntryDef = DefDatabase<EntityCodexEntryDef>.AllDefs.FirstOrDefault((EntityCodexEntryDef eced) => eced.linkedThings.Contains(compAEStudyUnlocks.parent.def));
            if (entityCodexEntryDef == null && (compAEStudyUnlocks.Props is CompProperties_AEStudyUnlocks AEStudyUnlocks) && !AEStudyUnlocks.isCreateEntityEntryWithoutCodexEntry)
            {
                return;
            }
            AEEntityEntry entityEntry = EntityEntries.FirstOrDefault((AEEntityEntry aeee) => ((aeee.EntityCodexEntryDef == entityCodexEntryDef && !compAEStudyUnlocks.isCreateSeparateEntityEntry) || (aeee.parentEntityEntry != null && aeee.parentEntityEntry.EntityCodexEntryDef == entityCodexEntryDef)) && (aeee.ThingDef == null || aeee.ThingDef == compAEStudyUnlocks.parent.def));
            if (entityEntry == null)
            {
                if (compAEStudyUnlocks.isCreateSeparateEntityEntry)
                {
                    entityEntry = new AEEntityEntry()
                    {
                        ThingDef = compAEStudyUnlocks.parent.def,
                        parentEntityEntry = EntityEntries.FirstOrDefault((AEEntityEntry aeee) => aeee.EntityCodexEntryDef == entityCodexEntryDef && ((aeee.ThingDef == null) || aeee.ThingDef == compAEStudyUnlocks.parent.def)),
                        AnomalyLabel = compAEStudyUnlocks.parent.def.LabelCap,
                        AnomalyDesc = compAEStudyUnlocks.parent.def.DescriptionDetailed
                    };
                    entityEntry.parentEntityEntryRef = entityEntry.parentEntityEntry?.ToString();
                }
                else
                {
                    entityEntry = new AEEntityEntry()
                    {
                        ThingDef = compAEStudyUnlocks.parent.def,
                        EntityCodexEntryDef = entityCodexEntryDef
                    };
                }
                EntityEntries.Add(entityEntry);
            }
            if (entityEntry.ThingDef == null)
            {
                entityEntry.ThingDef = compAEStudyUnlocks.parent.def;
            }
            if (EntityAddLetters(ref entityEntry, compAEStudyUnlocks.Letters))
            {
                if (!compAEStudyUnlocks.parent.LabelCap.NullOrEmpty())
                {
                    entityEntry.AnomalyLabel = compAEStudyUnlocks.parent.LabelCap;
                }
                if (!compAEStudyUnlocks.parent.DescriptionFlavor.NullOrEmpty())
                {
                    entityEntry.AnomalyDesc = compAEStudyUnlocks.parent.DescriptionFlavor;
                }
                entityEntry.ThreatClass = Mathf.Max(compAEStudyUnlocks.ThreatClass, entityEntry.ThreatClass);
            }
        }

        public void SyncEntityEntryFromVanilla(CompStudyUnlocks compStudyUnlocks)
        {
            if (compStudyUnlocks is CompAEStudyUnlocks compAEStudyUnlocks)
            {
                SyncEntityEntry(compAEStudyUnlocks);
                return;
            }
            EntityCodexEntryDef entityCodexEntryDef = DefDatabase<EntityCodexEntryDef>.AllDefs.FirstOrDefault((EntityCodexEntryDef eced) => eced.linkedThings.Contains(compStudyUnlocks.parent.def));
            AEEntityEntry entityEntry = EntityEntries.FirstOrDefault((AEEntityEntry aeee) => aeee.EntityCodexEntryDef == entityCodexEntryDef && (aeee.ThingDef == null || aeee.ThingDef == compStudyUnlocks.parent.def));
            if (entityEntry == null)
            {
                entityEntry = new AEEntityEntry()
                {
                    ThingDef = compStudyUnlocks.parent.def,
                    EntityCodexEntryDef = entityCodexEntryDef
                };
                EntityEntries.Add(entityEntry);
            }
            if (entityEntry.ThingDef == null)
            {
                entityEntry.ThingDef = compStudyUnlocks.parent.def;
            }
            EntityAddLetters(ref entityEntry, compStudyUnlocks.Letters);
        }

        public static bool EntityAddLetters(ref AEEntityEntry entityEntry, IReadOnlyList<ChoiceLetter> letters)
        {
            bool isAdded = false;
            foreach (ChoiceLetter choiceLetter in letters)
            {
                if (!entityEntry.letters.Any((ChoiceLetter cl) => cl.Label == choiceLetter.Label))
                {
                    ChoiceLetter copyLetter = LetterMaker.MakeLetter(choiceLetter.Label, choiceLetter.Text, choiceLetter.def, choiceLetter.lookTargets);
                    copyLetter.arrivalTick = choiceLetter.arrivalTick;
                    entityEntry.AddLetter(copyLetter);
                    isAdded = true;
                }
            }
            return isAdded;
        }

        public AEEntityEntry GetEntityEntryFromThingDef(ThingDef thingDef)
        {
            return EntityEntries.FirstOrDefault((AEEntityEntry aeee) => aeee.ThingDef == thingDef);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref EntityEntries, "EntityEntries", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (EntityEntries == null)
                {
                    EntityEntries = new List<AEEntityEntry>();
                }
            }
        }
    }
}
