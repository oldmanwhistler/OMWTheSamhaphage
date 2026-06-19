# BUGS

## Fuckin' traits man

* Why does character editor show a million psychopath traits... need to check save game data.. and it also has a million Psychopaths. Fuck.
* the conflictsWith logic for traits seems like it isn't by-directional? perhaps scan the entire defdatabase at the start and build up my own list of conflicting traits
* Specifically a problem with scoured mind I think because it is trying to affect multiple traits so it pingpongs?

## Major Bugs

* Flatten does not return to main menu (did I fix this?)
* Retune called as ability, then hit skip does not return to main menu

## Minor Bugs

* Excise isn't lethal if target is self, but shows a lethal icon. This is a general fix I need to make to the gui and the Props to make it hand things out properly.
