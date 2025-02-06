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

        List<StudyNote> StudyNotesStudy => Props.studyNotes;
        List<StudyNote> StudyNotesAll = new List<StudyNote>();

        public string currentAnomalyLabel;
        public string currentAnomalyDesc;
        public string currentAnomalyDescPart;

        public ThingWithComps parentThing;
        public CompAEStudyUnlocks compAEStudyUnlocksParent => compAEStudyUnlocksParentCached ?? (compAEStudyUnlocksParentCached = parentThing.GetComp<CompAEStudyUnlocks>());
        public CompAEStudyUnlocks compAEStudyUnlocksParentCached;
        public bool isHaveParentAnomaly => parentThing != null;

        public int NextIndex => nextIndex;
        public float CurrThreshold;
        public int ThreatClass = -1;
        public int lastNotificationTick; 

        public List<Thing> SpawnedRelatedAnalyzableThing
        {
            get
            {
                if (spawnedRelatedAnalyzableThing == null)
                {
                    spawnedRelatedAnalyzableThing = new List<Thing>();
                }
                for (int i = spawnedRelatedAnalyzableThing.Count() - 1; i >= 0; i--)
                {
                    if (spawnedRelatedAnalyzableThing[i] == null)
                    {
                        spawnedRelatedAnalyzableThing.RemoveAt(i);
                    }
                }
                return spawnedRelatedAnalyzableThing;
            }
        }
        private List<Thing> spawnedRelatedAnalyzableThing = new List<Thing>();

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

        public override void PostPostMake()
        {
            base.PostPostMake();
            LoadStudyNotes();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (respawningAfterLoad)
            {
                LoadStudyNotes();
            }
        }

        public void LoadStudyNotes()
        {
            StudyNotesAll = StudyNotesStudy.ToList();
            if (Props is CompProperties_AEStudyUnlocks aeProps)
            {
                StudyNotesAll.AddRange(aeProps.studyNotesManualUnlockable);
                StudyNotesAll.SortBy((StudyNote sn) => sn.threshold);
            }
            UpdateFromStudyNotes();

        }

        public void SetParentThing(ThingWithComps ParentThing)
        {
            parentThing = ParentThing;
            if (isHaveParentAnomaly && isSyncWithParent && compAEStudyUnlocksParent != null)
            {
                for (int i = 0; i < StudyNotesAll.Count(); i++)
                {
                    StudyNote studyNote = StudyNotesAll[i];
                    if (compAEStudyUnlocksParent.Letters.Any((ChoiceLetter cl) => cl.Label == studyNote.label))
                    {
                        TaggedString label = studyNote.label.Formatted("AnomaliesExpected.Misc.Redacted".Translate().Named("PAWN"));
                        TaggedString text = studyNote.text.Formatted("AnomaliesExpected.Misc.Redacted".Translate().Named("PAWN"));
                        ChoiceLetter choiceLetter = LetterMaker.MakeLetter(label, text, LetterDefOf.NeutralEvent, parent);
                        choiceLetter.arrivalTick = Find.TickManager.TicksGame;
                        letters.Add(choiceLetter);
                        int j = 0;
                        if ((j = StudyNotesStudy.IndexOf(studyNote)) >= 0)
                        {
                            nextIndex = i + 1;
                            studyProgress = nextIndex;
                            StudyComp.anomalyKnowledgeGained = Mathf.Max(StudyComp.anomalyKnowledgeGained, studyNote.threshold);
                        }
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
                if (studyNote != null && !letters.Any((ChoiceLetter cl) => cl.Label == studyNote.label))
                {
                    if (studier.NullOrEmpty())
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
                    if (studyNote.threshold > CurrThreshold)
                    {
                        UpdateFromStudyNote(studyNote);
                        GameComponent_AnomaliesExpected.instance.SyncEntityEntry(this);
                    }
                }
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
                    ChoiceLetter copyLetter = LetterMaker.MakeLetter(keptLetter.Label, keptLetter.Text, LetterDefOf.NeutralEvent, keptLetter.lookTargets);
                    copyLetter.arrivalTick = keptLetter.arrivalTick;
                    compAEStudyUnlocksParent.AddStudyNoteLetter(copyLetter);
                }
            }
        }

        protected void UpdateFromStudyNotes()
        {
            bool isUpdated = false;
            for (int i = 0; i < StudyNotesAll.Count(); i++)
            {
                StudyNote studyNote = StudyNotesAll[i];
                if (studyNote.threshold > CurrThreshold && letters.Any((ChoiceLetter cl) => cl.Label == studyNote.label))
                {
                    UpdateFromStudyNote(studyNote);
                    isUpdated = true;
                }
            }
            if (isUpdated)
            {
                GameComponent_AnomaliesExpected.instance.SyncEntityEntry(this);
            }
        }

        protected void UpdateFromStudyNote(StudyNote studyNote)
        {
            if (studyNote != null)
            {
                if (studyNote is AEStudyNote aestudyNote)
                {
                    if (aestudyNote.ThreatClass > ThreatClass)
                    {
                        if (Find.TickManager.TicksGame > lastNotificationTick)
                        {
                            Messages.Message("AnomaliesExpected.EntityDataBase.ThreatClassIncreased".Translate(parent.LabelCap), parent, MessageTypeDefOf.CautionInput);
                            lastNotificationTick = Find.TickManager.TicksGame + 250;
                        }
                        ThreatClass = aestudyNote.ThreatClass;
                    }
                    if (!aestudyNote.AnomalyLabel.NullOrEmpty())
                    {
                        currentAnomalyLabel = aestudyNote.AnomalyLabel;
                    }
                    if (!aestudyNote.AnomalyDesc.NullOrEmpty())
                    {
                        currentAnomalyDesc = aestudyNote.AnomalyDesc;
                    }
                    if (!aestudyNote.AnomalyDescPart.NullOrEmpty())
                    {
                        currentAnomalyDescPart = aestudyNote.AnomalyDescPart;
                    }
                    if (aestudyNote.ThingDefSpawn != null && !SpawnedRelatedAnalyzableThing.Any((Thing t) => t.def == aestudyNote.ThingDefSpawn))
                    {
                        //CompProperties_CompAnalyzableUnlockResearch comp = aestudyNote.ThingDefSpawn.GetCompProperties<CompProperties_CompAnalyzableUnlockResearch>();
                        //int analysisID = comp?.analysisID ?? (-1);
                        //Find.AnalysisManager.TryGetAnalysisProgress(analysisID, out var details);
                        //if (!(details?.Satisfied ?? false) && parent.Map.listerThings.ThingsOfDef(aestudyNote.ThingDefSpawn).NullOrEmpty())
                        //{
                        Thing monolith = Find.Anomaly.monolith;
                        if (monolith == null || monolith.Map == null)
                        {
                            monolith = parent;
                        }
                        ThingWithComps thing = ThingMaker.MakeThing(aestudyNote.ThingDefSpawn) as ThingWithComps;
                        SpawnedRelatedAnalyzableThing.Add(thing);
                        GenPlace.TryPlaceThing(thing, monolith.Position, monolith.Map, ThingPlaceMode.Near);
                        CompAnalyzableUnlockResearch compAnalyzable;
                        if ((compAnalyzable = thing.GetComp<CompAnalyzableUnlockResearch>()) == null || compAnalyzable.ResearchUnlocked.Any(r => !r.AnalyzedThingsRequirementsMet))
                        {
                            Find.LetterStack.ReceiveLetter("AnomaliesExpected.Misc.ResearchNote.Letter.Label".Translate(thing.LabelShortCap).RawText, "AnomaliesExpected.Misc.ResearchNote.Letter.Desc".Translate(parent.LabelCap, thing.LabelCap), LetterDefOf.PositiveEvent, thing);
                        }
                        //}
                    }
                }
                CurrThreshold = studyNote.threshold;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref parentThing, "parentThing");
            Scribe_Collections.Look(ref spawnedRelatedAnalyzableThing, "spawnedRelatedAnalyzableThing", LookMode.Deep);
        }
    }
}
