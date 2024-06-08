using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class Verb_AEPowerBeam : Verb_PowerBeam
    {
        private const int DurationTicks = 1000;
        private float BeamRadius => verbProps.ai_AvoidFriendlyFireRadius;

        private int lastTickBTSearch;

        public Thing BeamTarget
        {
            get
            {
                if (BeamTargetCached == null || !BeamTargetCached.Spawned || BeamTargetCached.Destroyed)
                {
                    BeamTargetCached = Find.AnyPlayerHomeMap.listerBuildings.allBuildingsColonist.FirstOrDefault((Building b) => b.HasComp<Comp_BeamTarget>());
                }
                return BeamTargetCached;
            }
        }
        private Thing BeamTargetCached;

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

        public override bool Available()
        {
            if (AEMod.Settings.PoweBeamRequireBeamTarget && BeamTarget == null)
            {
                if ((Find.TickManager.TicksGame > lastTickBTSearch + 500))
                {
                    Messages.Message("AnomaliesExpected.PowerBeam.BeamTargetMissing".Translate(caster.LabelCap).RawText, caster, MessageTypeDefOf.NeutralEvent, false);
                    lastTickBTSearch = Find.TickManager.TicksGame;
                }
                return false;
            }
            return base.Available();
        }

        public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
        {
            needLOSToCenter = false;
            return BeamRadius;
        }
    }
}
