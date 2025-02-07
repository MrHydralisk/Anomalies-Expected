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
        public bool isVanilla => EntityCodexEntryDef.modContentPack.IsCoreMod;
        public EntityCodexEntryDef EntityCodexEntryDef;
        public string categoryLabelCap => EntityCodexEntryDef?.category?.LabelCap ?? "AnomaliesExpected.EntityDataBase.ThreatClass.-1".Translate();
        public string threatClassString => $"AnomaliesExpected.EntityDataBase.ThreatClass".Translate(ThreatClass, $"AnomaliesExpected.EntityDataBase.ThreatClass.{ThreatClass}".Translate());
        public string groupName => EntityCodexEntryDef?.GetModExtension<EntityCodexEntryDefExtension>()?.groupName ?? parentEntityEntry?.groupName ?? "AnomaliesExpected.EntityDataBase.ThreatClass.-1".Translate();
        public string modName => EntityCodexEntryDef?.modContentPack?.Name ?? ThingDef?.modContentPack?.Name ?? "AnomaliesExpected.EntityDataBase.ThreatClass.-1".Translate();

        public int ThreatClass = -1;
        public float CurrThreshold = -1;

        public List<ChoiceLetter> letters = new List<ChoiceLetter>();

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

        public void ExposeData()
        {
            Scribe_Values.Look(ref AnomalyLabel, "AnomalyLabel");
            Scribe_Values.Look(ref AnomalyDesc, "AnomalyDesc");
            Scribe_Values.Look(ref ThreatClass, "ThreatClass", -1);
            Scribe_Values.Look(ref CurrThreshold, "CurrThreshold", -1);
            Scribe_Defs.Look(ref EntityCodexEntryDef, "EntityCodexEntryDef");
            Scribe_Defs.Look(ref ThingDef, "ThingDef");
            Scribe_Collections.Look(ref letters, "letters", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (letters == null)
                {
                    letters = new List<ChoiceLetter>();
                }
            }
        }
    }
}