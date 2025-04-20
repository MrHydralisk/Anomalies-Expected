using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace AnomaliesExpected
{
    public class PsychicRitualDef_AEConnectEntityDatabase : PsychicRitualDef_VoidProvocation
    {
        //public ThingDef ProviderBoxDef;
        //public float MaxWealth;
        //public ResearchProjectDef researchProjectDef;
        //public float maxResearchMult;
        //public List<Vector2> multFromQuality = new List<Vector2>();
        //private Building_Storage ProviderBox;


        //private static Dictionary<PsychicRitualRoleDef, List<IntVec3>> tmpParticipants = new Dictionary<PsychicRitualRoleDef, List<IntVec3>>(8);

        public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph graph)
        {
            List<PsychicRitualToil> list = base.CreateToils(psychicRitual, graph);
            list.Replace(list.FirstOrDefault((PsychicRitualToil prt) => prt is PsychicRitualToil_VoidProvocation), new PsychicRitualToil_AEConnectEntityDatabase(InvokerRole, psychicShockChanceFromQualityCurve));
            return list;
        }

        //public override IEnumerable<string> BlockingIssues(PsychicRitualRoleAssignments assignments, Map map)
        //{
        //    foreach (string item in base.BlockingIssues(assignments, map))
        //    {
        //        yield return item;
        //    }
        //    CompAffectedByFacilities compAffectedByFacilities = (assignments.Target.Thing as ThingWithComps).GetComp<CompAffectedByFacilities>();
        //    ProviderBox = compAffectedByFacilities.LinkedFacilitiesListForReading.FirstOrDefault((Thing t) => t.def == ProviderBoxDef) as Building_Storage;
        //    if (ProviderBox == null)
        //    {
        //        yield return "AnomaliesExpected.ProviderScripture.Ritual.MissingTheBox".Translate();
        //    }
        //    if (ProviderBox.slotGroup.HeldThings.EnumerableNullOrEmpty())
        //    {
        //        yield return "AnomaliesExpected.ProviderScripture.Ritual.BoxEmpty".Translate();
        //    }
        //}

        //public override TaggedString OutcomeDescription(FloatRange qualityRange, string qualityNumber, PsychicRitualRoleAssignments assignments)
        //{
        //    string desc = outcomeDescription.Formatted();
        //    Thing inputThing = ProviderBox?.slotGroup.HeldThings.FirstOrDefault() ?? null;
        //    if (inputThing != null)
        //    {
        //        float mult = MultFromQuality(qualityRange.min) * MultFromResearch();
        //        int inputAmount = InputAmount(inputThing);
        //        int outputAmount = Mathf.FloorToInt(inputAmount * mult);
        //        desc += "AnomaliesExpected.ProviderScripture.Ritual.OutputDesc".Translate($"{inputThing.def.label} x{inputAmount}", MultFromQuality(qualityRange.min).ToStringPercent(), MultFromResearch().ToStringPercent(), mult.ToStringPercent(), $"{inputThing.def.label} x{outputAmount}");
        //    }
        //    return desc;
        //}

        //public float MultFromQuality(float Quality)
        //{
        //    return multFromQuality.FirstOrDefault((Vector2 v) => Quality <= v.x).y;
        //}

        //public float MultFromResearch()
        //{
        //    float mult = 1f;
        //    if (researchProjectDef != null)
        //    {
        //        mult += researchProjectDef.ProgressReal / researchProjectDef.knowledgeCost * maxResearchMult;
        //    }
        //    return mult;
        //}

        //public int InputAmount(Thing inputThing)
        //{
        //    return Mathf.Min(Mathf.FloorToInt(MaxWealth / inputThing.def.BaseMarketValue), inputThing.stackCount);
        //}

        //private IReadOnlyDictionary<PsychicRitualRoleDef, List<IntVec3>> GenerateRolePositions(PsychicRitualRoleAssignments assignments)
        //{
        //    tmpParticipants.ClearAndPoolValueLists();
        //    foreach (PsychicRitualRoleDef role in Roles)
        //    {
        //        tmpParticipants[role] = SimplePool<List<IntVec3>>.Get();
        //    }
        //    int num = assignments.RoleAssignedCount(ChanterRole) + assignments.RoleAssignedCount(InvokerRole);
        //    int num2 = 0;
        //    foreach (Pawn item in assignments.AssignedPawns(InvokerRole))
        //    {
        //        _ = item;
        //        int num3 = 0;
        //        IntVec3 cell;
        //        do
        //        {
        //            cell = assignments.Target.Cell;
        //            cell += IntVec3.FromPolar(360f * (float)num2++ / (float)num, invocationCircleRadius);
        //        }
        //        while (!cell.Walkable(assignments.Target.Map) && num3++ <= 10);
        //        if (num3 >= 10)
        //        {
        //            cell = assignments.Target.Cell;
        //        }
        //        tmpParticipants[InvokerRole].Add(cell);
        //    }
        //    foreach (Pawn item2 in assignments.AssignedPawns(ChanterRole))
        //    {
        //        _ = item2;
        //        IntVec3 cell2 = assignments.Target.Cell;
        //        cell2 += IntVec3.FromPolar(360f * (float)num2++ / (float)num, invocationCircleRadius);
        //        tmpParticipants[ChanterRole].Add(cell2);
        //    }
        //    foreach (Pawn item3 in assignments.AssignedPawns(TargetRole))
        //    {
        //        _ = item3;
        //        tmpParticipants[TargetRole].Add(assignments.Target.Cell);
        //    }
        //    if (DefenderRole != null)
        //    {
        //        num2 = 0;
        //        int num4 = assignments.RoleAssignedCount(DefenderRole);
        //        bool playerRitual = assignments.AllAssignedPawns.Any((Pawn x) => x.Faction == Faction.OfPlayer);
        //        foreach (Pawn item4 in assignments.AssignedPawns(DefenderRole))
        //        {
        //            _ = item4;
        //            IntVec3 cell3 = assignments.Target.Cell;
        //            cell3 += IntVec3.FromPolar(360f * (float)num2++ / (float)num4, invocationCircleRadius + 5f);
        //            cell3 = GetBestStandableRolePosition(playerRitual, cell3, assignments.Target.Cell, assignments.Target.Map);
        //            tmpParticipants[DefenderRole].Add(cell3);
        //        }
        //    }
        //    return tmpParticipants;
        //}
    }
}
