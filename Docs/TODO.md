# TODO

## Next

* Genetic complexity should be calculated as a percentage * genetic complexity. The percentage could change based on the difficulty presets and the xenotype.
* render should mute psychic abilities
* how does WVC make genes always override anything else? Adds a gene class with 

```C#
		public virtual void Notify_OverriddenBy(Gene overriddenBy)
		{
			if (!WVC_Biotech.settings.enable_OverOverridableGenesMechanic)
			{
				return;
			}
			if (overriddenBy is not IGeneUnoverridable && overrideTries < 100 && overriddenBy.def.ConflictsWith(def))
			{
				this.OverrideBy(null);
				overriddenBy.OverrideBy(this);
			}
			overrideTries++;
		}
```

## MOD SETTINGS

* Add several presets for balance: default, story mode, hurt me daddy
* A mod setting to control the time of silent servitude and dissonance

## BALANCE

* silent servitude is too strong at recruiting
* blacklist traits based on total power value
* blacklist genes based on total power value
* max # of genes, story mode unlimited
* max # of traits, story mode unlimited
* Dissonance: level 1: off, level 2: scales to 0 based off of resonance, level3: fixed amount of time
* limit number of samhaphage? based on percentage of colony size, configurable setting?
* the fluxspawn aren't dying fast enough
* control for what to include in random mutation (e.g. blWretch)
* can genes modify worktags? need a blacklist trigger for randomly mutating those
* flatten strips genes/traits with Kind?
* will raising market value of the pawns just make selling fluxspawn into slavery very profitable?
* Resurrect needs should calculate the genes/traits and then use that to charge the resonance. Ability would have a yes/no selection pop up about continuing.

## BUGS

* replace tetragenic abilities with resonance and add them to the blacklist
* Excise isn't lethal if target is self
* Flatten does not return to main menu

## UI

* don't have search on the ability UI
* five icons per row
* stats panel in menu about the selected pawn's genes, traits, hediffs - hediffs would list bionics and psylinks

## Ideology

* Create the .rid file based on the current modlist on a new game

## Resources

### Genes

* add meditation focus types
* need short descriptions for the Hediffs

### Traits

* Should handle conflicting traits the same way as I do with genes with respect to the GUI.
* trait abilities should cause brain damage and be blocked by missing brain
* need to blacklist traits that disable worktypes
  
### Resonance

* resonance level should reduce the time of genetic dissonance
* gain resonance on kill based on the market value of what you killed / 100?
* "resonance efficiency" by xenotype. +/- percentage on increases/decreases.
* "resonance thirst" power up when hit zero resonance? maybe not useful if daily resonance is implemented.
* Daily resonance based on complexity?

### Backstories

* some kind of ability that wipes out backstories and relationships? and social mood? maybe take that out of flatten

## Xenotypes

### EchoVessel

* prevent raising damaged brain or too desiccated / rotting
* Implement echovessel abilities if CorpseChildren isn't available
* give resonance for the destroyed genes
* some kind of desiccation / rotting/ undead from B&S?

### Cradlemold

* Implement custom initiate lovin' (psychite goggles) if none of the other mods that implement it are available?
* Just don't include cradlemold without a lovin' system?

### Hallowbound

* inverse of scrub... flip a disabled gene to front of xenogenes

### Fluxspawn

* flicker stun needs to be a separate ability so it can be used while drafted
* Still working on flicker
* Faster speed
* Stun
* Smoke Cloud

### Samhaphage

### Sovereign Stillness

## Scenario

### Temple

* add a table to food area

## Requirements

* base requirements only AlphaGenes and Big&Small
* CorpseChildren and WVC should be Major Recommendations
