﻿using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
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
            val.Patch(AccessTools.Method(typeof(MainTabWindow_Research), "ListProjects"), transpiler: new HarmonyMethod(patchType, "MTWR_ListProjects_Transpiler"));
            val.Patch(AccessTools.Method(typeof(MainTabWindow_Research), "Select"), prefix: new HarmonyMethod(patchType, "MTWR_Select_Prefix"));
            val.Patch(AccessTools.Property(typeof(RaceProperties), "IsAnomalyEntity").GetGetMethod(), postfix: new HarmonyMethod(patchType, "RP_IsAnomalyEntity_Postfix"));
            val.Patch(AccessTools.Method(typeof(BackCompatibility), "FactionManagerPostLoadInit"), postfix: new HarmonyMethod(patchType, "BC_FactionManagerPostLoadInit_Postfix"));

            val.Patch(AccessTools.Method(typeof(PlaySettings), "DoPlaySettingsGlobalControls"), postfix: new HarmonyMethod(patchType, "PC_DoPlaySettingsGlobalControls_Postfix"));
            val.Patch(AccessTools.Method(typeof(Dialog_EntityCodex), "DoWindowContents"), postfix: new HarmonyMethod(patchType, "DEC_DoWindowContents_Postfix"));
            val.Patch(AccessTools.Method(typeof(CompStudyUnlocks), "Notify_StudyLevelChanged"), postfix: new HarmonyMethod(patchType, "CSU_Notify_StudyLevelChanged_Postfix"));
            val.Patch(AccessTools.Method(typeof(CompStudyUnlocksMonolith), "Notify_StudyLevelChanged"), postfix: new HarmonyMethod(patchType, "CSU_Notify_StudyLevelChanged_Postfix"));
            val.Patch(AccessTools.Method(typeof(CompStudyUnlocks), "PostPostMake"), postfix: new HarmonyMethod(patchType, "CSU_Notify_StudyLevelChanged_Postfix"));
            val.Patch(AccessTools.Method(typeof(CompStudyUnlocks), "OnStudied"), postfix: new HarmonyMethod(patchType, "CSU_OnStudied_Postfix"));
            val.Patch(AccessTools.Method(typeof(ITab_StudyNotes), "DrawTitle"), postfix: new HarmonyMethod(patchType, "ITSN_DrawTitle_Postfix"));

            val.Patch(AccessTools.Method(typeof(Thing), "GetGizmos"), postfix: new HarmonyMethod(patchType, "T_GetGizmos_Postfix"));
            val.Patch(AccessTools.Method(typeof(UnnaturalCorpse), "GetGizmos"), postfix: new HarmonyMethod(patchType, "T_GetGizmos_Postfix"));
            val.Patch(AccessTools.Method(typeof(Pawn_MutantTracker), "GetGizmos"), postfix: new HarmonyMethod(patchType, "T_GetGizmos_Postfix"));

            val.Patch(AccessTools.Property(typeof(ChoiceLetter_EntityDiscovered), "Choices").GetGetMethod(), prefix: new HarmonyMethod(patchType, "CLED_Choices_Prefix"));
            val.Patch(AccessTools.Method(typeof(EntityCodex), "SetDiscovered", new Type[] { typeof(EntityCodexEntryDef), typeof(ThingDef), typeof(Thing) }), transpiler: new HarmonyMethod(patchType, "EC_SetDiscovered_Transpiler"));

            val.Patch(AccessTools.Method(typeof(VerbUtility), "IsEMP"), postfix: new HarmonyMethod(patchType, "VU_IsEMP_Postfix"));
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

        public static IEnumerable<CodeInstruction> MTWR_ListProjects_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 3; i++)
            {
                if (codes[i].Calls(AccessTools.Property(typeof(ResearchProjectDef), "IsHidden").GetGetMethod()) && codes[i + 3].opcode == OpCodes.Ldloc_S && codes[i + 3].operand.ToString().Contains("UnityEngine.Rect (15)"))
                {
                    List<CodeInstruction> instructionsToInsert = new List<CodeInstruction>();
                    Label labelTrue = il.DefineLabel();
                    Label labelDone = (Label)codes[i + 1].operand;
                    codes[i + 1] = new CodeInstruction(OpCodes.Brfalse_S, labelTrue);
                    codes[i + 2].labels.Add(labelTrue);
                    instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_S, 10));
                    instructionsToInsert.Add(codes[i - 1]);
                    instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_S, 12));
                    instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_S, 15));
                    instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_S, 25));
                    instructionsToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), "ResearchDrawBottomRow")));
                    instructionsToInsert.Add(new CodeInstruction(OpCodes.Br, labelDone));
                    codes.InsertRange(i + 2, instructionsToInsert);
                    break;
                }
            }
            return codes.AsEnumerable();
        }

        public static void ResearchDrawBottomRow(MainTabWindow_Research mainTab, ResearchProjectDef researchProjectDef, Rect rectProj, Rect rectRow, Color studiedColor)
        {
            if (researchProjectDef.knowledgeCategory != null && researchProjectDef.RequiredAnalyzedThingCount > 0)
            {
                Color color = GUI.color;
                string text = AccessTools.Method(typeof(MainTabWindow_Research), "GetTechprintsInfoCached").Invoke(mainTab, new object[] { researchProjectDef.AnalyzedThingsCompleted, researchProjectDef.RequiredAnalyzedThingCount }) as string;
                Vector2 vector2 = Text.CalcSize(text);
                Rect rect1 = rectRow;
                rect1.xMin = rectRow.xMax - vector2.x - 10f;
                Rect rect2 = rectRow;
                rect2.width = rect2.height;
                rect2.x = rect1.x - rect2.width;
                GUI.color = studiedColor;
                Widgets.Label(rect1, text);
                GUI.color = Color.white;
                GUI.DrawTexture(rect2.ContractedBy(4f), (AccessTools.Field(typeof(MainTabWindow_Research), "StudyRequirementTex").GetValue(mainTab) as CachedTexture).Texture);
                GUI.color = color;
                if (Mouse.IsOver(rectProj) && !((bool)AccessTools.Field(typeof(MainTabWindow_Research), "editMode").GetValue(mainTab)))
                {
                    Widgets.DrawLightHighlight(rectProj);
                    List<string> inspectStrings = new List<string>();
                    inspectStrings.Add("AnomaliesExpected.Misc.ResearchNote.HiddenTip.0".Translate());
                    foreach (ThingDef thingDef in researchProjectDef.requiredAnalyzed)
                    {
                        inspectStrings.Add("- " + thingDef.LabelCap);
                    }
                    inspectStrings.Add("AnomaliesExpected.Misc.ResearchNote.HiddenTip.1".Translate());
                    TooltipHandler.TipRegion(rectProj, () => String.Join("\n", inspectStrings), researchProjectDef.GetHashCode() ^ 0x1664F);
                }
            }
        }

        public static bool MTWR_Select_Prefix(MainTabWindow_Research __instance, ResearchProjectDef project)
        {
            if (project.IsHidden)
            {
                __instance.CurTab = project.tab;
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
            if(Find.FactionManager.OfMechanoids == null) return;
            
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

        public static void T_GetGizmos_Postfix(ref IEnumerable<Gizmo> __result, Thing __instance)
        {
            List<Gizmo> NGizmos = __result.ToList();
            int index = NGizmos.FirstIndexOf((Gizmo g) => g is Command_Action ca && ca.defaultLabel == "EntityCodex".Translate() + "...") + 1;
            if (index > 0)
            {
                Gizmo gizmo = new Command_Action
                {
                    defaultLabel = "AnomaliesExpected.EntityDataBase.Label".Translate(),
                    defaultDesc = "AnomaliesExpected.EntityDataBase.Tip".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Buttons/AEEntityDB"),
                    action = delegate
                    {
                        AEEntityEntry selectedEntityEntry = GameComponent_AnomaliesExpected.instance.GetEntityEntryFromThingDef((__instance as Building_HoldingPlatform)?.HeldPawn.def ?? __instance.def);
                        Dialog_AEEntityDB dialog = new Dialog_AEEntityDB();
                        if (selectedEntityEntry != null)
                        {
                            dialog.SelectEntry(selectedEntityEntry);
                        }
                        Find.WindowStack.Add(dialog);
                    }
                };
                NGizmos.Insert(Mathf.Clamp(0, index, NGizmos.Count()), gizmo);
            }
            __result = NGizmos;
        }

        public static bool CLED_Choices_Prefix(ref IEnumerable<DiaOption> __result, ChoiceLetter_EntityDiscovered __instance)
        {
            List<DiaOption> NDiaOptions = new List<DiaOption>();
            if (__instance.codexEntry != null)
            {
                foreach (ResearchProjectDef project in __instance.codexEntry.discoveredResearchProjects)
                {
                    if (project.IsHidden)
                    {
                        NDiaOptions.Add(new DiaOption($"[{"Undiscovered".Translate()}]")
                        {
                            action = delegate
                            {
                                Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Research);
                                if (MainButtonDefOf.Research.TabWindow is MainTabWindow_Research mainTabWindow_Research)
                                {
                                    mainTabWindow_Research.CurTab = project.tab;
                                }
                            },
                            resolveTree = true
                        });
                    }
                    else
                    {
                        NDiaOptions.Add(new DiaOption("ViewHyperlink".Translate(project.label))
                        {
                            action = delegate
                            {
                                Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Research);
                                if (MainButtonDefOf.Research.TabWindow is MainTabWindow_Research mainTabWindow_Research)
                                {
                                    mainTabWindow_Research.Select(project);
                                }
                            },
                            resolveTree = true
                        });
                    }
                }
                NDiaOptions.Add(new DiaOption("ViewEntityCodex".Translate())
                {
                    action = delegate
                    {
                        if (__instance.codexEntry != null)
                        {
                            Find.WindowStack.Add(new Dialog_EntityCodex(__instance.codexEntry));
                        }
                    },
                    resolveTree = true
                });
                NDiaOptions.Add(new DiaOption("AnomaliesExpected.EntityDataBase.View".Translate())
                {
                    action = delegate
                    {
                        if (__instance.codexEntry != null)
                        {
                            AEEntityEntry selectedEntityEntry = GameComponent_AnomaliesExpected.instance.GetEntityEntryFromEntityCodexEntryDef(__instance.codexEntry);
                            Dialog_AEEntityDB dialog = new Dialog_AEEntityDB();
                            if (selectedEntityEntry != null)
                            {
                                dialog.SelectEntry(selectedEntityEntry);
                            }
                            Find.WindowStack.Add(dialog);
                        }
                    },
                    resolveTree = true
                });
            }
            NDiaOptions.Add(new DiaOption("Close".Translate())
            {
                action = delegate
                {
                    Find.LetterStack.RemoveLetter(__instance);
                },
                resolveTree = true
            });
            __result = NDiaOptions;
            return false;
        }

        public static IEnumerable<CodeInstruction> EC_SetDiscovered_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count - 20; i++)
            {
                if (codes[i].opcode == OpCodes.Ldstr && codes[i].operand.ToString().Contains("ENTITY"))
                {
                    codes[i + 6] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), "DiscoveredResearchProjects"));
                    codes.RemoveRange(i + 7, 13);
                    break;
                }
            }
            return codes.AsEnumerable();
        }

        public static string DiscoveredResearchProjects(EntityCodexEntryDef entry)
        {
            return entry.discoveredResearchProjects.Select((ResearchProjectDef rpd) => rpd.IsHidden ? $"[{"Undiscovered".Translate()}]" : rpd.LabelCap.ToString()).ToLineList("  - ");
        }

        public static void VU_IsEMP_Postfix(ref bool __result, Verb verb)
        {
            if (__result && verb.verbProps.LaunchesProjectile)
            {
                __result = !(verb.GetProjectile()?.projectile?.extraDamages?.Any((ExtraDamage ed) => ed.def != DamageDefOf.EMP) ?? false);
            }
        }
    }
}
