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

        public override bool IsLethal => true;

        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"Render {victim.LabelShort}'s body into raw biomass, ready for consumption.";
        }

        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Render");

        public override void ApplyCorpse(Corpse corpse, Pawn caster)
        {
            if (corpse?.InnerPawn == null || caster == null) return;

            Pawn victim = corpse.InnerPawn;

            string msg = $"{victim.LabelShort}'s corpse was destroyed after being rendered for their meat and attenuated for their genes.";
            System.Action sacrificeAction = () =>
            {
                Log.Debug(
                    $"Pre rendering: marketValue of the corpse {corpse.MarketValue}, victim {victim.LabelShort}");

                ThingApplyAttenuate attenuate = new ThingApplyAttenuate();
                attenuate.ApplyPawn(victim, caster);
                ThingApplyMute mute = new ThingApplyMute();
                mute.ApplyPawn(victim, caster);
                victim.Strip();
                KillUtility.PurgeBionics(victim);
                Log.Debug($"Post rendering: marketValue of the corpse {corpse.MarketValue}, victim {victim.LabelShort}");
                ResonanceUtility.Incr("Render", caster, OMW_Mod.settings.abilityValue.render.value);                
                KillUtility.CorpseDestroy(corpse);
                Messages.Message(msg, MessageTypeDefOf.NegativeEvent);
            };

            ShowCorpseConfirmation(victim, sacrificeAction);
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