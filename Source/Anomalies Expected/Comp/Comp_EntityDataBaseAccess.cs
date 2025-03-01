using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AnomaliesExpected
{
    public class Comp_EntityDataBaseAccess : ThingComp
    {
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Action
            {
                defaultLabel = "AnomaliesExpected.EntityDataBase.Label".Translate(),
                defaultDesc = "AnomaliesExpected.EntityDataBase.Tip".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Buttons/AEEntityDB"),
                action = delegate
                {
                    AEEntityEntry selectedEntityEntry = GameComponent_AnomaliesExpected.instance.GetEntityEntryFromThingDef((parent as Building_HoldingPlatform)?.HeldPawn.def ?? parent.def);
                    Dialog_AEEntityDB dialog = new Dialog_AEEntityDB();
                    if (selectedEntityEntry != null)
                    {
                        dialog.SelectEntry(selectedEntityEntry);
                    }
                    Find.WindowStack.Add(dialog);
                }
            };
        }
    }
}
