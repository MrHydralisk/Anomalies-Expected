using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace AnomaliesExpected
{
    public class Comp_EntityWithWeapon : ThingComp
    {
        public ThingDef weaponDef;
        public Pawn parentPawn => parent as Pawn;

        public override void Notify_BecameVisible()
        {
            base.Notify_BecameVisible();
            CheckIfCombatReady();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            TryFindWeaponDef();
            TryGiveWeapon();
        }

        public override void CompTick()
        {
            base.CompTick();
            if (parentPawn.IsHashIntervalTick(500))
            {
                CheckIfCombatReady();
            }
        }

        public void CheckIfCombatReady()
        {
            if (!parentPawn.DeadOrDowned && parentPawn.Spawned && parentPawn.Faction != Faction.OfPlayer)
            {
                TryFindWeaponDef();
                TryGiveWeapon();
                TryGiveAssaultJob();
            }
        }

        public void TryFindWeaponDef()
        {
            if (weaponDef == null)
            {
                weaponDef = DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef t) => t.IsWeapon && !t.weaponTags.NullOrEmpty() && t.weaponTags.Any((string s) => parentPawn.kindDef.weaponTags.Contains(s))).RandomElement();
            }
        }

        public void TryGiveWeapon()
        {
            if (!parentPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                return;
            }
            List<string> weaponTags = parentPawn.kindDef.weaponTags;
            if (weaponTags.NullOrEmpty())
            {
                return;
            }
            List<ThingWithComps> allEquipmentListForReading = parentPawn.equipment.AllEquipmentListForReading;
            for (int i = 0; i < allEquipmentListForReading.Count; i++)
            {
                if (allEquipmentListForReading[i].def.weaponTags.Any((string t) => weaponTags.Contains(t)))
                {
                    return;
                }
            }
            PawnWeaponGenerator.TryGenerateWeaponFor(parentPawn, default(PawnGenerationRequest));
            parentPawn.equipment.AddEquipment(ThingMaker.MakeThing(weaponDef) as ThingWithComps);
        }

        public void TryGiveAssaultJob()
        {
            if (parentPawn.GetLord() == null)
            {
                LordMaker.MakeNewLord(parentPawn.Faction, new LordJob_AssaultColony(), parentPawn.MapHeld, new List<Pawn> { parentPawn });
            }
        }

        public override void PostExposeData()
        {
            Scribe_Defs.Look(ref weaponDef, "weaponDef");
            base.PostExposeData();
        }
    }
}
