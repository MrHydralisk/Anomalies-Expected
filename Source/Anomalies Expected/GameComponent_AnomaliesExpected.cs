using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AnomaliesExpected
{
    public class GameComponent_AnomaliesExpected : GameComponent
    {
        public static GameComponent_AnomaliesExpected instance;

        protected List<AEEntityEntry> EntityEntries = new List<AEEntityEntry>();
        //protected bool isCollectedLetters;

        public GameComponent_AnomaliesExpected(Game game)
        {
            Log.Message($"AE: GameComponent_AnomaliesExpected");
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            instance = this;
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

        public void SyncEntityEntry(CompAEStudyUnlocks compAEStudyUnlocks)
        {
            AEEntityEntry entityEntry = EntityEntries.FirstOrDefault((AEEntityEntry aeee) => aeee.ThingDef == compAEStudyUnlocks.parent.def);
            if (entityEntry == null)
            {
                entityEntry = new AEEntityEntry() {
                    ThingDef = compAEStudyUnlocks.parent.def,
                    EntityCodexEntryDef = DefDatabase<EntityCodexEntryDef>.AllDefs.FirstOrDefault((EntityCodexEntryDef eced) => eced.linkedThings.Contains(compAEStudyUnlocks.parent.def))
                };
                EntityEntries.Add(entityEntry);
            }
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
