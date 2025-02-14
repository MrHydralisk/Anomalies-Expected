﻿using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AnomaliesExpected
{
    public class SnowArmyMapComponent : MapComponent
    {
        public int refreshTargetsForSnowBlockTick;
        public List<Thing> TargetsForSnowBlock = new List<Thing>();
        public Faction snowArmyFaction;

        public SnowArmyMapComponent(Map map) : base(map)
        {
        }

        public bool isReadyToWork()
        {
            if (snowArmyFaction == null)
            {
                snowArmyFaction = Find.FactionManager.FirstFactionOfDef(FactionDefOfLocal.AE_SnowArmy);
            }
            return true;
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (Find.TickManager.TicksGame >= refreshTargetsForSnowBlockTick)
            {
                RefreshTargetsForSnowBlock();
                refreshTargetsForSnowBlockTick = Find.TickManager.TicksGame + 30000;
            }
        }
        public void RefreshTargetsForSnowBlock()
        {
            if (!isReadyToWork())
            {
                return;
            }
            foreach (Thing thing in map.listerThings.AllThings)
            {
                if (!thing.Fogged() && thing.Faction.HostileTo(snowArmyFaction))
                {
                    if (thing is Pawn tPawn && !tPawn.Dead)
                    {
                        TargetsForSnowBlock.Add(thing);
                    }
                    else if (thing is Building tBuilding && tBuilding.def.building.ai_combatDangerous)
                    {
                        CompStunnable compStunnable = tBuilding.GetComp<CompStunnable>();
                        if (compStunnable != null && compStunnable.CanBeStunnedByDamage(DamageDefOfLocal.AENitrogen) && !compStunnable.StunHandler.Stunned)
                        {
                            TargetsForSnowBlock.Add(thing);
                        }
                    }
                }
            }
        }
    }
}
