using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_AEStudyUnlocks : CompProperties_StudyUnlocks
    {
        public bool isSyncWithParent;

        public CompProperties_AEStudyUnlocks()
        {
            compClass = typeof(CompAEStudyUnlocks);
        }
    }
}
