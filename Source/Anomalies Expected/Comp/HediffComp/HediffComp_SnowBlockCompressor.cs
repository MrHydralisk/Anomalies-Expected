using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Unity.Mathematics;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Noise;
using Verse.Sound;
using static UnityEngine.GraphicsBuffer;

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
            Log.Message($"TryCastAbility A {ability != null} | {ability?.CanCast.ToString() ?? "---"} | {parent?.def?.abilities?.FirstOrDefault()?.label ?? "---"} | {parent.AllAbilitiesForReading.FirstOrDefault(a => a.def == parent.def.abilities.FirstOrDefault())?.ToString() ?? "---"}\n{string.Join("\n", parent.AllAbilitiesForReading.Select(t => t.def.label))}");
            if (ability?.CanCast ?? false)
            {
                List<Thing> TargetsSelected = new List<Thing>();
                int2x2 rect = new int2x2(Pawn.Position.x - (int)ability.verb.verbProps.range, Pawn.Position.x + (int)ability.verb.verbProps.range, Pawn.Position.z - (int)ability.verb.verbProps.range, Pawn.Position.z + (int)ability.verb.verbProps.range);
                Log.Message($"rect ({rect.c0.x}, {rect.c1.x})({rect.c0.y}, {rect.c1.y})");
                HashSet<Thing> returnedThings = null;
                for (int i = rect.c0.x; i < rect.c1.x; i++)
                {
                    for (int j = rect.c0.y; j < rect.c1.y; j++)
                    {
                        IntVec3 c = new IntVec3(i, 0, j);
                        if (!c.InBounds(Pawn.Map) || Pawn.Position.DistanceTo(c) > ability.verb.verbProps.range)
                        {
                            continue;
                        }
                        List<Thing> thingList = c.GetThingList(Pawn.Map);
                        foreach (Thing thing in thingList)
                        {
                            if (thing.def.size.x > 1 || thing.def.size.z > 1)
                            {
                                if (returnedThings == null)
                                {
                                    returnedThings = new HashSet<Thing>();
                                }
                                if (returnedThings.Contains(thing))
                                {
                                    continue;
                                }
                                returnedThings.Add(thing);
                            }
                            if (!thing.Fogged() && thing.Faction.HostileTo(Pawn.Faction))
                            {
                                if (thing is Pawn tPawn && !tPawn.DeadOrDowned)
                                {
                                    if (!tPawn.health.hediffSet.HasHediff(Props.ignoreWithHediffDef))
                                    {
                                        TargetsSelected.Add(thing);
                                    }
                                }
                                else if (thing is Building tBuilding && tBuilding.def.building.ai_combatDangerous)
                                {
                                    CompStunnable compStunnable = tBuilding.GetComp<CompStunnable>();
                                    if (compStunnable != null && compStunnable.CanBeStunnedByDamage(Props.ProjectileDef.projectile.damageDef) && !compStunnable.StunHandler.Stunned)
                                    {
                                        TargetsSelected.Add(thing);
                                    }
                                }
                            }
                        }
                    }
                }

                Log.Message($"TryCastAbility B {TargetsSelected.Count()}\n{string.Join("\n", TargetsSelected.Select(t => $"{t.Label} = {t.Position} = {t.DestroyedOrNull()}"))}");
                float MaxDistance = float.MaxValue;
                IntVec3 center = IntVec3.Zero;
                if (TargetsSelected.Count > 0)
                {
                    foreach (Thing thing in TargetsSelected)
                    {
                        center += thing.Position;
                    }
                    float amount = TargetsSelected.Count;
                    center = new IntVec3(Mathf.RoundToInt(center.x / amount), Mathf.RoundToInt(center.y / amount), Mathf.RoundToInt(center.z / amount));
                    MaxDistance = TargetsSelected.Max((Thing t) => t.Position.DistanceTo(center));
                }
                while (TargetsSelected.Count > 0 && MaxDistance > Props.ProjectileDef.projectile.explosionRadius)
                {
                    int index = -1;
                    double distanceMax = double.MinValue;
                    for (int i = 0; i < TargetsSelected.Count; i++)
                    {
                        Thing thing = TargetsSelected[i];
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
                            foreach (Thing thing in TargetsSelected)
                            {
                                center += thing.Position;
                            }
                            float amount = TargetsSelected.Count;
                            center = new IntVec3(Mathf.RoundToInt(center.x / amount), Mathf.RoundToInt(center.y / amount), Mathf.RoundToInt(center.z / amount));
                            Log.Message($"center {center} => pawn {Pawn.Position}");
                            MaxDistance = TargetsSelected.Max((Thing t) => t.Position.DistanceTo(center));
                        }
                    }
                }
                Log.Message($"TryCastAbility C {MaxDistance} <= {Props.ProjectileDef.projectile.explosionRadius}");
                if (MaxDistance <= Props.ProjectileDef.projectile.explosionRadius)
                {
                    //List<Target> AOETarget = Targets.Where((Target p) => (float)Math.Round(Math.Sqrt(Math.Pow((p.X - center.X), 2) + Math.Pow((p.Y - center.Y), 2))) <= AOEradius).ToList();
                    //int UnderEffectTargets = Targets.Count((Target p) => p.isUnderEffect);
                    //int AOEUnderEffectTargets = AOETarget.Count((Target p) => p.isUnderEffect);
                    //foreach (Target t in AOETarget)
                    //{
                    //    t.isUnderEffect = true;
                    //}
                    //MessageBox.Show($"Found center with casualties {AOETarget.Count} - {AOEUnderEffectTargets}/{Targets.Count} - {UnderEffectTargets}");
                    //textBox4.Text += $"\r\n{AOETarget.Count} - {AOEUnderEffectTargets}/{Targets.Count} - {UnderEffectTargets}";
                    GlobalTargetInfo globalTargetInfo = new GlobalTargetInfo(center, Pawn.Map);

                    //Job job = JobMaker.MakeJob(ability.def.jobDef ?? JobDefOf.CastAbilityOnWorldTile);
                    //job.verbToUse = ability.verb;
                    //job.globalTarget = globalTargetInfo;
                    //job.ability = ability;
                    //pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);

                    ability.QueueCastingJob(new LocalTargetInfo(center), null);
                    canNextCastAbilityTick = Find.TickManager.TicksGame + ability.def.cooldownTicksRange.min;

                    //ability.Activate(globalTargetInfo);
                    Log.Message($"TryCastAbility D {center}");
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
