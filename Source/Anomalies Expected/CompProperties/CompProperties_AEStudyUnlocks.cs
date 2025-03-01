using RimWorld;
using System.Collections.Generic;

namespace AnomaliesExpected
{
    public class CompProperties_AEStudyUnlocks : CompProperties_StudyUnlocks
    {
        public bool isSyncWithParent;
        public bool isSyncParentDB;
        public bool isCreateEntityEntryWithoutCodexEntry;
        public bool isCreateSeparateEntityEntry;
        public List<StudyNote> studyNotesManualUnlockable = new List<StudyNote>();
        public List<StudyNote> studyNotesPawnUnlockable = new List<StudyNote>();
        public EntityCategoryDef category;

        public CompProperties_AEStudyUnlocks()
        {
            compClass = typeof(CompAEStudyUnlocks);
        }
    }
}
