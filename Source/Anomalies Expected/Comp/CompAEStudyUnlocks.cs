using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class CompAEStudyUnlocks : CompStudyUnlocks
    {
        public new CompProperties_AEStudyUnlocks Props => (CompProperties_AEStudyUnlocks)props;

        public string currentAnomalyLabel;
        public string currentAnomalyDesc;
        public string currentAnomalyDescPart;

        public ThingWithComps parentThing;
        public CompAEStudyUnlocks compAEStudyUnlocksParent => compAEStudyUnlocksParentCached ?? (compAEStudyUnlocksParentCached = parentThing.GetComp<CompAEStudyUnlocks>());
        public CompAEStudyUnlocks compAEStudyUnlocksParentCached;
        public bool isHaveParentAnomaly => parentThing != null;

        public int NextIndex => nextIndex;

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

        public void SetParentThing(ThingWithComps ParentThing)
        {
            parentThing = ParentThing;
            if (isHaveParentAnomaly && Props.isSyncWithParent && compAEStudyUnlocksParent != null)
            {
                for (int i = 0; i < Props.studyNotes.Count(); i++)
                {
                    StudyNote studyNote = Props.studyNotes[i];
                    if (compAEStudyUnlocksParent.Letters.Any((ChoiceLetter cl) => cl.Label == studyNote.label))
                    {
                        nextIndex = i + 1;
                        studyProgress = nextIndex;
                        TaggedString label = studyNote.label.Formatted("[REDACTED]".Named("PAWN"));
                        TaggedString text = studyNote.text.Formatted("[REDACTED]".Named("PAWN"));
                        ChoiceLetter choiceLetter = LetterMaker.MakeLetter(label, text, LetterDefOf.NeutralEvent, parent);
                        letters.Add(choiceLetter);
                        StudyComp.anomalyKnowledgeGained = Mathf.Max(StudyComp.anomalyKnowledgeGained, studyNote.threshold);
                    }
                }
                UpdateFromStudyNote();
            }
        }

        public void AddStudyNoteLetter(ChoiceLetter keptLetter)
        {
            letters.Add(keptLetter);
        }

        protected override void Notify_StudyLevelChanged(ChoiceLetter keptLetter)
        {
            UpdateFromStudyNote();
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

        protected void UpdateFromStudyNote()
        {
            if (studyProgress > 0 && studyProgress <= Props.studyNotes.Count)
            {
                StudyNote studyNote = Props.studyNotes[studyProgress - 1];
                if (studyNote is AEStudyNote aestudyNote)
                {
                    currentAnomalyLabel = aestudyNote.AnomalyLabel;
                    currentAnomalyDesc = aestudyNote.AnomalyDesc;
                    currentAnomalyDescPart = aestudyNote.AnomalyDescPart;
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref parentThing, "parentThing");
            UpdateFromStudyNote();
        }
    }
}
