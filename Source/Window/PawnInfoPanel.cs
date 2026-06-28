using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace OMW_Samhaphage
{
    public enum PawnInfoTab
    {
        Xenogenes,
        Endogenes,
        Traits,
        Skills
    }

    public class PawnInfoPanel
    {
        private PawnInfoTab activeTab = PawnInfoTab.Xenogenes;
        private Vector2 xenogeneScrollPosition;
        private Vector2 endogeneScrollPosition;
        private Vector2 traitScrollPosition;
        private Vector2 skillScrollPosition;

        public void Draw(Rect rect, Pawn pawn, string roleLabel)
        {
            if (pawn == null)
            {
                return;
            }

            const float padding = 8f;
            Rect contentRect = new Rect(rect.x + padding, rect.y + padding, rect.width - padding * 2f, rect.height - padding * 2f);
            GUI.BeginGroup(contentRect);
            try
            {
                float curY = 0f;
                Rect typeRect = new Rect(0f, curY, contentRect.width, 22f);
                GUI.color = new Color(0.9f, 0.8f, 0.2f);
                Widgets.Label(typeRect, roleLabel.ToUpperInvariant());
                GUI.color = Color.white;
                curY += 24f;

                Rect headerRect = new Rect(0f, curY, contentRect.width, 48f);
                Text.Font = GameFont.Small;
                Widgets.Label(headerRect, pawn.LabelShortCap);
                Text.Font = GameFont.Tiny;
                Widgets.Label(new Rect(0f, headerRect.yMax, contentRect.width, 20f), GetFactionLabel(pawn));
                Widgets.Label(new Rect(0f, headerRect.yMax + 16f, contentRect.width, 20f), GetStatusLabel(pawn));
                Text.Font = GameFont.Small;
                curY = headerRect.yMax + 38f;

                Widgets.DrawLineHorizontal(0f, curY, contentRect.width);
                curY += 8f;

                float tabHeight = 24f;
                float tabWidth = (contentRect.width - 6f) / 4f;
                Rect[] tabRects = new Rect[4]
                {
                    new Rect(0f, curY, tabWidth, tabHeight),
                    new Rect(tabWidth + 2f, curY, tabWidth, tabHeight),
                    new Rect((tabWidth + 2f) * 2f, curY, tabWidth, tabHeight),
                    new Rect((tabWidth + 2f) * 3f, curY, tabWidth, tabHeight)
                };

                DrawTabButton(tabRects[0], "Xeno", PawnInfoTab.Xenogenes);
                DrawTabButton(tabRects[1], "Endo", PawnInfoTab.Endogenes);
                DrawTabButton(tabRects[2], "Traits", PawnInfoTab.Traits);
                DrawTabButton(tabRects[3], "Skills", PawnInfoTab.Skills);
                curY += tabHeight + 6f;

                Rect contentArea = new Rect(0f, curY, contentRect.width, contentRect.height - curY - 4f);
                DrawActiveContent(contentArea, pawn);
            }
            finally
            {
                GUI.EndGroup();
                GUI.color = Color.white;
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.UpperLeft;
            }
        }

        private void DrawTabButton(Rect rect, string label, PawnInfoTab tab)
        {
            GUI.color = activeTab == tab ? new Color(0.65f, 0.8f, 1f) : Color.white;
            if (Widgets.ButtonText(rect, label))
            {
                activeTab = tab;
            }
            GUI.color = Color.white;
        }

        private void DrawActiveContent(Rect rect, Pawn pawn)
        {
            switch (activeTab)
            {
                case PawnInfoTab.Xenogenes:
                    DrawGeneList(rect, pawn?.genes?.Xenogenes?.ToList() ?? new List<Gene>(), ref xenogeneScrollPosition);
                    break;
                case PawnInfoTab.Endogenes:
                    DrawGeneList(rect, pawn?.genes?.Endogenes?.ToList() ?? new List<Gene>(), ref endogeneScrollPosition);
                    break;
                case PawnInfoTab.Traits:
                    DrawTraitList(rect, pawn?.story?.traits?.allTraits ?? new List<Trait>(), ref traitScrollPosition);
                    break;
                case PawnInfoTab.Skills:
                    DrawSkillList(rect, pawn, ref skillScrollPosition);
                    break;
            }
        }

        private void DrawGeneList(Rect rect, List<Gene> genes, ref Vector2 scrollPosition)
        {
            if (genes == null || genes.Count == 0)
            {
                Widgets.Label(rect, "None");
                return;
            }

            float listHeight = Mathf.Max(120f, genes.Count * 24f + 4f);
            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, listHeight);
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
            float curY = 0f;
            foreach (Gene gene in genes)
            {
                if (gene == null || gene.def == null)
                {
                    continue;
                }

                if (gene.Overridden)
                {
                    GUI.color = Color.gray;
                }
                else
                {
                    GUI.color = Color.white;
                }

                Widgets.DefIcon(new Rect(0f, curY, 20f, 20f), gene.def);
                Widgets.Label(new Rect(24f, curY, viewRect.width - 24f, 22f), gene.LabelCap);
                GUI.color = Color.white;
                curY += 24f;
            }
            Widgets.EndScrollView();
        }

        private void DrawTraitList(Rect rect, List<Trait> traits, ref Vector2 scrollPosition)
        {
            if (traits == null || traits.Count == 0)
            {
                Widgets.Label(rect, "None");
                return;
            }

            float listHeight = Mathf.Max(120f, traits.Count * 24f + 4f);
            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, listHeight);
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
            float curY = 0f;
            foreach (Trait trait in traits)
            {
                if (trait == null)
                {
                    continue;
                }

                Widgets.Label(new Rect(0f, curY, viewRect.width, 22f), trait.LabelCap);
                curY += 24f;
            }
            Widgets.EndScrollView();
        }

        private void DrawSkillList(Rect rect, Pawn pawn, ref Vector2 scrollPosition)
        {
            List<SkillRecord> skills = pawn?.skills?.skills;
            if (skills == null || skills.Count == 0)
            {
                Widgets.Label(rect, "None");
                return;
            }

            float listHeight = Mathf.Max(120f, skills.Count * 24f + 4f);
            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, listHeight);
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
            float curY = 0f;
            foreach (SkillRecord skill in skills)
            {
                if (skill == null || skill.def == null)
                {
                    continue;
                }

                Widgets.Label(new Rect(0f, curY, viewRect.width - 70f, 22f), skill.def.LabelCap);
                Widgets.Label(new Rect(viewRect.width - 70f, curY, 70f, 22f), GetPassionLabel(skill.passion));
                curY += 24f;
            }
            Widgets.EndScrollView();
        }

        private string GetFactionLabel(Pawn pawn)
        {
            return pawn.Faction?.Name ?? "No faction";
        }

        private string GetStatusLabel(Pawn pawn)
        {
            if (pawn.Dead)
            {
                return "corpse";
            }
            if (pawn.IsPrisoner)
            {
                return "prisoner";
            }
            if (pawn.IsSlave)
            {
                return "slave";
            }
            return "colonist";
        }

        private string GetPassionLabel(Passion passion)
        {
            switch (passion)
            {
                case Passion.Major:
                    return "Major";
                case Passion.Minor:
                    return "Minor";
                default:
                    return "None";
            }
        }
    }
}
