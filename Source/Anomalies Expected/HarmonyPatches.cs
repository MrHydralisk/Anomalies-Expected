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
    }
}
