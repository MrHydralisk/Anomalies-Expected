using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AnomaliesExpected
{
    public class CompStudiableAE : CompStudiable
    {
        public override void Study(Pawn studier, float studyAmount, float anomalyKnowledgeAmount = 0f)
        {
            if (studier.Dead)
            {
                studier = studier.Map?.mapPawns?.FreeColonists?.RandomElement();
            }
            bool completed = Completed;
            if (studyAmount > 0)
            {
                studyAmount *= Find.Storyteller.difficulty.researchSpeedFactor;
                studyAmount *= studier.GetStatValue(StatDefOf.ResearchSpeed);
                Find.StudyManager.Study(parent, studier, studyAmount);
                studier?.skills.Learn(SkillDefOf.Intellectual, 0.1f);
            }
            anomalyKnowledgeGained += anomalyKnowledgeAmount;
            if (ModsConfig.AnomalyActive && anomalyKnowledgeAmount > 0f)
            {
                Find.StudyManager.StudyAnomaly(parent, studier, anomalyKnowledgeAmount, KnowledgeCategory);
            }
            if (!completed && Completed)
            {
                QuestUtility.SendQuestTargetSignals(parent.questTags, "Researched", parent.Named("SUBJECT"), studier.Named("STUDIER"));
                if (!Props.completedMessage.NullOrEmpty())
                {
                    Messages.Message(Props.completedMessage, parent, MessageTypeDefOf.NeutralEvent);
                }
                if (studier != null && !Props.completedLetterText.NullOrEmpty() && !Props.completedLetterTitle.NullOrEmpty())
                {
                    Find.LetterStack.ReceiveLetter(Props.completedLetterTitle.Formatted(studier.Named("STUDIER"), parent.Named("PARENT")), Props.completedLetterText.Formatted(studier.Named("STUDIER"), parent.Named("PARENT")), Props.completedLetterDef ?? LetterDefOf.NeutralEvent, new List<Thing> { parent, studier });
                }
            }
        }
    }
}
