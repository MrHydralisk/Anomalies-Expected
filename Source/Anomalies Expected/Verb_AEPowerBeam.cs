using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class Verb_AEPowerBeam : Verb_PowerBeam
    {
        private const int DurationTicks = 1000;
        private float BeamRadius => verbProps.ai_AvoidFriendlyFireRadius;

        protected override bool TryCastShot()
        {
            if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
            {
                return false;
            }
            AEPowerBeam obj = (AEPowerBeam)GenSpawn.Spawn(ThingDefOfLocal.AEPowerBeam, currentTarget.Cell, caster.Map);
            obj.duration = DurationTicks;
            obj.instigator = caster;
            obj.Radius = BeamRadius;
            obj.weaponDef = ((base.EquipmentSource != null) ? base.EquipmentSource.def : null);
            obj.StartStrike();
            base.ReloadableCompSource?.UsedOnce();
            return true;
        }

        public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
        {
            needLOSToCenter = false;
            return BeamRadius;
        }
    }
}
