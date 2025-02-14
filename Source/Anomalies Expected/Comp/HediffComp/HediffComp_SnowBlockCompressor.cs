using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Mathematics;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class HediffComp_SnowBlockCompressor : HediffComp
    {
        public HediffCompProperties_SnowBlockCompressor Props => (HediffCompProperties_SnowBlockCompressor)props;

        private int TickNextExtinguish;
        public Ability ability => abilityCached ?? (abilityCached = parent.AllAbilitiesForReading.FirstOrDefault((Ability a) => a.def == parent.def.abilities.FirstOrDefault()));
        private Ability abilityCached;

        private SnowArmyMapComponent snowArmyMapComponent => snowArmyMmapComponentCached ?? (snowArmyMmapComponentCached = Pawn.Map?.GetComponent<SnowArmyMapComponent>() ?? null);
        private SnowArmyMapComponent snowArmyMmapComponentCached;

        public int canNextCastAbilityTick;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Pawn.IsHashIntervalTick(50) && Find.TickManager.TicksGame >= canNextCastAbilityTick)
            {
                TryCastAbility();
            }
            if (Pawn.IsHashIntervalTick(250))
            {
                TryExtinguish();
                TryPushCold();
            }
        }

        public void TryCastAbility()
        {
            if (ability?.CanCast ?? false)
            {
                List<(Thing, int)> TargetsSelected = new List<(Thing, int)>();
                for(int i = 0; i < snowArmyMapComponent.TargetsForSnowBlock.Count; i++)
                {
                    Thing thing = snowArmyMapComponent.TargetsForSnowBlock[i];
                    if (!thing.Position.InBounds(Pawn.Map) || Pawn.Position.DistanceTo(thing.Position) > ability.verb.verbProps.range || thing.Position.Roofed(Pawn.Map))
                    {
                        continue;
                    }
                    if (thing is Pawn tPawn && !tPawn.DeadOrDowned)
                    {
                        if (!tPawn.health.hediffSet.HasHediff(Props.ignoreWithHediffDef))
                        {
                            TargetsSelected.Add((thing, i));
                        }
                    }
                    else if (thing is Building tBuilding)
                    {
                        TargetsSelected.Add((thing, i));
                    }
                }
                float MaxDistance = float.MaxValue;
                IntVec3 center = IntVec3.Zero;
                if (TargetsSelected.Count > 0)
                {
                    foreach ((Thing thing, int i) in TargetsSelected)
                    {
                        center += thing.Position;
                    }
                    float amount = TargetsSelected.Count;
                    center = new IntVec3(Mathf.RoundToInt(center.x / amount), Mathf.RoundToInt(center.y / amount), Mathf.RoundToInt(center.z / amount));
                    MaxDistance = TargetsSelected.Max((t) => t.Item1.Position.DistanceTo(center));
                }
                while (TargetsSelected.Count > 0 && MaxDistance > Props.ProjectileDef.projectile.explosionRadius)
                {
                    int index = -1;
                    double distanceMax = double.MinValue;
                    for (int i = 0; i < TargetsSelected.Count; i++)
                    {
                        Thing thing = TargetsSelected[i].Item1;
                        double pDistance = thing.Position.DistanceTo(center);
                        if (pDistance > distanceMax)
                        {
                            index = i;
                            distanceMax = pDistance;
                        }
                    }
                    if (index != -1)
                    {
                        TargetsSelected.RemoveAt(index);
                        if (TargetsSelected.Count > 0)
                        {
                            center = IntVec3.Zero;
                            foreach ((Thing thing, int i) in TargetsSelected)
                            {
                                center += thing.Position;
                            }
                            float amount = TargetsSelected.Count;
                            center = new IntVec3(Mathf.RoundToInt(center.x / amount), Mathf.RoundToInt(center.y / amount), Mathf.RoundToInt(center.z / amount));
                            MaxDistance = TargetsSelected.Max((t) => t.Item1.Position.DistanceTo(center));
                        }
                    }
                }
                if (MaxDistance <= Props.ProjectileDef.projectile.explosionRadius)
                {
                    for (int i = TargetsSelected.Count - 1; i >= 0; i--)
                    {
                        if (TargetsSelected[i].Item1.Position.DistanceTo(center) <= Props.ProjectileDef.projectile.explosionRadius)
                        {
                            snowArmyMapComponent.TargetsForSnowBlock.RemoveAt(TargetsSelected[i].Item2);
                        }
                    }
                    ability.QueueCastingJob(new LocalTargetInfo(center), null);
                    canNextCastAbilityTick = Find.TickManager.TicksGame + ability.def.cooldownTicksRange.min;
                }
            }
        }

        public void TryExtinguish()
        {
            if (Find.TickManager.TicksGame < TickNextExtinguish)
            {
                return;
            }
            bool succeed = false;
            List<IntVec3> cells = GenRadial.RadialCellsAround(Pawn.Position, Props.radius, true).ToList();
            List<Thing> things = cells.SelectMany((IntVec3 iv3) => Pawn.Map.thingGrid.ThingsListAtFast(iv3)).ToList();
            foreach (Thing thing in things)
            {
                if (thing is Fire fire && !fire.DestroyedOrNull())
                {
                    fire.Destroy();
                    succeed = true;
                }
            }
            if (succeed)
            {
                TickNextExtinguish = Find.TickManager.TicksGame + Props.ticksBetweenExtinguish;
                SoundInfo soundInfo = new TargetInfo(Pawn.Position, Pawn.Map);
                soundInfo.volumeFactor *= 0.2f;
                soundInfo.pitchFactor *= 0.5f;
                Props.soundDef.PlayOneShot(soundInfo);
                foreach (IntVec3 cell in cells)
                {
                    FleckMaker.ThrowExplosionCell(cell, Pawn.Map, Props.fleckDef, Props.color);
                }
            }
        }

        public void TryPushCold()
        {
            if (Pawn.Spawned && Pawn.AmbientTemperature > -273f)
            {
                GenTemperature.PushHeat(Pawn, Pawn.BodySize * Props.heatPushedPerBodySize);
            }
        }

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);
            if (dinfo.Def != DamageDefOf.Burn && Pawn.IsBurning())
            {
                dinfo.SetAmount(2f * dinfo.Amount);
            }
        }

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref canNextCastAbilityTick, "canNextCastAbilityTick", -1);
        }
    }
}
