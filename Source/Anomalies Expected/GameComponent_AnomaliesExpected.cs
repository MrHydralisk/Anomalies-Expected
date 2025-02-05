﻿using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

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

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            Log.Message($"AE: FinalizeInit");
        }

        //public override void GameComponentTick()
        //{
        //    base.GameComponentTick();
        //    Log.Message($"AE: GameComponentTick");
        //}

        public override void LoadedGame()
        {
            base.LoadedGame();
            Log.Message($"AE: LoadedGame 1\n\n{string.Join("\n", EntityEntries.Select(x => $"{x.ThingDef.label}"))}");
            //if (!isCollectedLetters)
            //{

            foreach (EntityCodexEntryDef eced in DefDatabase<EntityCodexEntryDef>.AllDefs)
            {
                AddFromEntityCodex(eced);
            }

            foreach (Thing thing in Find.CurrentMap.listerThings.AllThings)
            {
                CompAEStudyUnlocks compAEStudyUnlocks = thing.TryGetComp<CompAEStudyUnlocks>();
                if (compAEStudyUnlocks != null)
                {
                    SyncEntityEntry(compAEStudyUnlocks);
                }
            }
            Log.Message($"AE: LoadedGame 2\n\n{string.Join("\n", EntityEntries.Select(x => $"{x.ThingDef.label}"))}");
            //    isCollectedLetters = true;
            //}
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
            AEEntityEntry entityEntry = EntityEntries.FirstOrDefault((AEEntityEntry aeee) => (aeee.EntityCodexEntryDef == entityCodexEntryDef) || (aeee.ThingDef == compAEStudyUnlocks.parent.def));
            if (entityEntry == null)
            {
                entityEntry = new AEEntityEntry() {
                    ThingDef = compAEStudyUnlocks.parent.def,
                    EntityCodexEntryDef = entityCodexEntryDef
                };
                EntityEntries.Add(entityEntry);
            }
            if (EntityAddLetters(ref entityEntry, compAEStudyUnlocks.Letters))
            {
                entityEntry.AnomalyLabel = compAEStudyUnlocks.currentAnomalyLabel;
                entityEntry.AnomalyDesc = compAEStudyUnlocks.currentAnomalyDesc;
            }
        }

        public void TryAddEntityEntryFromVanilla(CompStudyUnlocks compStudyUnlocks)
        {
            EntityCodexEntryDef entityCodexEntryDef = DefDatabase<EntityCodexEntryDef>.AllDefs.FirstOrDefault((EntityCodexEntryDef eced) => eced.linkedThings.Contains(compStudyUnlocks.parent.def));
            AEEntityEntry entityEntry = EntityEntries.FirstOrDefault((AEEntityEntry aeee) => (aeee.EntityCodexEntryDef == entityCodexEntryDef) || (aeee.ThingDef == compStudyUnlocks.parent.def));
            if (entityEntry == null)
            {
                entityEntry = new AEEntityEntry()
                {
                    ThingDef = compStudyUnlocks.parent.def,
                    EntityCodexEntryDef = entityCodexEntryDef
                };
                EntityEntries.Add(entityEntry);
            }
            EntityAddLetters(ref entityEntry, compStudyUnlocks.Letters);
        }

        public void UpdateEntityEntryFromVanilla(CompStudyUnlocks compStudyUnlocks, ChoiceLetter keptLetter)
        {
            EntityCodexEntryDef entityCodexEntryDef = DefDatabase<EntityCodexEntryDef>.AllDefs.FirstOrDefault((EntityCodexEntryDef eced) => eced.linkedThings.Contains(compStudyUnlocks.parent.def));
            AEEntityEntry entityEntry = EntityEntries.FirstOrDefault((AEEntityEntry aeee) => (aeee.EntityCodexEntryDef == entityCodexEntryDef) || (aeee.ThingDef == compStudyUnlocks.parent.def));
            if (entityEntry == null)
            {
                entityEntry = new AEEntityEntry()
                {
                    ThingDef = compStudyUnlocks.parent.def,
                    EntityCodexEntryDef = entityCodexEntryDef
                };
                EntityEntries.Add(entityEntry);
            }
            EntityAddLetters(ref entityEntry, compStudyUnlocks.Letters);
        }

        public static bool EntityAddLetters(ref AEEntityEntry entityEntry, IReadOnlyList<ChoiceLetter> letters)
        {
            bool isAdded = false;
            foreach(ChoiceLetter choiceLetter in letters)
            {
                if (!entityEntry.letters.Any((ChoiceLetter cl) => cl.Label == choiceLetter.Label))
                {
                    ChoiceLetter copyLetter = LetterMaker.MakeLetter(choiceLetter.Label, choiceLetter.Text, choiceLetter.def, choiceLetter.lookTargets);
                    copyLetter.arrivalTick = choiceLetter.arrivalTick;
                    entityEntry.letters.Add(copyLetter);
                }
            }
            return isAdded;
        } 

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            Log.Message($"AE: StartedNewGame");
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
            Log.Message($"AE: ExposeData");
        }
    }
}
