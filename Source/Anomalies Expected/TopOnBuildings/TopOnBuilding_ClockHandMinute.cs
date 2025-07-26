using RimWorld;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class TopOnBuilding_ClockHandMinute : TopOnBuilding_Clockwork
    {
        public IntVec3 target;

        public TopOnBuilding_ClockHandMinute(TopOnBuildingStructure TopOnBuildingStructure) : base(TopOnBuildingStructure)
        {
        }

        public override void OnTimerEnd()
        {
            Map map = compObelisk_Clockwork.parent.Map;
            IntVec3 position = compObelisk_Clockwork.parent.Position;
            target = map.mapPawns.FreeColonistsSpawned.RandomElement()?.Position ?? (position + IntVec3.North);
            Vector3 vector = (target.ToVector3Shifted() - position.ToVector3Shifted()).Yto0().normalized;
            CurRotation = vector.ToAngleFlat();
        }

        public override void OnWarmupEnd()
        {
            Map map = compObelisk_Clockwork.parent.Map;
            GenExplosion.DoExplosion(target, map, 15, DamageDefOf.Bomb, compObelisk_Clockwork.parent, damAmount: 1, propagationSpeed: 3);
        }
    }
}
