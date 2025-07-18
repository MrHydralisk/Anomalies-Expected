using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class CreepJoinerWorker_AEAggressiveIncident : BaseCreepJoinerWorker
    {
        public CreepJoinerDefExtension Ext => Tracker.aggressive.GetModExtension<CreepJoinerDefExtension>();

        public override bool CanOccurOnDeath => true;

        public override void DoResponse(List<TargetInfo> looktargets, List<NamedArgument> namedArgs)
        {
            if (!base.Pawn.SpawnedOrAnyParentSpawned)
            {
                return;
            }
            if (!base.Pawn.Dead)
            {
                Faction faction = Find.FactionManager.FirstFactionOfDef(Ext.factionDef) ?? Faction.OfEntities;
                base.Pawn.SetFaction(faction);
                if (base.Pawn.GetLord() != null)
                {
                    base.Pawn.GetLord().Notify_PawnLost(base.Pawn, PawnLostCondition.Undefined);
                }
                LordMaker.MakeNewLord(faction, new LordJob_AssaultColony(faction, canKidnap: Ext.canKidnap, canTimeoutOrFlee: Ext.canTimeoutOrFlee, sappers: false, useAvoidGridSmart: false, canSteal: false), base.Pawn.MapHeld).AddPawn(base.Pawn);
                foreach (Ability ability in base.Pawn.abilities.abilities)
                {
                    ability.ResetCooldown();
                }
            }
            IncidentParms incidentParms = new IncidentParms();
            incidentParms.target = base.Pawn.MapHeld;
            incidentParms.points = StorytellerUtility.DefaultThreatPointsNow(base.Pawn.MapHeld);
            incidentParms.forced = true;
            incidentParms.bypassStorytellerSettings = true;
            Find.Storyteller.incidentQueue.Add(Ext.incidentDef, Find.TickManager.TicksGame + Ext.ticksBeforeIncident, incidentParms);

            IntVec3 position = base.Pawn.PositionHeld;
            Map map = base.Pawn.MapHeld;
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
            Ext.soundDef.PlayOneShot(soundInfo);
            foreach (IntVec3 cell in cellsAffected)
            {
                float lengthHorizontal = (position - cell).LengthHorizontal;
                float num2 = 1f - lengthHorizontal / 9.9f;
                map.snowGrid.AddDepth(cell, num2 * 1);
            }
            float tempDiff = base.Pawn.AmbientTemperature + 10f;
            if (tempDiff > 1)
            {
                int cellsCount = Mathf.Min(base.Pawn.GetRoom()?.CellCount ?? int.MaxValue, 225);
                GenTemperature.PushHeat(position, map, -tempDiff * cellsCount);
            }
        }
    }
}
