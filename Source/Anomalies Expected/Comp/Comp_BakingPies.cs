using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class Comp_BakingPies : CompInteractable
    {
        public new CompProperties_BakingPies Props => (CompProperties_BakingPies)props;

        private List<Thing> piePieces = new List<Thing>();

        private int piePiecesExisting = 1;

        private int TickSpawn;
        private int TickCanCount;

        protected CompAEStudyUnlocks StudyUnlocks => studyUnlocksCached ?? (studyUnlocksCached = parent.TryGetComp<CompAEStudyUnlocks>());
        private CompAEStudyUnlocks studyUnlocksCached;

        public override bool HideInteraction => (StudyUnlocks?.NextIndex ?? 4) < 4;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                TickSpawn = Find.TickManager.TicksGame + Props.tickPerSpawn;
            }
            CalculatePiePieces();
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if (AEMod.Settings.DespawnPiecesOnDestroy)
            {
                for (int i = piePieces.Count() - 1; i >= 0; i--)
                {
                    Thing piece = piePieces[i];
                    if (piece != null && !piece.Destroyed && piece.Spawned)
                    {
                        TargetInfo targetInfo = new TargetInfo(piece.Position, piece.Map);
                        SoundDefOfLocal.Psycast_Skip_Exit.PlayOneShot(targetInfo);
                        FleckMaker.Static(targetInfo.Cell, targetInfo.Map, FleckDefOf.PsycastSkipInnerExit, 0.2f);
                        piece.Destroy();
                    }
                }
            }
            base.PostDestroy(mode, previousMap);
        }

        public override void CompTickLong()
        {
            base.CompTickLong();
            if (Find.TickManager.TicksGame >= TickSpawn)
            {
                SpawnPiePiece();
                TickSpawn = Find.TickManager.TicksGame + Props.tickPerSpawn;
            }
        }

        public int CalculatePiePieces(bool isNotRare = true)
        {
            if (isNotRare || Find.TickManager.TicksGame > TickCanCount + 60)
            {
                int countExisting = 1;
                for (int i = piePieces.Count() - 1; i >= 0; i--)
                {
                    Thing piePiece = piePieces[i];
                    if (piePiece == null || piePiece.Destroyed || piePiece.GetUniqueLoadID == parent.GetUniqueLoadID)
                    {
                        piePieces.RemoveAt(i);
                        continue;
                    }
                    countExisting++;
                }
                piePieces.Insert(0, parent);
                piePiecesExisting = countExisting;
                TickCanCount = Find.TickManager.TicksGame + 60;
            }
            return piePiecesExisting;
        }

        public void SpawnPiePiece()
        {
            int amount = CalculatePiePieces();
            if (AEMod.Settings.ReplicationLimit > 0)
            {
                amount = Mathf.Min(amount, AEMod.Settings.ReplicationLimit);
            }
            if (AEMod.Settings.SpawnPiePieceNotification)
            {
                Messages.Message("AnomaliesExpected.BakingPies.SpawnPiePiece".Translate(parent.LabelCap).RawText, new TargetInfo(parent.Position, parent.Map), MessageTypeDefOf.NeutralEvent);
            }
            int l = 0;
            for (int i = 0; i < piePieces.Count(); i++)
            {
                Thing piePiece = piePieces.ElementAtOrDefault(i);
                if (piePiece != null && !piePiece.Destroyed)
                {
                    if (piePiece.Spawned && i >= l)
                    {
                        IntVec3 pos = piePiece.Position;
                        Map map = piePiece.Map ?? parent.Map;
                        for (int j = 0; j < amount; j++)
                        {
                            Thing piece = ThingMaker.MakeThing(Props.piePiece);
                            piePieces.Add(piece);
                            bool isPlaced = GenPlace.TryPlaceThing(piece, pos, map, ThingPlaceMode.Near, null);
                            if (isPlaced)
                            {
                                TargetInfo targetInfo = new TargetInfo(piece.Position, piece.Map);
                                SoundDefOf.Psycast_Skip_Entry.PlayOneShot(targetInfo);
                                FleckMaker.Static(targetInfo.Cell, targetInfo.Map, FleckDefOf.PsycastSkipFlashEntry, 0.2f);
                                amount -= 1;
                                if (amount <= 0)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                l = i + Mathf.Min(8, amount / 5000);
                                break;
                            }
                        }
                        if (amount <= 0)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    piePieces.RemoveAt(i);
                    i--;
                }
            }
            CalculatePiePieces();
            if (amount > 100)
            {
                Find.TickManager.Pause();
                Find.LetterStack.ReceiveLetter("AnomaliesExpected.BakingPies.EndGame.Label".Translate(), "AnomaliesExpected.BakingPies.EndGame.Text".Translate(CalculatePiePieces(false), amount, parent.LabelCap), LetterDefOf.GameEnded, parent);
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                if (gizmo is Command_Action command_Action)
                {
                    command_Action.hotKey = KeyBindingDefOf.Misc8;
                }
                yield return gizmo;
            }
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
                        floatMenuOptions.Add(new FloatMenuOption("Increase by 3d", delegate
                        {
                            TickSpawn += 180000;
                        }));
                        floatMenuOptions.Add(new FloatMenuOption("Increase by 1d", delegate
                        {
                            TickSpawn += 60000;
                        }));
                        floatMenuOptions.Add(new FloatMenuOption("Increase by 1h", delegate
                        {
                            TickSpawn += 2500;
                        }));
                        floatMenuOptions.Add(new FloatMenuOption("Decrease by 1h", delegate
                        {
                            TickSpawn -= 2500;
                        }));
                        floatMenuOptions.Add(new FloatMenuOption("Decrease by 1d", delegate
                        {
                            TickSpawn -= 60000;
                        }));
                        floatMenuOptions.Add(new FloatMenuOption("Decrease by 3d", delegate
                        {
                            TickSpawn -= 180000;
                        }));
                        Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                    },
                    defaultLabel = "Dev: Change Spawn",
                    defaultDesc = $"Change timer till spawn piece of pie: {(TickSpawn - Find.TickManager.TicksGame).ToStringTicksToPeriodVerbose()}"
                };
                yield return new Command_Action
                {
                    action = delegate
                    {
                        SpawnPiePiece();
                    },
                    defaultLabel = "Dev: Spawn",
                    defaultDesc = $"Spawn piece of pie: {(TickSpawn - Find.TickManager.TicksGame).ToStringTicksToPeriodVerbose()}"
                };
            }
        }

        protected override void OnInteracted(Pawn caster)
        {
            HealthUtility.AdjustSeverity(caster, Props.ConsumptionHediffDef, 1);
            parent.Destroy();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref piePieces, "piePieces", LookMode.Reference);
            Scribe_Values.Look(ref TickSpawn, "TickSpawn", Find.TickManager.TicksGame + Props.tickPerSpawn);
        }


        public override string CompInspectStringExtra()
        {
            List<string> inspectStrings = new List<string>();
            int study = StudyUnlocks?.NextIndex ?? 4;
            if (study > 1)
            {
                inspectStrings.Add("AnomaliesExpected.BakingPies.Amount".Translate(CalculatePiePieces(false)).RawText);
                if (study > 2)
                {
                    inspectStrings.Add("AnomaliesExpected.BakingPies.Time".Translate((TickSpawn - Find.TickManager.TicksGame).ToStringTicksToPeriodVerbose()).RawText);
                }
            }
            return String.Join("\n", inspectStrings);
        }
    }
}
