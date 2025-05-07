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
        public bool isCreateSeparateEntityEntry => (Props is CompProperties_AEStudyUnlocks aeProps) ? aeProps.isCreateSeparateEntityEntry : false;
        public bool isHavePawnStudyNotes => (Props is CompProperties_AEStudyUnlocks aeProps) ? !aeProps.studyNotesPawnUnlockable.NullOrEmpty() : false;
        public CompStudiable StudyCompPub => StudyComp;

        List<StudyNote> StudyNotesStudy => Props.studyNotes;
        List<StudyNote> StudyNotesAll = new List<StudyNote>();

        public string currentAnomalyLabel;
        public string currentAnomalyDesc;
        public string currentAnomalyDescPart;

        public ThingWithComps parentThing;
        public CompAEStudyUnlocks compAEStudyUnlocksParent => compAEStudyUnlocksParentCached ?? (compAEStudyUnlocksParentCached = parentThing.GetComp<CompAEStudyUnlocks>());
        private CompAEStudyUnlocks compAEStudyUnlocksParentCached;
        public bool isHaveParentAnomaly => parentThing != null;
        public AEEntityEntry entityEntry => entityEntryCached ?? (entityEntryCached = GameComponent_AnomaliesExpected.instance.GetEntityEntryFromThingDef(parent.def));
        private AEEntityEntry entityEntryCached;

        public int NextIndex => nextIndex;
        public float CurrThreshold;
        public int ThreatClass = -1;
        public int lastNotificationTick;

        public int NextPawnSNIndex;

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
            if (entityEntry != null && entityEntry.letters.Count() > 0)
            {
                for (int i = 0; i < StudyNotesAll.Count(); i++)
                {
                    StudyNote studyNote = StudyNotesAll[i];
                    ChoiceLetter choiceLetterSource = entityEntry.letters.FirstOrDefault((ChoiceLetter cl) => cl.Label == studyNote.label);
                    if (choiceLetterSource != null)
                    {
                        TaggedString label = studyNote.label.Replace("{PAWN_nameDef}", "AnomaliesExpected.Misc.Redacted".Translate());
                        TaggedString text = studyNote.text.Replace("{PAWN_nameDef}", "AnomaliesExpected.Misc.Redacted".Translate());
                        ChoiceLetter choiceLetter = LetterMaker.MakeLetter(label, text, choiceLetterSource.def, parent);
                        choiceLetter.arrivalTick = Find.TickManager.TicksGame;
                        letters.Add(choiceLetter);
                        int j = 0;
                        if ((j = StudyNotesStudy.IndexOf(studyNote)) >= 0)
                        {
                            nextIndex = j + 1;
                            studyProgress = nextIndex;
                            StudyComp.anomalyKnowledgeGained = Mathf.Max(StudyComp.anomalyKnowledgeGained, studyNote.threshold);
                        }
                        UpdateFromStudyNote(studyNote);
                    }
                }
                UpdateFromStudyNotes();
            }
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
                    ChoiceLetter choiceLetterSource = compAEStudyUnlocksParent.Letters.FirstOrDefault((ChoiceLetter cl) => cl.Label == studyNote.label);
                    if ((studyNote is AEStudyNote aeStudyNote) && aeStudyNote.isSyncWithDB && choiceLetterSource != null)
                    {
                        TaggedString label = studyNote.label.Replace("{PAWN_nameDef}", "AnomaliesExpected.Misc.Redacted".Translate());
                        TaggedString text = studyNote.text.Replace("{PAWN_nameDef}", "AnomaliesExpected.Misc.Redacted".Translate());
                        ChoiceLetter choiceLetter = LetterMaker.MakeLetter(label, text, choiceLetterSource.def, parent);
                        choiceLetter.arrivalTick = Find.TickManager.TicksGame;
                        letters.Add(choiceLetter);
                        int j = 0;
                        if ((j = StudyNotesStudy.IndexOf(studyNote)) >= 0)
                        {
                            nextIndex = j + 1;
                            studyProgress = nextIndex;
                            StudyComp.anomalyKnowledgeGained = Mathf.Max(StudyComp.anomalyKnowledgeGained, studyNote.threshold);
                        }
                    }
                }
                UpdateFromStudyNotes();
            }
        }

        public void AddStudyNoteLetter(ChoiceLetter keptLetter, bool isSyncDB = false)
        {
            letters.Add(keptLetter);
            if (isSyncDB)
            {
                GameComponent_AnomaliesExpected.instance.SyncEntityEntry(this);
            }
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
                    TaggedString label = studyNote.label.Replace("{PAWN_nameDef}", studier);
                    TaggedString text = studyNote.text.Replace("{PAWN_nameDef}", studier);
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

        public void UnlockStudyNoteManual(int index, Pawn studier)
        {
            UnlockStudyNoteManual(index, studier?.Name?.ToStringShort ?? "AnomaliesExpected.Misc.Redacted".Translate());
        }

        public bool isStudyNoteManualUnlocked(int index)
        {
            if (Props is CompProperties_AEStudyUnlocks aeProps)
            {
                StudyNote studyNote = aeProps.studyNotesManualUnlockable.ElementAtOrDefault(index);
                if (studyNote != null && letters.Any((ChoiceLetter cl) => cl.Label == studyNote.label))
                {
                    return true;
                }
            }
            return false;
        }

        public void RegisterPawnStudyLevel(Pawn studier, int i, AEStudyNote aEStudyNote)
        {
            if (NextPawnSNIndex <= i)
            {
                if (entityEntry != null)
                {
                    int timesStudied = entityEntry.IncreasePawnStudy(i);
                    NextPawnSNIndex = i + 1;
                    if (aEStudyNote.AmountStudiedRequirment == timesStudied)
                    {
                        TaggedString studierName = studier?.Name?.ToStringShort ?? "AnomaliesExpected.Misc.Redacted".Translate();
                        TaggedString label = aEStudyNote.label.Replace("{PAWN_nameDef}", studierName);
                        TaggedString text = aEStudyNote.text.Replace("{PAWN_nameDef}", studierName);
                        ChoiceLetter let = LetterMaker.MakeLetter(label, text, LetterDefOf.NeutralEvent, parent);
                        Find.LetterStack.ReceiveLetter(let);
                        ChoiceLetter choiceLetter = LetterMaker.MakeLetter(label, text, LetterDefOf.NeutralEvent, parent);
                        choiceLetter.arrivalTick = Find.TickManager.TicksGame;
                        letters.Add(choiceLetter);
                        UpdateFromStudyNote(aEStudyNote);
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
                    compAEStudyUnlocksParent.AddStudyNoteLetter(copyLetter, (Props is CompProperties_AEStudyUnlocks aeProps) ? aeProps.isSyncParentDB : false);
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
                    if (aestudyNote.ThingDefSpawn != null && !(entityEntry?.SpawnedRelatedAnalyzableThingDef.Any((ThingDef td) => td == aestudyNote.ThingDefSpawn) ?? true))
                    {
                        entityEntry.SpawnThing(aestudyNote.ThingDefSpawn, parent);
                    }
                }
                CurrThreshold = studyNote.threshold;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref NextPawnSNIndex, "NextPawnSNIndex", 0);
            Scribe_References.Look(ref parentThing, "parentThing");
        }
    }
}
