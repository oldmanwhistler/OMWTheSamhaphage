# Dev Diary

## Week #1 - Feb 16th 2026

I start using the chat interface on grok. I've used Grok Imagine in the past for meme/image generation because it does 8 images at a time so it's a bit faster to get to what you want. Grok was okay for images and text generation, so-so on text generation with the classic absurd usage of the em-dash everywhere.

It was really bad for asking XML definition questions, no concept of game version or prioritizing new information over old information.

My focus was new Xenotypes as custom xenotypes and ended with exporting them as mod xenotypes. After the initial export I was I editing genes using Notepad++.

The original xenotype names were Corpse Children, Corpseborn, Fluxspawn, Samhaphage, Samhanophage.

I did some grind-house style movie posters and was playing around with adding screenshots of the in-game graphics on top of them before realizing I should wait until the mod is complete. See the DevDiary/grok_grindhouse directory for the posters.

I switch to using the chat interface of gemini 3.1 with Notepad++ for editing XML. This felt like pair programming with much better brainstorming, idea generation, helping me change previously written down ideas and generate more cohesion in terminology. When it came to creative writing it felt really collaborative and it was very good at suggesting a next task or extending ideas based on what I had done previously. It wasn't as bad as grok with the XML but it still sucked.

I created some custom genes for the first time. It was better at XML definitions but would still hallucinate what different fields meant (e.g. gemini told me DisplayInCategory meant priority of genes in an ExcludeTag). Finally figured out how to get the Xenotype Editor button on the main menu (the 1.6 fork is broken, so use 1.5). I'm using XenoPreview to pick genes from the xenotype editor and cut-and-paste them to a scratchpad and then to my mod XenotypeDefs.

The new xenotype names were Echovessel, Hallowbound, Fluxspawn, Samhaphage, Sovereign Stillness.

I had gemini write out some mod rules based on my human-readable lore document so it's easier to prime the lore in new chats.

## Week #2 - Feb 23rd 2026

The next exploration was moving from Notepad++ to using VSCode with Claude Haiku 4.5. I set up a proper workspace with the game definitions and *only* 1.6 definitions for the parent genetic mods. Dang, always start with using a proper IDE. Proper indentation and all kinds of extensions like spellcheck. I really like using AI to search for XML definitions amongst the mods I have installed as it also EXPLAINS the definitions for me. Not having used the stuff before I blow through my GitHub Copilot requests in an hour but in that time I got it to build some C# that "actually compiles" without looking at the source code. Vibe coding achieved but it took 6-10 iterations to compile and it doesn't work. Looking up more about how requests and tokens work is pretty confusing.

When I start looking at "vibe coded" output I think this is the wrong approach. I'm better off with modifying functionality from existing mods since this isn't a "green field" project. There are good examples of things I'm trying to do. I want to modify existing mod functionality to match the lore in my head.

I set up a scenario that starts with all the modded races so I can quickly create a new world for testing things. The testing is taking too long because it's a fresh world and some of the xenotypes have very low survivability without the right temperature. I go into creative mode and build up a structure for a starting base that thematically fits the vibe. I spend way too long playing with Blueprint and exporting / importing, and I still don't have it right. Where did the doors go?

## Week #3 - March 2nd, 2026

I switch my documentation from text files to markdown files to more fit in with the VS Code / AI / GitHub conventions. I grab the GitHub source code for AlphaGenes, Big and Small, and Outland Genetics since the mods don't include the C#. I Started working on converting the parasitic implanter from AG so that it takes a xenotype string from the AbilityDef. The parasitic implanter code doesn't quite fix my headcanon because the fluxspawn should birth litters which leads me down a path of them having a combination of xenogenetic implanter that also causes pregnancy.

I spend more time messing around with my testing scenario to get more ideas for features I want. I do a couple of two hour runs to get a feel for how the existing genes work, etc.

## Week #4 - March 9th, 2026

I get frustrated with the debug loop of having to load RimWorld to find out I forgot to close an XML tag. I look at getting rwxml-language-server working. Doesn't seem like it's doing anything so that was a waste of time.

I've given up on vibe coding, and I'm basing C# code off of other mods like AlphaGenes and WVC. I still use gemini for asking C# questions but anyone who thinks that is vibe coding can eat a bag of dicks. I fix up the PregnancyAbility more and create a new xenotype called Cradlemold to fit into the reproductive cycle and so it seems more like an alien/inject parasite movie and less like #JustRimWorldThings.

I notice that AG Random Mutations are always removed when there is a new batch which doesn't match what I'm trying to accomplish with fluxspawn being genetic mutation engines. What I want for the game loop is the samhaphages can "retune" someone's xenogenes to endogenes so they are kept, or the Hallowbound can kill a fluxspawn and use the BS Mimic
ability to acquire the xenogenes. To reinforce the idea that fluxspawn are fast breeding sources of genes/traits for the other xenotypes to evolve.

