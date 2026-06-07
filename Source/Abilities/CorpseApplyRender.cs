using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace OMW_Samhaphage
{    
    public class CorpseApplyRender : NullThrumAbilityCorpseOnly
    {
        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.render;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;

        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"Render {victim.LabelShort}'s body into raw biomass, ready for consumption.";
        }

        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Render");

        public override bool ApplyCorpse(Corpse corpse, Pawn caster)
        {
            if (corpse?.InnerPawn == null || caster == null) return false;

            Pawn victim = corpse.InnerPawn;

            bool value = false;
            string msg = $"{victim.LabelShort}'s corpse was destroyed after being rendered for their meat and attenuated for their genes.";
            System.Action sacrificeAction = () =>
            {
                ResonanceUtility.Incr("Render", caster, OMW_Mod.settings.abilityValue.render.value);
                // only attenuate corpses
                ThingApplyAttenuate attenuate = new ThingApplyAttenuate();
                SelectionAttenuate selectorAttenuate = attenuate.CanApplyAttenuate(victim, caster);
                if (selectorAttenuate != null)
                {
                    attenuate.ApplyAttenuate(victim, caster, selectorAttenuate);
                }
                KillUtility.CorpseDestroy(corpse);
                Messages.Message(msg, MessageTypeDefOf.NegativeEvent);
                value = true;                        
            };

            OMW_UIHelpers.ShowCorpseConfirmation(victim, sacrificeAction);
            return value;
        }

        public override bool CanApplyOnCorpse(Corpse corpse, Pawn caster, out string reason)
        {
            reason = "unknown";
            if (!ResonanceUtility.HasGene(caster))
            {
                reason = $"{caster.LabelShort} does not have a supply of resonance.";
                return false;
            }            
            return true;
        }
    }
}