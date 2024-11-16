using RimWorld;
using RimWorld.Planet;
using System.Linq;
using Verse;

namespace AnomaliesExpected
{
    public class BloodLakeMapComponent : CustomMapComponent
    {
        private const int MinSpawnDistFromGateExit = 10; //Temp

        public Building_AEBloodLake Entrance;

        public Building_AEBloodLakeExit Exit;

        public Building_AE Terminal;

        public Map SourceMap => (map.Parent as PocketMapParent)?.sourceMap;

        public BloodLakeMapComponent(Map map) : base(map)
        {
        }

        public override void MapGenerated()
        {
            Entrance = SourceMap?.listerThings?.ThingsOfDef(ThingDefOfLocal.AE_BloodLake).FirstOrDefault() as Building_AEBloodLake;
            Exit = map.listerThings.ThingsOfDef(ThingDefOfLocal.AE_BloodLakeExit).FirstOrDefault() as Building_AEBloodLakeExit;
            Terminal = map.listerThings.ThingsOfDef(ThingDefOfLocal.AE_BloodLakeTerminal).FirstOrDefault() as Building_AE;
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
                Entrance.StudyUnlocks.UnlockStudyNoteManual(1);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref Entrance, "Entrance");
            Scribe_References.Look(ref Exit, "Exit");
        }
    }
}
