using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_StudyNotepad : CompProperties_Interactable
    {
        public float learnXPPerProgressPoint = 80;
        public ResearchProjectDef RequiredResearchDef;

        public CompProperties_StudyNotepad()
        {
            compClass = typeof(Comp_StudyNotepad);
        }
    }
}
