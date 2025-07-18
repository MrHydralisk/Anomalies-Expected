using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class CompObelisk_Clockwork : CompInteractable
    {
        public int radius = 3;

        public void ShootBeam()
        {
            Map map = parent.Map;
            Vector3 target = map.mapPawns.FreeColonistsSpawned.RandomElement()?.Position.ToVector3Shifted() ?? (parent.Position.ToVector3Shifted() + Vector3.forward);
            Vector3 vector = (target - parent.Position.ToVector3Shifted()).Yto0();
            List<IntVec3> beamPoints = GenSight.BresenhamCellsBetween(parent.Position, parent.Position + vector.ToIntVec3() * 100);
            int pointsToCompare = radius * radius * 4;
            List<IntVec3> hitPoints = new List<IntVec3>();
            foreach(IntVec3 beamPoint in beamPoints)
            {
                if (!beamPoint.InBounds(map))
                {
                    break;
                }
                List<IntVec3> radialPoints = GenRadial.RadialCellsAround(beamPoint, radius, true).Where((IntVec3 iv) => iv.InBounds(map)).ToList();
                List<IntVec3> hitPointsLast = hitPoints.Skip(Math.Max(hitPoints.Count() - pointsToCompare, 0)).ToList();
                radialPoints = radialPoints.Except(hitPointsLast).ToList();
                hitPoints.AddRange(radialPoints);
            }
            List<Thing> damagedThings = new List<Thing>();
            foreach (IntVec3 hitPoint in hitPoints)
            {
                AffectCell(hitPoint, map, DamageDefOf.Bomb, damagedThings);
            }

        }

        public void AffectCell(IntVec3 c, Map map, DamageDef damageDef, List<Thing> damagedThings)
        {
            //if (explosion.doVisualEffects && (def.explosionCellMote != null || def.explosionCellFleck != null) && canThrowMotes)
            //{
            //    float t = Mathf.Clamp01((explosion.Position - c).LengthHorizontal / explosion.radius);
            //    Color color = Color.Lerp(def.explosionColorCenter, def.explosionColorEdge, t);
            //    if (def.explosionCellMote != null)
            //    {
            //        if (c.GetFirstThing(explosion.Map, def.explosionCellMote) is Mote mote)
            //        {
            //            mote.spawnTick = Find.TickManager.TicksGame;
            //        }
            //        else
            //        {
            //            MoteMaker.ThrowExplosionCell(c, explosion.Map, def.explosionCellMote, color);
            //        }
            //    }
            //    else
            //    {
            //        FleckMaker.ThrowExplosionCell(c, explosion.Map, def.explosionCellFleck, color);
            //    }
            //}
            //if (def.explosionCellEffecter != null && (def.explosionCellEffecterMaxRadius < float.Epsilon || c.InHorDistOf(explosion.Position, def.explosionCellEffecterMaxRadius)) && Rand.Chance(def.explosionCellEffecterChance))
            //{
            //    def.explosionCellEffecter.Spawn(explosion.Position, c, explosion.Map);
            //}
            //thingsToAffect.Clear();
            //float num = float.MinValue;
            //bool flag = false;
            //List<Thing> thingsToAffect = new List<Thing>();
            //List<Thing> list = map.thingGrid.ThingsListAt(c);
            //for (int i = 0; i < list.Count; i++)
            //{
            //    Thing thing = list[i];
            //    if (thing.def.category != ThingCategory.Mote && thing.def.category != ThingCategory.Ethereal)
            //    {
            //        thingsToAffect.Add(thing);
            //        //if (thing.def.Fillage == FillCategory.Full && thing.def.Altitude > num)
            //        //{
            //        //    flag = true;
            //        //    num = thing.def.Altitude;
            //        //}
            //    }
            //}
            //for (int j = 0; j < thingsToAffect.Count; j++)
            //{
            //    if (thingsToAffect[j].def.Altitude >= num)
            //    {
            //        if (t.def.category == ThingCategory.Mote || t.def.category == ThingCategory.Ethereal || damagedThings.Contains(t))
            //        {
            //            return;
            //        }
            //        damagedThings.Add(t);
            //    }
            //}
            //if (!flag)
            //{
            //    ExplosionDamageTerrain(explosion, c);
            //}
            //if (def.explosionSnowMeltAmount > 0.0001f)
            //{
            //    float lengthHorizontal = (c - explosion.Position).LengthHorizontal;
            //    float num2 = 1f - lengthHorizontal / explosion.radius;
            //    if (num2 > 0f)
            //    {
            //        explosion.Map.snowGrid.AddDepth(c, (0f - num2) * def.explosionSnowMeltAmount);
            //    }
            //}
            //if (def != DamageDefOf.Bomb && def != DamageDefOf.Flame)
            //{
            //    return;
            //}
            //List<Thing> list2 = explosion.Map.listerThings.ThingsOfDef(ThingDefOf.RectTrigger);
            //for (int k = 0; k < list2.Count; k++)
            //{
            //    RectTrigger rectTrigger = (RectTrigger)list2[k];
            //    if (rectTrigger.activateOnExplosion && rectTrigger.Rect.Contains(c))
            //    {
            //        rectTrigger.ActivatedBy(null);
            //    }
            //}
        }

        protected override void OnInteracted(Pawn caster)
        {
            ShootBeam();
        }
    }
}
