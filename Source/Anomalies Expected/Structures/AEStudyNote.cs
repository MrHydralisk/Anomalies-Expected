using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class AEStudyNote : StudyNote
    {
        [MustTranslate]
        public string AnomalyLabel;
        [MustTranslate]
        public string AnomalyDesc;
        [MustTranslate]
        public string AnomalyDescPart;
        public ThingDef ThingDefSpawn;
    }
}