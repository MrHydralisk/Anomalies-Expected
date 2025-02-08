using RimWorld;
using System.Collections.Generic;
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