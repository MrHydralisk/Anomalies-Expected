﻿using Verse;

namespace AnomaliesExpected
{
    public class Comp_BloodLakeTerminal : Comp_CanDestroyedAfterStudy
    {
        private BloodLakeMapComponent mapComponent => mapComponentCached ?? (mapComponentCached = parent.Map?.GetComponent<BloodLakeMapComponent>() ?? null);
        private BloodLakeMapComponent mapComponentCached;


        public override void DestroyAnomaly(Pawn caster = null)
        {
            mapComponent.Entrance.StudyUnlocks.UnlockStudyNoteManual(1, caster?.LabelShortCap ?? "");
            mapComponent.DestroySubMap();
        }
    }
}
