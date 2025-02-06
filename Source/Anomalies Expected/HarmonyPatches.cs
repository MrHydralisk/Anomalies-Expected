using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
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

            List<ThingDef> allDefsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;
            foreach (ThingDef item in allDefsListForReading)
            {
                if (item.building != null && item.HasComp<CompStunnable>())
                {
                    CompProperties_Stunnable compProperties_Stunnable = item.GetCompProperties<CompProperties_Stunnable>();
                    compProperties_Stunnable.affectedDamageDefs.Add(DamageDefOfLocal.AENitrogen);
                }
            }

            val.Patch(AccessTools.Property(typeof(ResearchProjectDef), "IsHidden").GetGetMethod(), prefix: new HarmonyMethod(patchType, "RPD_IsHidden_Prefix"));
            val.Patch(AccessTools.Property(typeof(RaceProperties), "IsAnomalyEntity").GetGetMethod(), postfix: new HarmonyMethod(patchType, "RP_IsAnomalyEntity_Postfix"));
            val.Patch(AccessTools.Method(typeof(BackCompatibility), "FactionManagerPostLoadInit"), postfix: new HarmonyMethod(patchType, "BC_FactionManagerPostLoadInit_Postfix"));
            
            val.Patch(AccessTools.Method(typeof(PlaySettings), "DoPlaySettingsGlobalControls"), postfix: new HarmonyMethod(patchType, "PC_DoPlaySettingsGlobalControls_Postfix"));
            val.Patch(AccessTools.Method(typeof(Dialog_EntityCodex), "DoWindowContents"), postfix: new HarmonyMethod(patchType, "DEC_DoWindowContents_Postfix"));
            val.Patch(AccessTools.Method(typeof(CompStudyUnlocks), "Notify_StudyLevelChanged"), postfix: new HarmonyMethod(patchType, "CSU_Notify_StudyLevelChanged_Postfix"));
            val.Patch(AccessTools.Method(typeof(CompStudyUnlocksMonolith), "Notify_StudyLevelChanged"), postfix: new HarmonyMethod(patchType, "CSU_Notify_StudyLevelChanged_Postfix"));
            val.Patch(AccessTools.Method(typeof(CompStudyUnlocks), "PostPostMake"), postfix: new HarmonyMethod(patchType, "CSU_Notify_StudyLevelChanged_Postfix"));
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

        public static void BC_FactionManagerPostLoadInit_Postfix()
        {
            Faction SnowArmy = Find.FactionManager.FirstFactionOfDef(FactionDefOfLocal.AE_SnowArmy);
            if (SnowArmy == null)
            {
                SnowArmy = FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms(FactionDefOfLocal.AE_SnowArmy));
                Find.FactionManager.Add(SnowArmy);
            }
            FactionRelation factionRelationAB = SnowArmy.RelationWith(Find.FactionManager.OfMechanoids);
            factionRelationAB.baseGoodwill = 0;
            factionRelationAB.kind = FactionRelationKind.Neutral;
            FactionRelation factionRelationBA = Find.FactionManager.OfMechanoids.RelationWith(SnowArmy);
            factionRelationBA.baseGoodwill = 0;
            factionRelationBA.kind = FactionRelationKind.Neutral;
        }

        public static void PC_DoPlaySettingsGlobalControls_Postfix(WidgetRow row, bool worldView)
        {
            if (!worldView && ModsConfig.AnomalyActive && Find.Anomaly.AnomalyStudyEnabled)
            {
                if (row.ButtonIcon(ContentFinder<Texture2D>.Get("UI/Buttons/AEEntityCodex"), "AnomaliesExpected.EntityDataBase.Tip".Translate()))
                {
                    Find.WindowStack.Add(new Dialog_AEEntityDB());
                }
            }
        }

        public static void DEC_DoWindowContents_Postfix(Dialog_EntityCodex __instance, Rect inRect)
        {
            Rect rect2 = new Rect(inRect.width - 39, 7, 32, 32);
            if (Widgets.ButtonImage(rect2, ContentFinder<Texture2D>.Get("UI/Buttons/AEEntityCodex"), tooltip: "AnomaliesExpected.EntityDataBase.Tip".Translate()))
            {
                Find.WindowStack.Add(new Dialog_AEEntityDB());
                __instance.Close();
            }
        }

        public static void CSU_Notify_StudyLevelChanged_Postfix(CompStudyUnlocks __instance)
        {
            GameComponent_AnomaliesExpected.instance.SyncEntityEntryFromVanilla(__instance);
        }
    }
}
