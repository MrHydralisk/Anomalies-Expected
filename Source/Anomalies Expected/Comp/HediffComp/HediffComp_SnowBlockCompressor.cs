using RimWorld;
using System.Collections.Generic;
using System.Linq;
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
            }
        }

        public void TryCastAbility()
        {
            if (Pawn.Spawned && (ability?.CanCast ?? false))
            {
                List<(Thing, int)> TargetsSelected = new List<(Thing, int)>();
                for (int i = 0; i < snowArmyMapComponent.TargetsForSnowBlock.Count; i++)
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
                bool flag = TargetsSelected.NullOrEmpty() && !snowArmyMapComponent.TargetsForSnowBlockAll.NullOrEmpty();
                int loopInd = 0;
                while (TargetsSelected.Count > 0 && (MaxDistance > Props.ProjectileDef.projectile.explosionRadius || (center.GetRoof(Pawn.Map)?.isThickRoof ?? false)) && loopInd < 5000)
                {
                    loopInd++;
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
                if (loopInd >= 5000)
                {
                    Log.Warning($"Snow Golem TryCastAbility {loopInd}/5000 tries.");
                    flag = true;
                }
                if (flag)
                {
                    //Log.Warning($"Snow Golem TryCastAbility trying to find random target.");
                    center = snowArmyMapComponent.TargetsForSnowBlockAll?.Where((Thing t) => (!(t is Pawn p) || !p.DeadOrDowned) && !(t.Position.GetRoof(Pawn.Map)?.isThickRoof ?? false))?.RandomElement()?.Position ?? IntVec3.Invalid;
                    if (center != IntVec3.Invalid)
                    {
                        MaxDistance = 0;
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
            if (Find.TickManager.TicksGame < TickNextExtinguish || !Pawn.Spawned)
            {
                return;
            }
            bool succeed = false;
            List<IntVec3> cells = GenRadial.RadialCellsAround(Pawn.Position, Props.radius, true).ToList();
            List<IntVec3> cellsAffected = new List<IntVec3>();
            foreach (IntVec3 cell in cells)
            {
                if (cell.InBounds(Pawn.Map) && GenSight.LineOfSight(Pawn.Position, cell, Pawn.Map, skipFirstCell: true))
                {
                    cellsAffected.Add(cell);
                    List<Thing> things = Pawn.Map.thingGrid.ThingsListAtFast(cell);
                    for (int i = things.Count - 1; i >= 0; i--)
                    {
                        if (things[i] is Fire fire && !fire.DestroyedOrNull())
                        {
                            fire.Destroy();
                            succeed = true;
                        }
                    }
                }
            }
            if (succeed)
            {
                TickNextExtinguish = Find.TickManager.TicksGame + Props.ticksBetweenExtinguish;
                SoundInfo soundInfo = new TargetInfo(Pawn.Position, Pawn.Map);
                soundInfo.volumeFactor *= 0.2f;
                soundInfo.pitchFactor *= 0.5f;
                Props.soundDef.PlayOneShot(soundInfo);
                foreach (IntVec3 cell in cellsAffected)
                {
                    FleckMaker.ThrowExplosionCell(cell, Pawn.Map, Props.fleckDef, Props.color);
                }
            }
        }

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref canNextCastAbilityTick, "canNextCastAbilityTick", -1);
        }
    }
}
