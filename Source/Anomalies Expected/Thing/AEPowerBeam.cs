﻿using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class AEPowerBeam : OrbitalStrike
    {
        public float Radius = 5f;

        private const int FiresStartedPerTick = 2;

        private static readonly IntRange FlameDamageAmountRange = new IntRange(75, 120);

        private static readonly IntRange CorpseFlameDamageAmountRange = new IntRange(7, 14);

        private static List<Thing> tmpThings = new List<Thing>();

        public override void StartStrike()
        {
            base.StartStrike();
            Mote obj = (Mote)ThingMaker.MakeThing(ThingDefOf.Mote_PowerBeam);
            obj.exactPosition = base.Position.ToVector3Shifted();
            obj.Scale = 6 * Radius;
            obj.rotationRate = 1.2f;
            GenSpawn.Spawn(obj, base.Position, base.Map);
        }

        protected override void Tick()
        {
            base.Tick();
            if (!base.Destroyed)
            {
                for (int i = 0; i < FiresStartedPerTick; i++)
                {
                    StartRandomFireAndDoFlameDamage();
                }
            }
        }

        private void StartRandomFireAndDoFlameDamage()
        {
            IntVec3 c = (from x in GenRadial.RadialCellsAround(base.Position, Radius, useCenter: true)
                         where x.InBounds(base.Map)
                         select x).RandomElementByWeight((IntVec3 x) => 1f - Mathf.Min(x.DistanceTo(base.Position) / Radius, 1f) + 0.05f);
            FireUtility.TryStartFireIn(c, base.Map, Rand.Range(0.1f, 0.925f), instigator);
            tmpThings.Clear();
            tmpThings.AddRange(c.GetThingList(base.Map));
            for (int i = 0; i < tmpThings.Count; i++)
            {
                int num = ((tmpThings[i] is Corpse) ? CorpseFlameDamageAmountRange.RandomInRange : FlameDamageAmountRange.RandomInRange);
                Pawn pawn = tmpThings[i] as Pawn;
                BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = null;
                if (pawn != null)
                {
                    battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(pawn, RulePackDefOf.DamageEvent_PowerBeam, instigator as Pawn);
                    Find.BattleLog.Add(battleLogEntry_DamageTaken);
                }
                tmpThings[i].TakeDamage(new DamageInfo(DamageDefOf.Flame, num, 0f, -1f, instigator, null, weaponDef)).AssociateWithLog(battleLogEntry_DamageTaken);
            }
            tmpThings.Clear();
        }
    }
}
