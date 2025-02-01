using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
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
    }
}
