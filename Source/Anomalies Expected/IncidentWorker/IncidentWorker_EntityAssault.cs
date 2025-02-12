using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace AnomaliesExpected
{
    public class IncidentWorker_EntityAssault : IncidentWorker
    {
        public IncidentDefExtension Ext => def.GetModExtension<IncidentDefExtension>();
        public List<EntityCodexEntryDef> entityCodexEntryDefsRequired => Ext?.entityCodexEntryDefsRequired ?? null;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (ModsConfig.AnomalyActive && (entityCodexEntryDefsRequired?.Any(eced => !Find.EntityCodex.Discovered(eced)) ?? false))
            {
                return false;
            }
            return base.CanFireNowSub(parms);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Faction faction = Find.FactionManager.FirstFactionOfDef(Ext.factionDef) ?? Faction.OfEntities;
            parms.faction = faction;
            parms.raidArrivalMode = Ext.pawnsArrivalModeDef;
            PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(Ext.pawnGroupKindDef, parms);
            float num = Ext.factionDef.MinPointsToGeneratePawnGroup(Ext.pawnGroupKindDef);
            if (parms.points < num)
            {
                parms.points = (defaultPawnGroupMakerParms.points = num * 2f);
            }
            CheckGetOptions(defaultPawnGroupMakerParms, faction.def);
            List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms).ToList();
            if (!parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms))
            {
                return false;
            }
            parms.raidArrivalMode.Worker.Arrive(list, parms);
            //if (AnomalyIncidentUtility.IncidentShardChance(parms.points))
            //{
            //    AnomalyIncidentUtility.PawnShardOnDeath(list.RandomElement());
            //}
            LordMaker.MakeNewLord(faction, new LordJob_AssaultColony(), parms.target as Map, list);
            SendStandardLetter(def.letterLabel, def.letterText, def.letterDef, parms, list);
            return true;
        }



        public void CheckGetOptions(PawnGroupMakerParms groupParms, FactionDef faction)
        {
            PawnGroupMakerUtility.TryGetRandomPawnGroupMaker(groupParms, out var pawnGroupMaker);
            List<PawnGenOptionWithXenotype> pawnGenOptionWithXenotypes = PawnGroupMakerUtility.GetOptions(groupParms, faction, pawnGroupMaker.options, groupParms.points, groupParms.points, null).ToList();

            float highestCost = pawnGenOptionWithXenotypes.Max(x => x.Cost);
            SimpleCurve PawnWeightFactorByMostExpensivePawnCostFractionCurve = new SimpleCurve
            {
                new CurvePoint(0.2f, 0.01f),
                new CurvePoint(0.3f, 0.3f),
                new CurvePoint(0.5f, 1f)
            };
            foreach (PawnGenOptionWithXenotype option in pawnGenOptionWithXenotypes)
            {
                Log.Message($"CheckGetOptions {option.SelectionWeight} | {option.Cost}");
                CheckPawnGenOptionValid(option.Option, groupParms);
                Log.Message($"CheckGetOptions {option.SelectionWeight * PawnWeightFactorByMostExpensivePawnCostFractionCurve.Evaluate(option.Cost / highestCost)} = option.SelectionWeight * PawnWeightFactorByMostExpensivePawnCostFractionCurve.Evaluate({option.Cost} / {highestCost}) = {option.SelectionWeight} * {PawnWeightFactorByMostExpensivePawnCostFractionCurve.Evaluate(option.Cost / highestCost)}");
            }

        }

        public void CheckPawnGenOptionValid(PawnGenOption o, PawnGroupMakerParms groupParms)
        {
            Log.Message($"CheckPawnGenOptionValid {o.kind.LabelCap}");
            if (groupParms != null)
            {
                if (groupParms.generateFightersOnly && !o.kind.isFighter)
                {
                    Log.Message($"CheckPawnGenOptionValid 0");
                }
                if (groupParms.dontUseSingleUseRocketLaunchers && o.kind.weaponTags != null && o.kind.weaponTags.Contains("GunSingleUse"))
                {
                    Log.Message($"CheckPawnGenOptionValid 1");
                }
                if (groupParms.raidStrategy != null && !groupParms.raidStrategy.Worker.CanUsePawnGenOption(groupParms.points, o, null, groupParms.faction))
                {
                    Log.Message($"CheckPawnGenOptionValid 2");
                }
                if (groupParms.raidAgeRestriction != null && groupParms.raidAgeRestriction.Worker.ShouldApplyToKind(o.kind) && !groupParms.raidAgeRestriction.Worker.CanUseKind(o.kind))
                {
                    Log.Message($"CheckPawnGenOptionValid 3");
                }
            }
            if (ModsConfig.BiotechActive && Find.BossgroupManager.ReservedByBossgroup(o.kind))
            {
                Log.Message($"CheckPawnGenOptionValid 4");
            }
            if (ModsConfig.AnomalyActive && o.kind is CreepJoinerFormKindDef)
            {
                Log.Message($"CheckPawnGenOptionValid 5");
            }
            if (o.kind.maxPerGroup < int.MaxValue)
            {
                Log.Message($"CheckPawnGenOptionValid 6");
            }
            Log.Message($"CheckPawnGenOptionValid True");
            return;
        }
    }
}
