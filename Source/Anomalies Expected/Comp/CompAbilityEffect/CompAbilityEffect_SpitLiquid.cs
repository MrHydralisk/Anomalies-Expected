using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AnomaliesExpected
{
    public class CompAbilityEffect_SpitLiquid : CompAbilityEffect
    {
        public new CompProperties_AbilitySpitLiquid Props => (CompProperties_AbilitySpitLiquid)props;

        private Pawn Pawn => parent.pawn;
        public override bool CanCast => base.CanCast && !(Pawn.Position.GetRoof(Pawn.Map)?.isThickRoof ?? false);

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            RoofDef roofDef = Pawn.Position.GetRoof(Pawn.Map);
            if (!(Pawn.Position.GetRoof(Pawn.Map)?.isThickRoof ?? true))
            {
                RoofCollapserImmediate.DropRoofInCells(Pawn.Position, Pawn.Map);
            }
            ((Projectile)GenSpawn.Spawn(Props.projectileDef, Pawn.Position, Pawn.Map)).Launch(Pawn, Pawn.DrawPos, target, target, ProjectileHitFlags.IntendedTarget);
            if (Props.sprayEffecter != null)
            {
                Props.sprayEffecter.Spawn(parent.pawn.Position, target.Cell, parent.pawn.Map).Cleanup();
            }
            base.Apply(target, dest);
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            GenDraw.DrawRadiusRing(target.Cell, Props.projectileDef.projectile.explosionRadius);
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            if (Props.isAICanTargetAlways)
            {
                return true;
            }
            if (Pawn.Faction != null && !target.Cell.InBounds(Pawn.Map) && !parent.verb.TryFindShootLineFromTo(Pawn.Position, target, out var _, ignoreRange: true))
            {
                List<Thing> thingList = target.Cell.GetThingList(Pawn.Map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    if (thingList[i].Faction == Pawn.Faction)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
