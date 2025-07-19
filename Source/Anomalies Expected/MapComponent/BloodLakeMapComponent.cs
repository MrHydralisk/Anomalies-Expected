using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class BloodLakeMapComponent : CustomMapComponent
    {
        public Building_AEBloodLake Entrance;

        public Building_AEBloodLakeExit Exit;

        public Building_AE Terminal;

        public List<Thing> UndergroundNests = new List<Thing>();

        private IntRange durationBloodFog = new IntRange(40000, 80000);
        private IntRange ticksPerSummonRange = new IntRange(20000, 40000);
        private int TickNextSummon;
        private int TickNextBloodFog;

        private Dictionary<PawnKindDef, int> PawnDefConvertor = new Dictionary<PawnKindDef, int>()
        {
            [PawnKindDefOf.Fingerspike] = 1,
            [PawnKindDefOf.Trispike] = 3,
            [PawnKindDefOf.Toughspike] = 4,
            [PawnKindDefOf.Bulbfreak] = 16,
        };

        private int initialEntityAmount;

        public Map SourceMap => (map.Parent as PocketMapParent)?.sourceMap;

        public BloodLakeMapComponent(Map map) : base(map)
        {
        }

        public override void MapGenerated()
        {
            Entrance = SourceMap?.listerThings?.ThingsOfDef(ThingDefOfLocal.AE_BloodLake).FirstOrDefault() as Building_AEBloodLake;
            Exit = map.listerThings.ThingsOfDef(ThingDefOfLocal.AE_BloodLakeExit).FirstOrDefault() as Building_AEBloodLakeExit;
            Terminal = map.listerThings.ThingsOfDef(ThingDefOfLocal.AE_BloodLakeTerminal).FirstOrDefault() as Building_AE;
            UndergroundNests = map.listerThings.ThingsOfDef(ThingDefOfLocal.AE_BloodLakeUndergroundNest);
            initialEntityAmount = EntityAmount(map.mapPawns.AllPawnsSpawned.Where((Pawn p) => p.Faction?.def == FactionDefOf.Entities));
            if (Entrance == null)
            {
                Log.Warning("BloodLake not found");
                return;
            }
            if (Exit == null)
            {
                Log.Error("BloodLakeExit not found");
                return;
            }
            if (Terminal == null)
            {
                Log.Error("BloodLakeTerminal not found");
                return;
            }
            if (UndergroundNests.NullOrEmpty())
            {
                Log.Error("Any BloodLakeUndergroundNest not found");
                return;
            }
            TickNextBloodFog = Find.TickManager.TicksGame + durationBloodFog.RandomInRange;
            TickNextSummon = Find.TickManager.TicksGame + (int)ticksPerSummonRange.Average * 2;
        }

        public int EntityAmount(IEnumerable<Pawn> entity)
        {
            int amount = 0;
            foreach (Pawn p in entity)
            {
                if (PawnDefConvertor.TryGetValue(p.kindDef, out int pawnValue))
                {
                    amount += pawnValue;
                }
            }
            return amount;
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (Find.TickManager.TicksGame % 2500 == 0)
            {
                if (Find.TickManager.TicksGame > TickNextSummon)
                {
                    List<Pawn> colonists = map.mapPawns.FreeColonistsAndPrisonersSpawned;
                    if (colonists.Count() > 0)
                    {
                        TrySpawnWaveFromUndergroundNest(colonists);
                    }
                    else
                    {
                        TrySpawnWaveFromUndergroundNest();
                    }
                }
                if (Find.TickManager.TicksGame > TickNextBloodFog)
                {
                    int duration = durationBloodFog.RandomInRange;
                    InitiateBloodFog(duration);
                    TickNextBloodFog = Find.TickManager.TicksGame + duration + durationBloodFog.RandomInRange;
                }
            }
        }

        public void TrySpawnWaveFromUndergroundNest(List<Pawn> colonists = null)
        {
            Thing UndergroundNest;
            int currentEntityAmount = EntityAmount(map.mapPawns.AllPawnsSpawned.Where((Pawn p) => p.Faction?.def == FactionDefOf.Entities));
            List<Pawn> emergingFleshbeasts = new List<Pawn>();
            float count;
            if (colonists != null && (count = colonists.Count()) > 0)
            {
                IntVec3 pos = IntVec3.Invalid;
                IntVec3 total = IntVec3.Zero;
                foreach (Pawn p in colonists)
                {
                    total += p.Position;
                }
                pos = new IntVec3(Mathf.RoundToInt(total.x / count), Mathf.RoundToInt(total.y / count), Mathf.RoundToInt(total.z / count));
                if (pos == IntVec3.Invalid)
                {
                    pos = map.Center;
                }
                UndergroundNest = UndergroundNests.OrderBy((Thing t) => t.Position.DistanceTo(pos)).FirstOrDefault();
                float points = Mathf.Max(StorytellerUtility.DefaultThreatPointsNow(map), 500) * Mathf.Max(1, (1 + map.gameConditionManager.ActiveConditions.Count())) * AEMod.Settings.UndergroundFleshmassNestMult * (colonists.NullOrEmpty() ? AEMod.Settings.UndergroundFleshmassNestRestore : 1);
                emergingFleshbeasts = FleshbeastUtility.GetFleshbeastsForPoints(points, map);
                int EntityToRemove = (currentEntityAmount + EntityAmount(emergingFleshbeasts)) - initialEntityAmount;
                if (AEMod.Settings.DevModeInfo)
                {
                    Log.Message($"Attack {points} = ({StorytellerUtility.DefaultThreatPointsNow(map)}, 500) * {Mathf.Max(1, (1 + map.gameConditionManager.ActiveConditions.Count()))} * {AEMod.Settings.UndergroundFleshmassNestMult} * {(colonists.NullOrEmpty() ? AEMod.Settings.UndergroundFleshmassNestRestore : 1)}\n{EntityToRemove} = ({currentEntityAmount} + {EntityAmount(emergingFleshbeasts)}) - {initialEntityAmount}:\n{string.Join("\n", emergingFleshbeasts.Select(e => e.LabelCap))}");
                }
                while (EntityToRemove > 0 && emergingFleshbeasts.Count() > 0)
                {
                    Pawn p = emergingFleshbeasts.RandomElement();
                    if (PawnDefConvertor.TryGetValue(p.kindDef, out int pawnValue))
                    {
                        EntityToRemove -= pawnValue;
                    }
                    else
                    {
                        EntityToRemove -= 1;
                    }
                    emergingFleshbeasts.Remove(p);
                }
                if (AEMod.Settings.DevModeInfo)
                {
                    Log.Message($"Attack {string.Join("\n", emergingFleshbeasts.Select(e => e.LabelCap))}");
                }

            }
            else
            {
                UndergroundNest = UndergroundNests.OrderBy((Thing t) => t.Position.DistanceTo(Entrance.Position)).FirstOrDefault();
                int EntityToRestore = Math.Min(initialEntityAmount - currentEntityAmount, Mathf.CeilToInt(initialEntityAmount * AEMod.Settings.UndergroundFleshmassNestRestore));
                if (AEMod.Settings.DevModeInfo)
                {
                    Log.Message($"Restore {EntityToRestore} = {initialEntityAmount} - {currentEntityAmount}, {initialEntityAmount} * {AEMod.Settings.UndergroundFleshmassNestRestore}");
                }
                while (EntityToRestore > 0)
                {
                    var entity = PawnDefConvertor.Where(e => e.Value <= EntityToRestore).RandomElement();
                    Pawn p = PawnGenerator.GeneratePawn(entity.Key, Faction.OfEntities);
                    emergingFleshbeasts.Add(p);
                    EntityToRestore -= entity.Value;
                }
                if (AEMod.Settings.DevModeInfo)
                {
                    Log.Message($"Restore {string.Join("\n", emergingFleshbeasts.Select(e => e.LabelCap))}");
                }
            }
            ThingDef thingDef = ThingDefOfLocal.AE_BloodLakeUndergroundNest;
            if (emergingFleshbeasts.Count() > 0)
            {
                CellRect cellRect = GenAdj.OccupiedRect(UndergroundNest.Position, Rot4.North, thingDef.Size);
                List<PawnFlyer> list = new List<PawnFlyer>();
                List<IntVec3> list2 = new List<IntVec3>();
                foreach (Pawn emergingFleshbeast in emergingFleshbeasts)
                {
                    IntVec3 randomCell = cellRect.RandomCell;
                    GenSpawn.Spawn(emergingFleshbeast, randomCell, map);
                    if (CellFinder.TryFindRandomCellNear(UndergroundNest.Position, map, Mathf.CeilToInt(thingDef.size.x / 2), (IntVec3 c) => !c.Fogged(map) && c.Walkable(map) && !c.Impassable(map), out var result))
                    {
                        emergingFleshbeast.rotationTracker.FaceCell(result);
                        list.Add(PawnFlyer.MakeFlyer(ThingDefOf.PawnFlyer_Stun, emergingFleshbeast, result, null, null, flyWithCarriedThing: false));
                        list2.Add(randomCell);
                    }
                }
                if (list2.Count != 0)
                {
                    SpawnRequest spawnRequest = new SpawnRequest(list.Cast<Thing>().ToList(), list2, 1, 1f);
                    spawnRequest.initialDelay = 250;
                    map.deferredSpawner.AddRequest(spawnRequest);
                    SoundDefOf.Pawn_Fleshbeast_EmergeFromPitGate.PlayOneShot(UndergroundNest);
                    emergingFleshbeasts.Clear();
                }
                Messages.Message("AnomaliesExpected.BloodLake.UndergroundNestSpawn".Translate().RawText, UndergroundNest, MessageTypeDefOf.NegativeEvent);
                TickNextSummon = Find.TickManager.TicksGame + Mathf.RoundToInt(ticksPerSummonRange.RandomInRange * AEMod.Settings.UndergroundFleshmassNestFrequencyMult);
            }
        }

        public void InitiateBloodFog(int duration)
        {
            GameCondition gameCondition = GameConditionMaker.MakeCondition(GameConditionDefOfLocal.AE_BloodFog, duration);
            map.gameConditionManager.RegisterCondition(gameCondition);
        }

        public void DestroySubMap(bool lostConnection = false)
        {
            DamageInfo damageInfo = new DamageInfo(DamageDefOf.Bomb, 99999f, 999f);
            for (int num = map.mapPawns.AllPawns.Count - 1; num >= 0; num--)
            {
                Pawn pawn = map.mapPawns.AllPawns[num];
                if (lostConnection)
                {
                    pawn.Kill(null);
                }
                else
                {
                    pawn.TakeDamage(damageInfo);
                    if (!pawn.Dead)
                    {
                        pawn.Kill(damageInfo);
                    }
                }
            }
            if (Entrance.LoadInProgress)
            {
                Entrance.CancelLoad();
            }
            PocketMapUtility.DestroyPocketMap(map);
            if (!lostConnection)
            {
                Messages.Message("AnomaliesExpected.BloodLake.ReactorMeltdownExplosion".Translate().RawText, Entrance, MessageTypeDefOf.NegativeEvent);
                Entrance.isDestroyedMap = true;
                Comp_CanDestroyedAfterStudy canDestroyedAfterStudy = Entrance.GetComp<Comp_CanDestroyedAfterStudy>();
                if (canDestroyedAfterStudy != null)
                {
                    canDestroyedAfterStudy.isCanDestroyForced = true;
                }
                Find.CameraDriver.shaker.DoShake(0.2f);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref TickNextSummon, "TickNextSummon");
            Scribe_Values.Look(ref TickNextBloodFog, "TickNextBloodFog");
            Scribe_Values.Look(ref initialEntityAmount, "initialEntityAmount");
            Scribe_References.Look(ref Entrance, "Entrance");
            Scribe_References.Look(ref Exit, "Exit");
            Scribe_References.Look(ref Terminal, "Terminal");
            Scribe_Collections.Look(ref UndergroundNests, "UndergroundNests", LookMode.Reference);
        }
    }
}
