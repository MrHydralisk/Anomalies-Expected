using RimWorld;
using RimWorld.Planet;
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
        private IntRange ticksPerSummonRange = new IntRange(10000, 30000);
        private int TickNextSummon;
        private int TickNextBloodFog;

        //private int initialEntityAmount;

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
            //initialEntityAmount = map.mapPawns.AllPawnsSpawned.Count((Pawn p) => p.Faction.def == FactionDefOf.Entities);
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
                        TickNextSummon = Find.TickManager.TicksGame + ticksPerSummonRange.RandomInRange;
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

        public void TrySpawnWaveFromUndergroundNest(List<Pawn> colonists)
        {
            IntVec3 pos = IntVec3.Invalid;
            IntVec3 total = IntVec3.Zero;
            float count = colonists.Count();
            if (count > 0)
            {
                foreach (Pawn p in colonists)
                {
                    total += p.Position;
                }
                pos = new IntVec3(Mathf.RoundToInt(total.x / count), Mathf.RoundToInt(total.y / count), Mathf.RoundToInt(total.z / count));
                if (pos == IntVec3.Invalid)
                {
                    pos = map.Center;
                }
                Thing UndergroundNest = UndergroundNests.Where((Thing t) => t.Position.DistanceTo(pos) > 15).OrderBy((Thing t) => t.Position.DistanceTo(pos)).FirstOrDefault();
                ThingDef thingDef = ThingDefOfLocal.AE_BloodLakeUndergroundNest;
                List<Pawn> emergingFleshbeasts = FleshbeastUtility.GetFleshbeastsForPoints(StorytellerUtility.DefaultThreatPointsNow(map) * Mathf.Max(1, (1 + map.gameConditionManager.ActiveConditions.Count())) * AEMod.Settings.UndergroundFleshmassNestMult, map);
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
            PocketMapUtility.DestroyPocketMap(map);
            if (!lostConnection)
            {
                Messages.Message("AnomaliesExpected.BloodLake.ReactorMeltdownExplosion".Translate().RawText, Entrance, MessageTypeDefOf.NegativeEvent);
                Entrance.isDestroyedMap = true;
                Comp_CanDestroyedAfterStudy canDestroyedAfterStudy = Entrance.GetComp<Comp_CanDestroyedAfterStudy>();
                Log.Message($"canDestroyedAfterStudy == null {canDestroyedAfterStudy == null}");
                if (canDestroyedAfterStudy != null)
                {
                    canDestroyedAfterStudy.isCanDestroyForced = true;
                    Log.Message($"canDestroyedAfterStudy.isCanDestroyForced {canDestroyedAfterStudy.isCanDestroyForced}");
                }
                Find.CameraDriver.shaker.DoShake(0.2f);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref TickNextSummon, "TickNextSummon");
            Scribe_Values.Look(ref TickNextBloodFog, "TickNextBloodFog");
            //Scribe_Values.Look(ref initialEntityAmount, "initialEntityAmount");
            Scribe_References.Look(ref Entrance, "Entrance");
            Scribe_References.Look(ref Exit, "Exit");
            Scribe_References.Look(ref Terminal, "Terminal");
            Scribe_Collections.Look(ref UndergroundNests, "UndergroundNests", LookMode.Reference);
        }
    }
}
