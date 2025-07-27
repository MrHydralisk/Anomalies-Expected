using RimWorld;
using Verse;

namespace AnomaliesExpected
{
    public class CompProperties_ZoneOfEffect : CompProperties
    {
        public ThingDef mote = ThingDefOf.Mote_PowerBeam;

        public DamageDef damageDef = DamageDefOf.Flame;

        public SoundDef sound;

        public float radius = 5f;

        public int ticksPerDamage = 50;

        public int DamageAmount = 1;
        public int DamageAmountDowned = 1;

        public CompProperties_ZoneOfEffect()
        {
            compClass = typeof(Comp_ZoneOfEffect);
        }
    }
}
