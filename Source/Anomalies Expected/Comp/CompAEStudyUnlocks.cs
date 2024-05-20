using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class CompAEStudyUnlocks : CompStudyUnlocks
    {
        public string currentAnomalyLabel;
        public string currentAnomalyDesc;
        public string currentAnomalyDescPart;

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

        protected override void Notify_StudyLevelChanged(ChoiceLetter keptLetter)
        {
            UpdateFromStudyNote();
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
            UpdateFromStudyNote();
        }
    }
}
