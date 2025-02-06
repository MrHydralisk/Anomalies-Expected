using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace AnomaliesExpected
{
    public class GameComponent_AnomaliesExpected : GameComponent
    {
        public static GameComponent_AnomaliesExpected instance;

        public List<AEEntityEntry> EntityEntries = new List<AEEntityEntry>();
        //protected bool isCollectedLetters;

        public GameComponent_AnomaliesExpected(Game game)
        {
            instance = this;
            Log.Message($"AE: GameComponent_AnomaliesExpected");
        }

        //public override void FinalizeInit()
        //{
        //    base.FinalizeInit();
        //    Log.Message($"AE: FinalizeInit");
        //}

        //public override void GameComponentTick()
        //{
        //    base.GameComponentTick();
        //    Log.Message($"AE: GameComponentTick");
        //}

        public override void LoadedGame()
        {
            base.LoadedGame();
            //if (!isCollectedLetters)
            //{

            SyncWithEntityCodex();

            foreach(Map map in Find.Maps)
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
            //    isCollectedLetters = true;
            //}
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            SyncWithEntityCodex();
            //Log.Message($"AE: StartedNewGame");
        }

        public void SyncWithEntityCodex()
        {
            foreach (EntityCodexEntryDef eced in DefDatabase<EntityCodexEntryDef>.AllDefs)
            {
                AddFromEntityCodex(eced);
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
            //Log.Message($"compAEStudyUnlocks {compAEStudyUnlocks.parent.LabelCap}: {entityCodexEntryDef == null} | {compAEStudyUnlocks.Props is CompProperties_AEStudyUnlocks} | {compAEStudyUnlocks.Props is CompProperties_AEStudyUnlocks AEStudyUnlockss && !AEStudyUnlockss.isCreateEntityEntryWithoutCodexEntry}");
            if (entityCodexEntryDef == null && (compAEStudyUnlocks.Props is CompProperties_AEStudyUnlocks AEStudyUnlocks) && !AEStudyUnlocks.isCreateEntityEntryWithoutCodexEntry)
            {
                return;
            }
            AEEntityEntry entityEntry = EntityEntries.FirstOrDefault((AEEntityEntry aeee) => aeee.EntityCodexEntryDef == entityCodexEntryDef && (aeee.ThingDef == null || aeee.ThingDef == compAEStudyUnlocks.parent.def));
            if (entityEntry == null)
            {
                entityEntry = new AEEntityEntry() {
                    ThingDef = compAEStudyUnlocks.parent.def,
                    EntityCodexEntryDef = entityCodexEntryDef
                };
                EntityEntries.Add(entityEntry);
            }
            if (entityEntry.ThingDef == null)
            {
                entityEntry.ThingDef = compAEStudyUnlocks.parent.def;
            }
            //Log.Message($"SyncEntityEntry A {entityEntry.AnomalyLabel} | {compAEStudyUnlocks.currentAnomalyLabel} | {compAEStudyUnlocks.parent.LabelCap}");
            if (EntityAddLetters(ref entityEntry, compAEStudyUnlocks.Letters))
            {
                //Log.Message($"SyncEntityEntry B {compAEStudyUnlocks.parent.LabelCap}={entityEntry.AnomalyLabel}\n{compAEStudyUnlocks.parent.DescriptionFlavor}={entityEntry.AnomalyDesc}");
                if (!compAEStudyUnlocks.parent.LabelCap.NullOrEmpty())
                {
                    entityEntry.AnomalyLabel = compAEStudyUnlocks.parent.LabelCap;
                }
                if (!compAEStudyUnlocks.parent.DescriptionFlavor.NullOrEmpty())
                {
                    entityEntry.AnomalyDesc = compAEStudyUnlocks.parent.DescriptionFlavor;
                }
                entityEntry.ThreatClass = Mathf.Max(compAEStudyUnlocks.ThreatClass, entityEntry.ThreatClass);
                //Log.Message($"SyncEntityEntry C {compAEStudyUnlocks.parent.LabelCap}={entityEntry.AnomalyLabel}\n{compAEStudyUnlocks.parent.DescriptionFlavor}={entityEntry.AnomalyDesc}");
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
            //Log.Message($"TryAddEntityEntryFromVanilla {compStudyUnlocks.parent.LabelCap}");
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
            //Log.Message($"EntityAddLetters {letters.Count()}");
            foreach(ChoiceLetter choiceLetter in letters)
            {
                if (!entityEntry.letters.Any((ChoiceLetter cl) => cl.Label == choiceLetter.Label))
                {
                    ChoiceLetter copyLetter = LetterMaker.MakeLetter(choiceLetter.Label, choiceLetter.Text, choiceLetter.def, choiceLetter.lookTargets);
                    copyLetter.arrivalTick = choiceLetter.arrivalTick;
                    entityEntry.letters.Add(copyLetter);
                    isAdded = true;
                }
            }
            return isAdded;
        } 

        //public override void GameComponentOnGUI()
        //{
        //    base.GameComponentOnGUI();
        //    Log.Message($"AE: GameComponentOnGUI");
        //}

        //public override void GameComponentUpdate()
        //{
        //    base.GameComponentUpdate();
        //    Log.Message($"AE: GameComponentUpdate");
        //}

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
            //Log.Message($"AE: ExposeData");
        }
    }
}
