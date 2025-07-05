using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class CompObelisk_Clockwork : CompInteractable
    {
        public void ShootBeam()
        {
            Vector3 target = parent.Map.mapPawns.FreeColonistsSpawned.RandomElement()?.Position.ToVector3Shifted() ?? (parent.Position.ToVector3Shifted() + Vector3.forward);
            Vector3 vector = (target - parent.Position.ToVector3Shifted()).Yto0();
            List<IntVec3> hitPoints = GenSight.BresenhamCellsBetween(parent.Position, parent.Position + vector.ToIntVec3() * 100);
            foreach(IntVec3 hitPoint in hitPoints)
            {
                GenExplosion.DoExplosion(hitPoint, parent.Map, 1, DamageDefOf.Bomb, parent);
            }

        }

        protected override void OnInteracted(Pawn caster)
        {
            ShootBeam();
        }
    }
}
