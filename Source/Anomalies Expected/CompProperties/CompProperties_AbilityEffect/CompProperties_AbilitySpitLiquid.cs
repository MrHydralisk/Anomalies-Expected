﻿using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_AbilitySpitLiquid : CompProperties_AbilityEffect
    {
        public ThingDef projectileDef;
        public bool isAICanTargetAlways = false;
        public EffecterDef sprayEffecter;

        public CompProperties_AbilitySpitLiquid()
        {
            compClass = typeof(CompAbilityEffect_SpitLiquid);
        }
    }
}