I try out the BS Parasite genes again and it seems like a pretty valid mechanism for keeping your fluxspawn alive for the early game. Just stash them in another pawn when it gets chilly. This is changing my original idea of the brute/spitters being minor variations of the fluxspawn. Maybe they are an intermediary stage between the fluxspawn and the Hallowbound? I want it to be possible to do a full run from fluxspawn to Hallowbound to samhaphage to sovereign stillness cuz that sounds neat.

Gemini is the friggin GOAT for sitting down and iterating over the nomenclature. A fucking idiot for generating XML and C#, but brainstorming is fun. Yeah I'm talking about you, gemini. It's <biostatMet> not <biostats><metabolism>. TryResurrecting not Resurrect.

I discovered a JetBrains plugin I can get working with VS Code that was last released Dec 2025. YES. And although it's a pain-in-the-ass to figure out how to use it to disassemble dlls it means I can finally try to understand mods that have no source code. I start looking how GeneRipper mod works, which leads me to all the custom UI stuff in WVC. (It's crazy when I think about how much work must have went into it compared to how long it took me to do the little I have done so far). Also this means I'll finally be able to figure out how to add more traits to Corpse Children.

I have come to the same conclusion that probably happened for WVC: trying to set up a lot of new abilities freaking sucks when it requires editing XML files on top of the C#. At least with C# you get compile errors unlike with XML. So back to the drawing board. I need to create a generic menu system for applying actions. Check self.xenotype and if target is self, pawn, hostile or corpse and then load different "actions" (just function calls triggered from a menu) based on the combination. It will be a single gene and a single ability gizmo. I'll be able to easily have a hierarchy of what each of the different xenotypes do and move them around. I should be able to create a reusable UI for selecting a gene from a list or just displaying the genes before doing an action. This should also be the best possible performance since it requires user interaction to run vs adding stuff going on in the background.

## Week #5 - March 16th, 2026

I'm feeling burned out from working overtime at work. This mod development is taking so long and I want to just play. I get more motivated after doing a 3 hour play test. Because I'm using this menu system for abilities there is no cool-down mechanics on abilities so it's too OP. I'm trying to balance it out by having suicidal abilities and destroying corpses so they can't be used multiple times. I might go back to AbilityDefs once I have everything coded up and working. Just stick the abilities on the main gene for each xenotype so I don't have to mess around with a gene per ability and so I don't have to deal with testing combinations.

This approach of having one gene with all the abilities has a big pay-off: I can test the changes using the same save file which shortens the efforts to recreate scenarios.

## Week #6 - March 23rd, 2026

I found another way to balance the mod -- carcinomas. I was already having the Fluxspawn use the teratogenic genes from AlphaGenes and the Taukai. The carcinomas can be a resource to activate different abilities like the fluxspawn's ability to shift between hiveling, brute and flicker. I'm also changing the original spitters to flickers with the idea that they can move very fast to stun and then parasite foes. The spitter "range only" fluxspawn didn't work well without weapons.

I ended up doing a big refactor for the menu system for the abilities.

## Week #7 - March 30th, 2026

I'm getting burned out on this project and I haven't played RimWorld in almost two months. My attempts at generating in-game graphics for the abilities is not going well. I try a bunch of pixel art AI generators and they all kind of such or have shady monetization practices. I find a prompt that works ok with gemini and then just use Paint/Copilot to remove backgrounds and paint.net to resize it. I'm feeling better at having some assets now. I start added carcinomas as resources based on Taukai race concepts; once I know more about how to properly use resources I should add this back to AlphaGenes. The existing code in AlphaGenes wipes out all carcinomas when the ability is used instead of treating it like a resource.

## Week #8 - April 6th, 2026

Overtime at work; no RimWorld.

## Week #9 - April 13th, 2026

Overtime at work; no RimWorld.

## Week #10 - April 20th, 2026

Overtime at work; no RimWorld.

## Week #11 - April 27th, 2026

I want to get this project done "enough" that I can start playing again. It's been three months of no RimWorld. I have a big refactor ahead of me. I've already set up some classes around abilities to reduce all of the boilerplate code every ability had. I also want to try out BetterFloatMenu to use icons for abilities rather than a boring FloatMenu. I end up tweaking it, yay MIT license.

## Week #12 - May 4th, 2026

I finished the refactoring that was causing me not to work on this.

- Everything is using the same NullThrumAbility classes
- Changed "Verb" to selector since Verb is already a nomenclature in base RimWorld
- Menu system is almost all icons now
- Doing more playtests... it's definitely too fiddly where you need to apply the same commands all the time.
- Acquiring resonance to power the abilities without fluxspawn is very challenging, but when you have fluxspawn it's almost too easy.

I had my first experience of hallucination in `Gemini Code Assist` where it suggested a non-existent `<AbilityDef><targetParams><NeverTargetHostile>` but then when I followed up with a question if that exists, it manages to look at my `TODO.md` tracker to figure out what I was working on, know the bug I was trying to solve and then give me up a bunch of good suggestions for how to fix it. Oh wait, it hallucinates `canTargetMutants`.

