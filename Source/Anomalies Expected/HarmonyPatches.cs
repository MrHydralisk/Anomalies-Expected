﻿using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
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
            val.Patch(AccessTools.Method(typeof(CompStudyUnlocks), "OnStudied"), postfix: new HarmonyMethod(patchType, "CSU_OnStudied_Postfix"));
            val.Patch(AccessTools.Method(typeof(Building_VoidMonolith), "GetGizmos"), postfix: new HarmonyMethod(patchType, "BVM_GetGizmos_Postfix"));
            val.Patch(AccessTools.Method(typeof(ITab_StudyNotes), "DrawTitle"), postfix: new HarmonyMethod(patchType, "ITSN_DrawTitle_Postfix"));
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

        public static void CSU_OnStudied_Postfix(CompStudyUnlocks __instance, Pawn studier, float amount, KnowledgeCategoryDef category = null)
        {
            if ((__instance is CompAEStudyUnlocks aEStudyUnlocks) && (aEStudyUnlocks.Props is CompProperties_AEStudyUnlocks aeProps) && !aeProps.studyNotesPawnUnlockable.NullOrEmpty())
            {
                float anomalyKnowledgeGained = aEStudyUnlocks.StudyCompPub.anomalyKnowledgeGained;
                for (int i = aEStudyUnlocks.NextPawnSNIndex; i < aeProps.studyNotesPawnUnlockable.Count; i++)
                {
                    AEStudyNote studyNote = aeProps.studyNotesPawnUnlockable[i] as AEStudyNote;
                    if (anomalyKnowledgeGained >= studyNote.threshold)
                    {
                        aEStudyUnlocks.RegisterPawnStudyLevel(studier, i, studyNote);
                    }
                }
            }
        }

        public static void BVM_GetGizmos_Postfix(ref IEnumerable<Gizmo> __result, Building_VoidMonolith __instance)
        {
            List<Gizmo> NGizmos = __result.ToList();
            Gizmo gizmo = new Command_Action
            {
                defaultLabel = "AnomaliesExpected.EntityDataBase.Label".Translate(),
                defaultDesc = "AnomaliesExpected.EntityDataBase.Tip".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Buttons/AEEntityDB"),
                action = delegate
                {
                    Find.WindowStack.Add(new Dialog_AEEntityDB());
                }
            };
            int index = NGizmos.FirstIndexOf((Gizmo g) => g is Command_Action ca && ca.defaultLabel == "EntityCodex".Translate() + "...") + 1;
            NGizmos.Insert(Mathf.Clamp(0, index, NGizmos.Count()), gizmo);
            __result = NGizmos;
        }

        public static void ITSN_DrawTitle_Postfix(ITab_StudyNotes __instance, Rect rect)
        {
            Rect rect1 = new Rect(rect.width - 34, 0, 26, 26);
            if (Widgets.ButtonImage(rect1, ContentFinder<Texture2D>.Get("UI/Buttons/AEEntityCodex"), tooltip: "AnomaliesExpected.EntityDataBase.Tip".Translate()))
            {
                Dialog_AEEntityDB dialog = new Dialog_AEEntityDB();
                Thing selectedThing = Find.Selector.SingleSelectedThing;
                if (selectedThing != null)
                {
                    AEEntityEntry selectedEntityEntry = GameComponent_AnomaliesExpected.instance.GetEntityEntryFromThingDef((selectedThing as Building_HoldingPlatform)?.HeldPawn.def ?? selectedThing.def);
                    if (selectedEntityEntry != null)
                    {
                        dialog.SelectEntry(selectedEntityEntry);
                    }
                }
                Find.WindowStack.Add(dialog);
                (MainButtonDefOf.Inspect.TabWindow as MainTabWindow_Inspect).CloseOpenTab();
            }
        }
    }
}
