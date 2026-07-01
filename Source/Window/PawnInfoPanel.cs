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
        private PawnInfoTab activeTab = PawnInfoTab.Endogenes;
        private Vector2 xenogeneScrollPosition;
        private Vector2 endogeneScrollPosition;
        private Vector2 traitScrollPosition;
        private Vector2 skillScrollPosition;

        private static Texture2D IconColonist => ContentFinder<Texture2D>.Get(
            "UI/Icons/PawnInfo/colonist",
            false) ?? BaseContent.BadTex;
        private static Texture2D IconCorpse => ContentFinder<Texture2D>.Get(
            "UI/Icons/PawnInfo/corpse",
            false) ?? BaseContent.BadTex;
        private static Texture2D IconPrisoner => ContentFinder<Texture2D>.Get(
            "UI/Icons/PawnInfo/prisoner",
            false) ?? BaseContent.BadTex;
        private static Texture2D IconSlave => ContentFinder<Texture2D>.Get(
            "UI/Icons/PawnInfo/slave",
            false) ?? BaseContent.BadTex;

        public void Draw(Rect rect, Pawn source, Pawn dest, string roleLabel)
        {
            const float padding = 8f;
            Rect contentRect = new Rect(rect.x + padding, rect.y + padding, rect.width - padding * 2f, rect.height - padding * 2f);

            GUI.BeginGroup(contentRect);
            try
            {
                float curY = 0f;
                Rect typeRect = new Rect(0f, curY, contentRect.width, 48f);

                if (source == null)
                {
                    return;
                }

                GUI.color = new Color(0.9f, 0.8f, 0.2f);
                Widgets.Label(typeRect, roleLabel.ToUpperInvariant());
                GUI.color = Color.white;
                curY += 24f;

                Rect leftRect = new Rect(0f, curY, contentRect.width / 2f - 12f, 24f);
                Rect rightRect = new Rect(contentRect.width / 2f + 12f, curY, contentRect.width / 2f - 12f, 24f);

                // Left column: name, xenotype, status (stacked 3 rows)
                Text.Font = GameFont.Small;
                Widgets.Label(new Rect(leftRect.x, curY, leftRect.width, 24f), source.LabelCap);

                XenotypeDef xenotypeDef = source.genes?.Xenotype ?? XenotypeDefOf.Baseliner;
                Widgets.LabelWithIcon(new Rect(leftRect.x, curY + 24f, leftRect.width, 20f), "  "+xenotypeDef.LabelCap, xenotypeDef.Icon);
                Widgets.LabelWithIcon(new Rect(leftRect.x, curY + 48f, leftRect.width, 20f), "  "+GetStatusLabel(source),
                    GetStatusIcon(source));
                Widgets.Label(new Rect(leftRect.x, curY + 72f, leftRect.width, 20f), "");

                // Right column: stats (right-justified, split into 2 lines)
                Text.Font = GameFont.Tiny;
                string traitStatus;
                int traitCount = TraitPlusUtility.CountTraits(source);
                if (OMWGenes.HasNullThrum(source))
                {
                    int traitLimit = OMW_Mod.settings.limitTraits.GetLimit(source.genes?.Xenotype);
                    if (traitLimit > 1000)
                    {
                        traitStatus = $"{traitCount} traits";
                    }
                    else
                    {
                        traitStatus = $"{traitCount}/{traitLimit} traits";
                    }
                }
                else
                {
                    traitStatus = $"{traitCount} traits";
                }

                int geneCount = source.genes?.GenesListForReading.Count ?? 0;
                int metabolism = OMWGenes.CalculateMetabolism(source);
                int complexity = OMWGenes.CalculateComplexity(source);

                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(new Rect(rightRect.x, curY, rightRect.width, 16f), $"{traitStatus}");
                Widgets.Label(new Rect(rightRect.x, curY + 18f, rightRect.width, 16f), $"{geneCount} genes");
                Widgets.Label(new Rect(rightRect.x, curY + 2*18f, rightRect.width, 16f), $"{complexity} complexity");
                Widgets.Label(new Rect(rightRect.x, curY + 3*18f, rightRect.width, 16f),
                    $"{metabolism} metabolism");
                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Small;

                curY += 90f;

                float tabHeight = 24f;
                float tabWidth = (contentRect.width - 6f) / 4f;
                Rect[] tabRects = new Rect[4]
                {
                    new Rect(0f, curY, tabWidth, tabHeight),
                    new Rect(tabWidth + 2f, curY, tabWidth, tabHeight),
                    new Rect((tabWidth + 2f) * 2f, curY, tabWidth, tabHeight),
                    new Rect((tabWidth + 2f) * 3f, curY, tabWidth, tabHeight)
                };

                DrawTabButton(tabRects[0], "Endo", PawnInfoTab.Endogenes);
                DrawTabButton(tabRects[1], "Xeno", PawnInfoTab.Xenogenes);
                DrawTabButton(tabRects[2], "Traits", PawnInfoTab.Traits);
                DrawTabButton(tabRects[3], "Skills", PawnInfoTab.Skills);
                curY += tabHeight + 6f;

                Rect contentArea = new Rect(0f, curY, contentRect.width, contentRect.height - curY - 4f);
                DrawActiveContent(contentArea, source, dest);
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

        private void DrawActiveContent(Rect rect, Pawn source, Pawn dest)
        {
             switch (activeTab)
            {
                case PawnInfoTab.Xenogenes:
                    DrawGeneList(rect, source, dest, true, ref xenogeneScrollPosition);
                    break;
                case PawnInfoTab.Endogenes:
                    DrawGeneList(rect, source, dest, false, ref endogeneScrollPosition);
                    break;
                case PawnInfoTab.Traits:
                    DrawTraitList(rect, source, dest, ref traitScrollPosition);
                    break;
                case PawnInfoTab.Skills:
                    DrawSkillList(rect, source, ref skillScrollPosition);
                    break;
            }
        }

        private void DrawGeneList(Rect rect, Pawn source, Pawn dest, bool xenotype, ref Vector2 scrollPosition)
        {
            List<Gene> genes;

            if (xenotype)
            {
                genes = source?.genes?.Xenogenes ?? new List<Gene>();                   
            }
            else
            {
                genes = source?.genes?.Endogenes ?? new List<Gene>();
            }

            if (genes.Count == 0)
            {
                Widgets.Label(rect, "None");
                return;
            }

            HashSet<Gene> alreadyHas = dest?.genes?.GenesListForReading.ToHashSet() ?? new HashSet<Gene>();

            float listHeight = Mathf.Max(120f, genes.Count * 24f + 4f);
            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, listHeight);
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
            float curY = 0f;
            foreach (Gene gene in genes.OrderBy(g => g.LabelCap))
            {
                if (gene.def == null)
                {
                    continue;
                }

                if (gene.Overridden)
                {
                    GUI.color = Color.gray;
                }
                else if (alreadyHas.Contains(gene))
                {
                    GUI.color = Color.red;
                }
                else
                {
                    if (gene.def.biostatArc > 0)
                    {
                        GUI.color = Color.yellow;
                    }
                    else if (gene.def.biostatMet > 0)
                    {
                        GUI.color = Color.green;
                    }
                    else if (gene.def.biostatMet < 0)
                    {
                        GUI.color = Color.red;
                    }
                    else {
                        GUI.color = Color.white;
                    }
                }

                Widgets.DefIcon(new Rect(0f, curY, 20f, 20f), gene.def);
                Widgets.Label(new Rect(24f, curY, viewRect.width - 24f, 22f), gene.LabelCap);
                GUI.color = Color.white;
                curY += 24f;
            }
            Widgets.EndScrollView();
        }

        private void DrawTraitList(Rect rect, Pawn source, Pawn dest, ref Vector2 scrollPosition)
        {
            List<Trait> traits = source?.story.traits.allTraits ?? new List<Trait>();
            if (traits.Count == 0)
            {
                Widgets.Label(rect, "None");
                return;
            }

            HashSet<TraitDef> alreadyHas = dest?.story?.traits?.allTraits.Select(t => t.def).ToHashSet() ?? new HashSet<TraitDef>();

            float listHeight = Mathf.Max(120f, traits.Count * 24f + 4f);
            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, listHeight);
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
            float curY = 0f;
            foreach (Trait trait in traits.OrderBy(t => t.LabelCap))
            {
                if (trait.sourceGene != null)
                {
                    GUI.color = Color.cyan;
                }
                else if (trait.suppressedByTrait)
                {
                    GUI.color = Color.gray;
                }
                else if (alreadyHas.Contains(trait.def))
                {
                    GUI.color = Color.red;
                }
                else
                {
                    GUI.color = Color.white;
                }


                Widgets.Label(new Rect(0f, curY, viewRect.width, 22f), trait.LabelCap);
                curY += 24f;
                GUI.color = Color.white;
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
                
                // need to pad the skill with spaces for alignment
                Widgets.Label(new Rect(24f, curY, 20f, 22f), $"{skill.GetLevelForUI()}");
                Widgets.Label(new Rect(48f, curY, viewRect.width - 70f, 22f), skill.def.defName );
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
                return "Corpse";
            }
            if (pawn.IsPrisoner)
            {
                return "Prisoner";
            }
            if (pawn.IsSlave)
            {
                return "Slave";
            }
            return "Colonist";
        }

        private Texture2D GetStatusIcon(Pawn pawn)
        {
            if (pawn.Dead)
            {
                return IconCorpse;
            }

            if (pawn.IsPrisoner)
            {
                return IconPrisoner;
            }

            if (pawn.IsSlave)
            {
                return IconSlave;
            }

            return IconColonist;            
        }

        private string GetPassionLabel(Passion passion)
        {
            switch (passion)
            {
                case Passion.None:
                    return "";
                case Passion.Major:
                    return "**";
                case Passion.Minor:
                    return "*";
                default:
                    return "?";
            }
        }
    }
}
