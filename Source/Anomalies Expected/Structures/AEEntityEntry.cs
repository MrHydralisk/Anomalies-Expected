using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AnomaliesExpected
{
    public class AEEntityEntry : IExposable
    {
        public TaggedString AnomalyLabel;
        public string AnomalyDesc;

        public ThingDef ThingDef;
        public AEEntityEntry parentEntityEntry;
        public string parentEntityEntryRef;
        public bool isVanilla => EntityCodexEntryDef.modContentPack.IsCoreMod;
        public EntityCodexEntryDef EntityCodexEntryDef;
        public string categoryLabelCap => EntityCodexEntryDef?.category?.LabelCap ?? "AnomaliesExpected.EntityDataBase.ThreatClass.-1".Translate();
        public string threatClassString => $"AnomaliesExpected.EntityDataBase.ThreatClass".Translate(ThreatClass, $"AnomaliesExpected.EntityDataBase.ThreatClass.{ThreatClass}".Translate());
        public string groupName => EntityCodexEntryDef?.GetModExtension<EntityCodexEntryDefExtension>()?.groupName ?? parentEntityEntry?.groupName ?? "AnomaliesExpected.EntityDataBase.ThreatClass.-1".Translate();
        public string modName => EntityCodexEntryDef?.modContentPack?.Name ?? ThingDef?.modContentPack?.Name ?? "AnomaliesExpected.EntityDataBase.ThreatClass.-1".Translate();

        public int ThreatClass = -1;
        public List<int> CurrPawnAmountStudied = new List<int>();

        public List<ChoiceLetter> letters = new List<ChoiceLetter>();

        public List<ThingDef> SpawnedRelatedAnalyzableThingDef
        {
            get
            {
                if (spawnedRelatedAnalyzableThingDef == null)
                {
                    spawnedRelatedAnalyzableThingDef = new List<ThingDef>();
                }
                for (int i = spawnedRelatedAnalyzableThingDef.Count() - 1; i >= 0; i--)
                {
                    if (spawnedRelatedAnalyzableThingDef[i] == null)
                    {
                        spawnedRelatedAnalyzableThingDef.RemoveAt(i);
                    }
                }
                return spawnedRelatedAnalyzableThingDef;
            }
        }
        private List<ThingDef> spawnedRelatedAnalyzableThingDef = new List<ThingDef>();

        public int IncreasePawnStudy(int index)
        {
            int missingElements = index + 1 - CurrPawnAmountStudied.Count;
            if (missingElements > 0)
            {
                for (int i = 0; i < missingElements; i++)
                {
                    CurrPawnAmountStudied.Add(0);
                }
            }
            CurrPawnAmountStudied[index]++;
            return CurrPawnAmountStudied[index];
        }

        public void AddLetter(ChoiceLetter choiceLetter)
        {
            letters.Add(choiceLetter);
            if (parentEntityEntry != null)
            {
                if (!parentEntityEntry.letters.Any((ChoiceLetter cl) => cl.Label == choiceLetter.Label))
                {
                    ChoiceLetter copyLetter = LetterMaker.MakeLetter(choiceLetter.Label, choiceLetter.Text, choiceLetter.def, choiceLetter.lookTargets);
                    copyLetter.arrivalTick = choiceLetter.arrivalTick;
                    parentEntityEntry.AddLetter(copyLetter);
                }
            }
        }

        public void SpawnThing(ThingDef thingDef, Thing parent = null)
        {
            IntVec3 pos;
            Map map;
            Thing monolith = Find.Anomaly.monolith;
            Log.Message($"{monolith != null} && {monolith.Spawned}");
            if (monolith != null && monolith.Spawned)
            {
                pos = monolith.Position;
                map = monolith.Map;
            }
            else if (parent?.Spawned ?? false)
            {
                pos = parent.Position;
                map = parent.Map;
            }
            else if (parent?.SpawnedOrAnyParentSpawned ?? false)
            {
                pos = parent.PositionHeld;
                map = parent.MapHeld;
            }
            else
            {
                map = Find.AnyPlayerHomeMap;
                CellFinder.TryFindRandomCell(map, (IntVec3 c) => DropCellFinder.IsGoodDropSpot(c, map, false, false), out pos);
            }
            ThingWithComps thing = ThingMaker.MakeThing(thingDef) as ThingWithComps;
            GenPlace.TryPlaceThing(thing, pos, map, ThingPlaceMode.Near);
            CompAnalyzableUnlockResearch compAnalyzable;
            if ((compAnalyzable = thing.GetComp<CompAnalyzableUnlockResearch>()) == null || compAnalyzable.ResearchUnlocked.Any(r => !r.AnalyzedThingsRequirementsMet))
            {
                Find.LetterStack.ReceiveLetter("AnomaliesExpected.Misc.ResearchNote.Letter.Label".Translate(thing.LabelShortCap).RawText, "AnomaliesExpected.Misc.ResearchNote.Letter.Desc".Translate(AnomalyLabel, thing.LabelCap), LetterDefOf.PositiveEvent, thing);
            }
            if (!SpawnedRelatedAnalyzableThingDef.Any((ThingDef td) => td == thingDef))
            {
                SpawnedRelatedAnalyzableThingDef.Add(thing.def);
            }
        }

        public override string ToString()
        {
            return $"{ThingDef?.defName ?? "-"}|{EntityCodexEntryDef?.defName ?? "-"}";
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref AnomalyLabel, "AnomalyLabel");
            Scribe_Values.Look(ref AnomalyDesc, "AnomalyDesc");
            Scribe_Values.Look(ref ThreatClass, "ThreatClass", -1);
            Scribe_Values.Look(ref parentEntityEntryRef, "parentEntityEntryRef");
            Scribe_Defs.Look(ref EntityCodexEntryDef, "EntityCodexEntryDef");
            Scribe_Defs.Look(ref ThingDef, "ThingDef");
            Scribe_Collections.Look(ref letters, "letters", LookMode.Deep);
            Scribe_Collections.Look(ref CurrPawnAmountStudied, "CurrPawnAmountStudied", LookMode.Value);
            Scribe_Collections.Look(ref spawnedRelatedAnalyzableThingDef, "spawnedRelatedAnalyzableThingDef");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (letters == null)
                {
                    letters = new List<ChoiceLetter>();
                }
                if (CurrPawnAmountStudied == null)
                {
                    CurrPawnAmountStudied = new List<int>();
                }
            }
        }
    }
}