In my playtesting I am getting a Crash-To-Desktop with no logging. I need to get smarter about using RimSort's save/open modlists to go back and forth between debugging and trying out bigger modlists. I end up spending a few hours getting back to a stable base of performance mods.

See [OMW Performance Collection on Steam](https://github.com/oldmanwhistler/omw-modlists/blob/main/optimization.md)

## Week #13 - May 11th, 2026

I'm not happy with how using the ability "feels". It's better after changing the job so it makes the target pawn wait, and then fixing a problem where one of the menus doesn't pause. This week is a *lot* of bug fixing and playtesting. Add some new abilities: compress, unmute, mute, bootleg, crosstalk.

Finally added the settings menu and started attaching all the constant values in the classes to the settings menu. Also added the various exports I need to debug game balance. 

I found a pretty funny bug with Gemini, it always breaks when it comes to applying changes to the Bootleg ability... I wonder if "bootleg" is tripping the guardrails all the time.

## Week #14 - May 18th, 2026

So many bugfixes. I'm doing a big multi-mod / trait playtest. I find some crash-to-desktop bugs with all the combinations. Some traits really have issues when there is a gene that can apply them. Like the entire _Pathway To Ascension_ mod just straight up kills pawns when they get the trait without doing the correct method.

## Week #15 - May 25th, 2026

This is a "neat" bug. The _Defensive Fighter_ from _Traits Plus_ causes CTD when stealing the gene with Harrow. I can get the same CTD to happen when I use Character Editor to apply the trait directly. Is the CTD is happening if the Defensive Fighter trait is added while a pawn is drafted? Kind of makes sense, and the might be a general issue that can happen when adding genes/traits to drafted pawns. I put in some code to prevent the abilities from being used while drafted, but then when I tried again it still does CTD.

It's a head-scratcher. When I open up the mod the trait is in, I see from the def that this one specifically that is causing a CTD uses `<disabledWorkTags>`. That might be the culprit? I disable it and reload and it isn't, but looking at the Pawn who was trying to take the trait, and they already have another gene that gives a conflicting trait. (Defensive Fighter vs Bloodlust) so I am guessing that is the CTD? Remove Bloodlist, try adding Defensive Fighter and... CTD. ARGH.

... and this is the story about why I'm just going to ban stealing every gene that has a trait and call it a day.

## Week #16 - June 1st, 2026

My son bought me "Crimson Desert" and my gf wants to watch me collect cats so this is going to slow me down. Still, good progress on bugfixes. Got trait blacklisting working. I gave up on creating shamblers and I'm just going for destroying corpses instead.

I made the mistake of vibe coding a ton of changes for the UI control my abilties and now everything is fucked. 43 file changes to debug. YAAAAAY. There's like 5 hours down the drain and just a bunch of reverting to get it to work.

It's a common pattern I've seen with LLMs where they don't realize there is abstraction/common methods to use and they do a change like 4 different ways.

## Week #17 - June 8th, 2026

Finally finished the UI stuff and I have a better understanding of the async nature of jumping between menus. It's a big improvement having it always return back to the main ability menu. More refactoring of the blacklist genes so that it's easier to read and I added the blacklist reason to the tool tips when you are in the gene selection menus.

## Week #18 - June 15th, 2026

Holy crap, using Gemini Code Assist had another one of those moments where it went completely off the rails. I need to bite the bullet and get Claude.

## Week #19 - June 22nd, 2026

I took a break and tried out ModMixer on a small idea I had: [Shiny Math Rocks](https://steamcommunity.com/sharedfiles/filedetails/?id=3747009272). I was happy with the result. I tried out GPT and while it ran out of the free tier it got the job done really fast and the coding was pretty much a one shot. Sure I spent another 8 hours f'n around and making dice, but I liked the workflow it had for mod testing and it made publishing very simple. ModMixer also has good discord community. 

After that I tried some small ModMixer stuff with the Gemini free tier and it was friggin' awful. Seems like Gemini just isn't very good.

I'm not a huge fan of generating code via CLI instead of via IDE (cuz I am an old fogey who needs to read and modify the code. Vibe coding is hard to adapt to). I'd love to have ModMixer as an MCP in my IDE. The way it disassembles the main game to get the APIs and internals plus gives the LLM a loop where it can self-correct syntax errors is **chef's kiss** .

For VS Code I switched back to GitHub Copilot free after the removed Gemini support for it. I'm using the auto-completions as I type now. I find writing out the comment of what I plan to do usually give a helpful suggestion for the function that will implement it, almost better than the using the agentic chat that can go off the rails too easily when it hallucinates syntax.

Pretty big bug fix to the Bootleg ability that steals traits. It lets you "upgrade" existing spectrum traits. I took a look at the trait counts and the "1 million psychopaths" problem is fixed as well. I think it was a state that could happen when I allowed a trait that was blocked by a gene.

## Week #20 - June 27th, 2026

