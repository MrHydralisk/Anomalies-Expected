using RimWorld;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class TopOnBuilding_ClockHandMinute : TopOnBuilding_Clockwork
    {
        public IntVec3 target;

        public TopOnBuilding_ClockHandMinute() : base()
        {
        }

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
            base.OnTimerEnd();
        }

        public override void OnWarmupEnd()
        {
            Map map = compObelisk_Clockwork.parent.Map;
            Thing zone = ThingMaker.MakeThing(compObelisk_Clockwork.Props.MinuteHandZoneDef);
            GenSpawn.Spawn(zone, target, compObelisk_Clockwork.parent.Map);
            Comp_ZoneOfEffect zoneComp = zone.TryGetComp<Comp_ZoneOfEffect>();
            if (zoneComp != null)
            {
                zoneComp.StartAttack(compObelisk_Clockwork.parent, compObelisk_Clockwork.parent.def);
                zoneComp.damageMult = compObelisk_Clockwork.Props.DamageMultActive;
            }
            base.OnWarmupEnd();
        }

        public override void OnAwaken()
        {
            Messages.Message("Minute", new TargetInfo(Obelisk_Clockwork.Position, Obelisk_Clockwork.Map), MessageTypeDefOf.NeutralEvent);
            base.OnAwaken();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref target, "target");
        }
    }
}
