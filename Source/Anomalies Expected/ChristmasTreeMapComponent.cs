using RimWorld;
using RimWorld.Planet;
using System.Linq;
using Verse;

namespace AnomaliesExpected
{
    public class ChristmasTreeMapComponent : CustomMapComponent
    {
        public Building_AEChristmasTree Entrance;

        public Building_AEChristmasTreeExit Exit;

        private int tickOnDestroy;
        public int TickTillDestroy => tickOnDestroy - Find.TickManager.TicksGame;

        public Map SourceMap => (map.Parent as PocketMapParent)?.sourceMap;

        public ChristmasTreeMapComponent(Map map) : base(map)
        {
        }

        public override void MapGenerated()
        {
            Entrance = SourceMap?.listerThings?.ThingsOfDef(ThingDefOfLocal.AE_ChristmasTree).FirstOrDefault() as Building_AEChristmasTree;
            Exit = map.listerThings.ThingsOfDef(ThingDefOfLocal.AE_ChristmasTreeExit).FirstOrDefault() as Building_AEChristmasTreeExit;
            if (Entrance == null)
            {
                Log.Warning("ChristmasTree not found");
                return;
            }
            if (Exit == null)
            {
                Log.Error("ChristmasTreeExit not found");
                return;
            }
            tickOnDestroy = Find.TickManager.TicksGame + 600000;
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (Find.TickManager.TicksGame >= tickOnDestroy)
            {
                DestroySubMap();
            }
        }

        public void DestroySubMap()
        {
            DamageInfo damageInfo = new DamageInfo(DamageDefOf.Frostbite, 99999f, 999f);
            for (int num = map.mapPawns.AllPawns.Count - 1; num >= 0; num--)
            {
                Pawn pawn = map.mapPawns.AllPawns[num];
                pawn.TakeDamage(damageInfo);
                if (!pawn.Dead)
                {
                    pawn.Kill(damageInfo);
                }
            }
            PocketMapUtility.DestroyPocketMap(map);
            Alert_ChristmasTreeUnstable.RemoveTarget(Entrance);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref tickOnDestroy, "tickOnDestroy");
            Scribe_References.Look(ref Entrance, "Entrance");
            Scribe_References.Look(ref Exit, "Exit");
        }
    }
}
