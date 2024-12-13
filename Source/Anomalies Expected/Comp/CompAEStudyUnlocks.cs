using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class CompAEStudyUnlocks : CompStudyUnlocks
    {
        public new CompProperties_StudyUnlocks Props => (CompProperties_StudyUnlocks)props;
        public bool isSyncWithParent => (Props is CompProperties_AEStudyUnlocks aeProps) ? aeProps.isSyncWithParent : false;

        public string currentAnomalyLabel;
        public string currentAnomalyDesc;
        public string currentAnomalyDescPart;

        public ThingWithComps parentThing;
        public CompAEStudyUnlocks compAEStudyUnlocksParent => compAEStudyUnlocksParentCached ?? (compAEStudyUnlocksParentCached = parentThing.GetComp<CompAEStudyUnlocks>());
        public CompAEStudyUnlocks compAEStudyUnlocksParentCached;
        public bool isHaveParentAnomaly => parentThing != null;

        public int NextIndex => nextIndex;

        public List<Thing> spawnedRelatedAnalyzableThing = new List<Thing>();

        public override string TransformLabel(string label)
        {
            if (!currentAnomalyLabel.NullOrEmpty())
            {
                return currentAnomalyLabel;
            }
            return label;
        }
        public virtual string TransformDesc(string desc)
        {
            if (!currentAnomalyDesc.NullOrEmpty())
            {
                return currentAnomalyDesc;
            }
            return desc;
        }

        public override string GetDescriptionPart()
        {
            if (!currentAnomalyDescPart.NullOrEmpty())
            {
                return currentAnomalyDescPart;
            }
            return "";
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            UpdateFromStudyNotes();
        }

        public void SetParentThing(ThingWithComps ParentThing)
        {
            parentThing = ParentThing;
            if (isHaveParentAnomaly && isSyncWithParent && compAEStudyUnlocksParent != null)
            {
                for (int i = 0; i < Props.studyNotes.Count(); i++)
                {
                    StudyNote studyNote = Props.studyNotes[i];
                    if (compAEStudyUnlocksParent.Letters.Any((ChoiceLetter cl) => cl.Label == studyNote.label))
                    {
                        nextIndex = i + 1;
                        studyProgress = nextIndex;
                        TaggedString label = studyNote.label.Formatted("AnomaliesExpected.Misc.Redacted".Translate().Named("PAWN"));
                        TaggedString text = studyNote.text.Formatted("AnomaliesExpected.Misc.Redacted".Translate().Named("PAWN"));
                        ChoiceLetter choiceLetter = LetterMaker.MakeLetter(label, text, LetterDefOf.NeutralEvent, parent);
                        choiceLetter.arrivalTick = Find.TickManager.TicksGame;
                        letters.Add(choiceLetter);
                        StudyComp.anomalyKnowledgeGained = Mathf.Max(StudyComp.anomalyKnowledgeGained, studyNote.threshold);
                    }
                }
                UpdateFromStudyNotes();
            }
        }

        public void AddStudyNoteLetter(ChoiceLetter keptLetter)
        {
            letters.Add(keptLetter);
        }

        public void UnlockStudyNoteManual(int index, string studier = "")
        {
            if (Props is CompProperties_AEStudyUnlocks aeProps)
            {
                StudyNote studyNote = aeProps.studyNotesManualUnlockable.ElementAtOrDefault(index);
                if (studyNote != null)
                {
                    if (studier == "")
                    {
                        studier = "AnomaliesExpected.Misc.Redacted".Translate();
                    }
                    TaggedString label = studyNote.label.Formatted(studier.Named("PAWN"));
                    TaggedString text = studyNote.text.Formatted(studier.Named("PAWN"));
                    ChoiceLetter let = LetterMaker.MakeLetter(label, text, LetterDefOf.NeutralEvent, parent);
                    Find.LetterStack.ReceiveLetter(let);
                    ChoiceLetter choiceLetter = LetterMaker.MakeLetter(label, text, LetterDefOf.NeutralEvent, parent);
                    choiceLetter.arrivalTick = Find.TickManager.TicksGame;
                    letters.Add(choiceLetter);
                }
                UpdateFromStudyNote(studyNote);
            }
        }

        protected override void Notify_StudyLevelChanged(ChoiceLetter keptLetter)
        {
            UpdateFromStudyNotes();
            if (isHaveParentAnomaly)
            {
                if (keptLetter.hyperlinkThingDefs == null)
                {
                    keptLetter.hyperlinkThingDefs = new List<ThingDef>();
                }
                if (!compAEStudyUnlocksParent.letters.Any((ChoiceLetter cl) => cl.Label == keptLetter.Label))
                {
                    keptLetter.hyperlinkThingDefs.Add(parent.def);
                    compAEStudyUnlocksParent.AddStudyNoteLetter(keptLetter);
                }
            }
        }

        protected void UpdateFromStudyNotes()
        {
            if (studyProgress > 0 && studyProgress <= Props.studyNotes.Count())
            {
                StudyNote studyNote = Props.studyNotes[studyProgress - 1];
                UpdateFromStudyNote(studyNote);
            }
        }

        protected void UpdateFromStudyNote(StudyNote studyNote)
        {
            if (studyNote != null && studyNote is AEStudyNote aestudyNote)
            {
                currentAnomalyLabel = aestudyNote.AnomalyLabel;
                currentAnomalyDesc = aestudyNote.AnomalyDesc;
                currentAnomalyDescPart = aestudyNote.AnomalyDescPart;
                if (aestudyNote.ThingDefSpawn != null && !spawnedRelatedAnalyzableThing.Any((Thing t) => t.def == aestudyNote.ThingDefSpawn))
                {
                    //CompProperties_CompAnalyzableUnlockResearch comp = aestudyNote.ThingDefSpawn.GetCompProperties<CompProperties_CompAnalyzableUnlockResearch>();
                    //int analysisID = comp?.analysisID ?? (-1);
                    //Find.AnalysisManager.TryGetAnalysisProgress(analysisID, out var details);
                    //if (!(details?.Satisfied ?? false) && parent.Map.listerThings.ThingsOfDef(aestudyNote.ThingDefSpawn).NullOrEmpty())
                    //{
                    ThingWithComps thing = ThingMaker.MakeThing(aestudyNote.ThingDefSpawn) as ThingWithComps;
                    spawnedRelatedAnalyzableThing.Add(thing);
                    Thing monolith = Find.Anomaly.monolith;
                    GenPlace.TryPlaceThing(thing, monolith.Position, monolith.Map, ThingPlaceMode.Near, null);
                    CompAnalyzableUnlockResearch compAnalyzable;
                    if ((compAnalyzable = thing.GetComp<CompAnalyzableUnlockResearch>()) == null || compAnalyzable.ResearchUnlocked.Any(r => !r.AnalyzedThingsRequirementsMet))
                    {
                        Find.LetterStack.ReceiveLetter("AnomaliesExpected.Misc.ResearchNote.Letter.Label".Translate(thing.LabelShortCap).RawText, "AnomaliesExpected.Misc.ResearchNote.Letter.Desc".Translate(parent.LabelCap, thing.LabelCap), LetterDefOf.PositiveEvent, thing);
                    }
                    //}
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref parentThing, "parentThing");
            Scribe_Collections.Look(ref spawnedRelatedAnalyzableThing, "spawnedRelatedAnalyzableThing", LookMode.Reference);
        }
    }
}
