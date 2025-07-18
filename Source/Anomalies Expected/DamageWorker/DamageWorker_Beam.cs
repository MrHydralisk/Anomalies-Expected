using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class DamageWorker_Beam : DamageWorker
    {
        private static List<Thing> thingsToAffect = new List<Thing>();

        public override void ExplosionAffectCell(Explosion explosion, IntVec3 c, List<Thing> damagedThings, List<Thing> ignoredThings, bool canThrowMotes)
        {
            if (explosion.doVisualEffects && (def.explosionCellMote != null || def.explosionCellFleck != null) && canThrowMotes)
            {
                float t = Mathf.Clamp01((explosion.Position - c).LengthHorizontal / explosion.radius);
                Color color = Color.Lerp(def.explosionColorCenter, def.explosionColorEdge, t);
                if (def.explosionCellMote != null)
                {
                    if (c.GetFirstThing(explosion.Map, def.explosionCellMote) is Mote mote)
                    {
                        mote.spawnTick = Find.TickManager.TicksGame;
                    }
                    else
                    {
                        MoteMaker.ThrowExplosionCell(c, explosion.Map, def.explosionCellMote, color);
                    }
                }
                else
                {
                    FleckMaker.ThrowExplosionCell(c, explosion.Map, def.explosionCellFleck, color);
                }
            }
            if (def.explosionCellEffecter != null && (def.explosionCellEffecterMaxRadius < float.Epsilon || c.InHorDistOf(explosion.Position, def.explosionCellEffecterMaxRadius)) && Rand.Chance(def.explosionCellEffecterChance))
            {
                def.explosionCellEffecter.Spawn(explosion.Position, c, explosion.Map);
            }
            thingsToAffect.Clear();
            float num = float.MinValue;
            bool flag = false;
            List<Thing> list = explosion.Map.thingGrid.ThingsListAt(c);
            for (int i = 0; i < list.Count; i++)
            {
                Thing thing = list[i];
                if (thing.def.category != ThingCategory.Mote && thing.def.category != ThingCategory.Ethereal)
                {
                    thingsToAffect.Add(thing);
                    if (thing.def.Fillage == FillCategory.Full && thing.def.Altitude > num)
                    {
                        flag = true;
                        num = thing.def.Altitude;
                    }
                }
            }
            for (int j = 0; j < thingsToAffect.Count; j++)
            {
                if (thingsToAffect[j].def.Altitude >= num)
                {
                    ExplosionDamageThing(explosion, thingsToAffect[j], damagedThings, ignoredThings, c);
                }
            }
            if (!flag)
            {
                ExplosionDamageTerrain(explosion, c);
            }
            if (def.explosionSnowMeltAmount > 0.0001f)
            {
                float lengthHorizontal = (c - explosion.Position).LengthHorizontal;
                float num2 = 1f - lengthHorizontal / explosion.radius;
                if (num2 > 0f)
                {
                    explosion.Map.snowGrid.AddDepth(c, (0f - num2) * def.explosionSnowMeltAmount);
                }
            }
            if (def != DamageDefOf.Bomb && def != DamageDefOf.Flame)
            {
                return;
            }
            List<Thing> list2 = explosion.Map.listerThings.ThingsOfDef(ThingDefOf.RectTrigger);
            for (int k = 0; k < list2.Count; k++)
            {
                RectTrigger rectTrigger = (RectTrigger)list2[k];
                if (rectTrigger.activateOnExplosion && rectTrigger.Rect.Contains(c))
                {
                    rectTrigger.ActivatedBy(null);
                }
            }
        }
    }
}
