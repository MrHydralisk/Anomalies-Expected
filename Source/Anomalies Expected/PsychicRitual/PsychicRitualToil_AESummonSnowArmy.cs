using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class PsychicRitualToil_AESummonSnowArmy : PsychicRitualToil
    {
        private PsychicRitualRoleDef invokerRole;

        protected PsychicRitualToil_AESummonSnowArmy()
        {
        }

        public PsychicRitualToil_AESummonSnowArmy(PsychicRitualRoleDef invokerRole)
        {
            this.invokerRole = invokerRole;
        }

        public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph parent)
        {
            base.Start(psychicRitual, parent);
            Pawn pawn = psychicRitual.assignments.FirstAssignedPawn(invokerRole);
            psychicRitual.ReleaseAllPawnsAndBuildings();
            if (pawn != null)
            {
                ApplyOutcome(psychicRitual, pawn);
            }
        }

        private void ApplyOutcome(PsychicRitual psychicRitual, Pawn invoker)
        {
            PsychicRitualDef_AESummonSnowArmy def = psychicRitual.def as PsychicRitualDef_AESummonSnowArmy;
            IncidentParms incidentParms = new IncidentParms();
            incidentParms.target = invoker.Map;
            incidentParms.points = StorytellerUtility.DefaultThreatPointsNow(invoker.Map);
            incidentParms.pointMultiplier = def.CombatPointsMultFromQualityCurve.Evaluate(psychicRitual.PowerPercent);
            incidentParms.forced = true;
            if (Find.Anomaly.LevelDef.anomalyThreatTier > 1 || !Find.Anomaly.GenerateMonolith)
            {
                Find.Storyteller.incidentQueue.Add(def.IncidentDefAdvanced, Find.TickManager.TicksGame, incidentParms);
            }
            else
            {
                Find.Storyteller.incidentQueue.Add(def.IncidentDefBasic, Find.TickManager.TicksGame, incidentParms);
            }

            IntVec3 position = psychicRitual.assignments.Target.Cell;
            Map map = psychicRitual.assignments.Target.Map;
            List<IntVec3> cells = GenRadial.RadialCellsAround(position, 9.9f, true).ToList();
            List<IntVec3> cellsAffected = new List<IntVec3>();
            foreach (IntVec3 cell in cells)
            {
                if (cell.InBounds(map) && GenSight.LineOfSight(position, cell, map, skipFirstCell: true))
                {
                    cellsAffected.Add(cell);
                }
            }
            SoundInfo soundInfo = new TargetInfo(position, map);
            soundInfo.volumeFactor *= 0.2f;
            soundInfo.pitchFactor *= 0.5f;
            def.soundDef.PlayOneShot(soundInfo);
            foreach (IntVec3 cell in cellsAffected)
            {
                float lengthHorizontal = (position - cell).LengthHorizontal;
                float num2 = 1f - lengthHorizontal / 9.9f;
                map.snowGrid.AddDepth(cell, num2 * 1);
            }
            float tempDiff = invoker.AmbientTemperature + 10f;
            if (tempDiff > 1)
            {
                int cellsCount = Mathf.Min(invoker.GetRoom()?.CellCount ?? int.MaxValue, 225);
                GenTemperature.PushHeat(position, map, -tempDiff * cellsCount);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref invokerRole, "invokerRole");
        }
    }
}
