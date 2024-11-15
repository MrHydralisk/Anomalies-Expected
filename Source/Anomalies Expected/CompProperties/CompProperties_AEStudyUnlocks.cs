﻿using RimWorld;
using System.Collections.Generic;

namespace AnomaliesExpected
{
    public class CompProperties_AEStudyUnlocks : CompProperties_StudyUnlocks
    {
        public bool isSyncWithParent;
        public List<StudyNote> studyNotesManualUnlockable = new List<StudyNote>();

        public CompProperties_AEStudyUnlocks()
        {
            compClass = typeof(CompAEStudyUnlocks);
        }
    }
}
