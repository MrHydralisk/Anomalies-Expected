using NAudio.Utils;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AnomaliesExpected
{
    public class Dialog_AEEntityDatabaseAnomaly : Window
    {
        private Vector2 ScrollPos;

        private float ScrollHeight;

        //private AEEntityEntry selectedEntry;

        //private List<(string, List<AEEntityEntry>)> EntriesByType = new List<(string, List<AEEntityEntry>)>();

        //private Dictionary<string, float> categoryRectSizes = new Dictionary<string, float>();

        //private static readonly Vector2 ButSize = new Vector2(150f, 38f);

        private const float HeaderHeight = 30f;

        private const float EntrySize = 128f;

        private const float EntryGap = 8f;
        //private List<Thing> RelatedAnalyzableThingsCached = new List<Thing>();

        public override Vector2 InitialSize => new Vector2(UI.screenWidth * 0.6f, UI.screenHeight * 0.75f);

        //private int state = 0;
        //private bool isShowRecord;
        //private bool[] lettersOpened;
        private Comp_EntityDatabaseAnomaly entityDatabaseAnomaly;

        public Dialog_AEEntityDatabaseAnomaly(Comp_EntityDatabaseAnomaly comp_EntityDatabaseAnomaly)
        {
            doCloseX = true;
            forcePause = true;
            entityDatabaseAnomaly = comp_EntityDatabaseAnomaly;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;
            Rect rect = inRect;
            //rect.height -= ButSize.y + 10f;
            TaggedString taggedString = "AnomaliesExpected.EntityDatabaseAnomaly.Desc".Translate(entityDatabaseAnomaly.entityIncidentsAvailable.Count((AEEntityIncidents aeei) => aeei.isCanFireNow), entityDatabaseAnomaly.entityIncidentsAvailable.Count());
            using (new TextBlock(GameFont.Medium))
            {
                Widgets.Label(new Rect(0f, 0f, rect.width, HeaderHeight), entityDatabaseAnomaly.parent.Label);
            }
            rect.yMin += 40f;
            TaggedString taggedString1 = "AnomaliesExpected.EntityDataBase.Desc".Translate();
            float num = Text.CalcHeight(taggedString1, rect.width);
            Widgets.Label(new Rect(0f, rect.y, rect.width, num), taggedString);
            rect.yMin += num + 10f;
            AllEntities(rect);
        }

        private void AllEntities(Rect rect)
        {
            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, ScrollHeight);
            Widgets.BeginScrollView(rect, ref ScrollPos, viewRect);
            float num = 0f;
            foreach (AEEntityIncidents entityIncidents in entityDatabaseAnomaly.entityIncidentsAvailable)
            {
                Rect rect2 = new Rect(0, num, viewRect.width, EntrySize);
                DrawEntry(rect2, entityIncidents);
                num += EntryGap + EntrySize;
            }
            ScrollHeight = num;
            Widgets.EndScrollView();
        }

        private void DrawEntry(Rect rect, AEEntityIncidents entityIncidents)
        {
            Widgets.DrawOptionBackground(rect, entityIncidents.entityCodexEntryDef.provocationIncidents.Contains(entityDatabaseAnomaly.selectedIncidentDef));
            Rect rect1 = new Rect(rect.x, rect.y, EntrySize, EntrySize);
            Texture2D uiIcon = entityIncidents.entityCodexEntryDef.icon;
            Color colorTMP = GUI.color;
            GUI.DrawTexture(rect1.ContractedBy(2f), uiIcon);
            GUI.color = colorTMP;
            using (new TextBlock(GameFont.Medium))
            {
                Widgets.Label(new Rect(rect.x + EntrySize + 4, rect.y, rect.width - EntrySize - 4, HeaderHeight), entityIncidents.entityCodexEntryDef.LabelCap);
            }
            using (new TextBlock(GameFont.Small))
            {
                Widgets.Label(new Rect(rect.x + EntrySize + 4, rect.y + HeaderHeight, rect.width - EntrySize - 4, rect.height - HeaderHeight), (entityIncidents.isCanFireNow ? "AnomaliesExpected.EntityDatabaseAnomaly.Available.Now".Translate() : entityIncidents.isFiredTooRecently ? "AnomaliesExpected.EntityDatabaseAnomaly.Available.Recently".Translate() : entityIncidents.isCannotBeProvoked ? "Cant" : "AnomaliesExpected.EntityDatabaseAnomaly.Available.Else".Translate()));
            }
            if (entityIncidents.entityCodexEntryDef.Discovered && entityIncidents.isCanFireNow && Widgets.ButtonInvisible(rect))
            {
                entityDatabaseAnomaly.selectedIncidentDef = entityIncidents.incidentDefs.RandomElement();
            }
        }

        //public void SelectEntry(AEEntityEntry entry)
        //{
        //    selectedEntry = entry;
        //    isShowRecord = true;
        //    lettersOpened = new bool[selectedEntry.letters.Count()];
        //    SoundDefOf.Tick_High.PlayOneShotOnCamera();
        //    UpdateRelatedAnalyzableThing();
        //}
    }
}

