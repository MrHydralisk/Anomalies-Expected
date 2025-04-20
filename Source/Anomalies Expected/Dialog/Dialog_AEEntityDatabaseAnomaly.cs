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
            TaggedString taggedString = "AnomaliesExpected.EntityDatabaseAnomaly.Desc".Translate(entityDatabaseAnomaly.parent.Label);
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
            Text.Anchor = TextAnchor.MiddleCenter;
            foreach (EntityCodexEntryDef entityCodexEntryDef in DefDatabase<EntityCodexEntryDef>.AllDefs)
            {
                Rect rect2 = new Rect(0, num, EntrySize, EntrySize);
                DrawEntry(rect2, entityCodexEntryDef);
                num += EntryGap + EntrySize;
            }
            ScrollHeight = num;
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.EndScrollView();
        }

        private void DrawEntry(Rect rect, EntityCodexEntryDef entry)
        {
            Widgets.DrawOptionBackground(rect, entry.provocationIncidents.Contains(entityDatabaseAnomaly.selectedIncidentDef));
            Rect rect1 = new Rect(rect);
            Texture2D uiIcon;
            Color colorTMP = GUI.color;
            if (entry.Discovered)
            {
                uiIcon = entry.icon; ;
            }
            else
            {
                uiIcon = entry.silhouette;
            }
            GUI.DrawTexture(rect1.ContractedBy(2f), uiIcon);
            GUI.color = colorTMP;
            //if (entry.letters.Count() > 0)
            //{
            //    GUI.DrawTexture(new Rect(rect.x + rect.width - 20, rect.y - 1, 18, 18), TexButton.CategorizedResourceReadout);
            //}
            if (Widgets.ButtonInvisible(rect))
            {
                //SelectEntry(entry);
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

