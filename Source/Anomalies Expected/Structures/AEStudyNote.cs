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
        public int ThreatClass = -1;
        public ThingDef ThingDefSpawn;
    }
}