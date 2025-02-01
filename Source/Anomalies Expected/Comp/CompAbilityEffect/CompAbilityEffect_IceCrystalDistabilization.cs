using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace AnomaliesExpected
{
    public class CompAbilityEffect_IceCrystalDistabilization : CompAbilityEffect
    {
        public new CompProperties_AbilityIceCrystalDistabilization Props => (CompProperties_AbilityIceCrystalDistabilization)props;

        private Pawn Pawn => parent.pawn;

        public override bool CanCast => base.CanCast && !Pawn.IsBurning();

        public override void Apply(GlobalTargetInfo target)
        {
            base.Apply(target);
            Activate(target);
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Activate(target.ToGlobalTargetInfo(Pawn.Map));
        }

        public void Activate(GlobalTargetInfo target)
        {
            GenExplosion.DoExplosion(target.Cell, target.Map, parent.def.verbProperties.range, Props.damageDef, Pawn, damAmount: Props.damAmount, armorPenetration: Props.armorPenetration);
            Pawn.Kill(null);
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            Log.Message($"DrawEffectPreview");
            GenDraw.DrawRadiusRing(target.Cell, parent.def.verbProperties.range);
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            int targetsCount = 0;
            int targetsWEffectCount = 0;
            if (Pawn.Faction != null)
            {
                foreach (IntVec3 pos in GenRadial.RadialCellsAround(target.Cell, parent.def.verbProperties.range, true))
                {
                    if (GenGrid.InBounds(pos, Pawn.Map) && parent.verb.TryFindShootLineFromTo(target.Cell, pos, out var _, ignoreRange: true))
                    {
                        List<Thing> thingList = pos.GetThingList(Pawn.Map);
                        for (int i = 0; i < thingList.Count; i++)
                        {
                            if (thingList[i].Faction.HostileTo(Pawn.Faction))
                            {
                                if (thingList[i] is Pawn tPawn && !tPawn.DeadOrDowned)
                                {
                                    if (!tPawn.health.hediffSet.HasHediff(Props.ignoreWithHediffDef))
                                    {
                                        targetsCount++;
                                    }
                                    targetsWEffectCount++;
                                }
                                else if (thingList[i] is Building tBuilding && tBuilding.def.building.ai_combatDangerous)
                                {
                                    targetsCount++;
                                    targetsWEffectCount++;
                                }
                            }
                        }
                    }
                }
            }
            return targetsWEffectCount >= Props.minTargetsWEffect && targetsCount >= Props.minTargets;
        }
    }
}
