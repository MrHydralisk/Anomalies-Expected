﻿using RimWorld;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class DamageWorker_PsychicFlash : DamageWorker_AddInjury
    {
        public override DamageResult Apply(DamageInfo dinfo, Thing thing)
        {
            if (thing is Pawn pawn)
            {
                float statCasterValue = (dinfo.Instigator as Pawn)?.GetStatValue(StatDefOf.PsychicSensitivity) ?? 1;
                float statValue = pawn.GetStatValue(StatDefOf.PsychicSensitivity);
                if (statValue > 0f)
                {
                    pawn.stances?.stunner?.StunFor(Mathf.RoundToInt(250f * statCasterValue * statValue), null);
                }
            }
            return base.Apply(dinfo, thing);
        }
    }
}
