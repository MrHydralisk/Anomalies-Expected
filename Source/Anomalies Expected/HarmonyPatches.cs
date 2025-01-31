using HarmonyLib;
using System;
using Verse;

namespace AnomaliesExpected
{
    [StaticConstructorOnStartup]
    public class HarmonyPatches
    {
        private static readonly Type patchType;

        static HarmonyPatches()
        {
            patchType = typeof(HarmonyPatches);
            Harmony val = new Harmony("rimworld.mrhydralisk.AnomaliesExpectedPatch");

            val.Patch(AccessTools.Property(typeof(ResearchProjectDef), "IsHidden").GetGetMethod(), prefix: new HarmonyMethod(patchType, "RPD_IsHidden_Prefix"));
            val.Patch(AccessTools.Property(typeof(RaceProperties), "IsAnomalyEntity").GetGetMethod(), postfix: new HarmonyMethod(patchType, "RP_IsAnomalyEntity_Postfix"));
        }

        public static bool RPD_IsHidden_Prefix(ref bool __result, ResearchProjectDef __instance)
        {
            if (!__instance.IsFinished && ModsConfig.AnomalyActive)
            {
                __result = Find.EntityCodex.Hidden(__instance) || (__instance.knowledgeCategory != null && !__instance.AnalyzedThingsRequirementsMet);
                return false;
            }
            return true;
        }

        public static void RP_IsAnomalyEntity_Postfix(ref bool __result, RaceProperties __instance)
        {
            __result = __result || __instance.FleshType == FleshTypeDefOfLocal.AE_EntitySnowArmy;
        }
    }
}
