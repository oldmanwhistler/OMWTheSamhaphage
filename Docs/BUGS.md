# BUGS

## Fuckin' traits man

* Why does character editor show a million psychopath traits... need to check save game data.. and it also has a million Psychopaths. Fuck.
* the conflictsWith logic for traits seems like it isn't by-directional? perhaps scan the entire defdatabase at the start and build up my own list of conflicting traits
* Specifically a problem with scoured mind I think because it is trying to affect multiple traits so it pingpongs?
* The routine I added in the settings menu "kind" of cleans this up, like it brought it down from hundreds to 3 for each trait.
* I wonder if it goes up when I load a save or call Refresh(pawn). 
* When I am adding genes I need to call the ConflictsWith() method instead of how I am doing it now.
* I need the same blocker for adding traits.
* It keeps going up, is it a thing where AddGene gets called multiple times for the same gene?
* If I think about this logically, pawns are expected to get genes added over time, but traits usually only exist at character creation. So probably things get wonky if you add a trait but the character had a gene that was in conflict with it.

## Major Bugs

* Flatten called as ability, does not return to main menu
* Retune called as ability, then hit skip does not return to main menu
* manual flatten doesn't return to the main menu
* manual attentuate doesn't return to the main menu
* Handle spectrum traits properly (acquiring goes through the spectrum like how it does for Corpse Children)

## Minor Bugs

* Excise isn't lethal if target is self, but shows a lethal icon. This is a general fix I need to make to the gui and the Props to make it hand things out properly.
* saw a scenario where amplifying to samhaphage did not give you the frame ability
* PawnApplyInfestFluxspawnHiveling doesn't handle population limit
