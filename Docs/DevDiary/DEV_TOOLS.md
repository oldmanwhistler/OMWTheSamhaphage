# Dev Tools Used

## Week #1

* notepad++ and File Explorer
* [Rimsort](https://rimsort.github.io/RimSort/)
* grok 4.1 web interface
* [[1.5-1.6] Custom Xenotype Exporter Tool](https://steamcommunity.com/sharedfiles/filedetails/?id=3254345251)
* [What's That Mod](https://steamcommunity.com/sharedfiles/filedetails/?id=2258431182)  
* gemini 3.1 web interface

## Week #2

* VS Code
* [XenoPreview](https://steamcommunity.com/sharedfiles/filedetails/?id=3484461413)
* [[1.5 Fork] Xenotype And Ideology Buttons TitleScreen](https://steamcommunity.com/sharedfiles/filedetails/?id=3243233522)
  
* [Blueprints Forked 1.6](https://steamcommunity.com/sharedfiles/filedetails/?id=3525001145)

## Week #3

* [This site has a javascript based XML def library](https://rimworld.lattemacchiato.dev/), slow AF though and the def window is too small

* [Workshop walker lets you do a reverse lookup and find mods that are dependent on a mod](https://workshop-walker.disconsented.com/app/294100). Useful for finding extensions and patches.
* [AlphaGenes GitHub](https://github.com/juanosarg/AlphaGenes)
* [Big and Small GitHub](https://github.com/RedMattis/BigSmall_Framework)
* [Outland: Genetics](https://github.com/O21-Outland/Outland-Genetics)

## Week #4

* [rwxml-language-server extension for VS Code](https://github.com/1264600905/rwxml-language-server/tree/pr-rebuild). It has a fork where someone is updating it for 1.6, but I couldn't figure it out how to build an extension with it or how to get language servers to work.
* A [jetbrains plugin for RimWorld Development that you can use with VS Code](https://plugins.jetbrains.com/plugin/21728-rimworld-development-environment/versions/stable)!
  * dotnet nuget update source "RimWorldDevEnv" --source "C:\Users\USER\Documents\VSCodeDotNet\LocalPackages"

## Week #11

* I've been using Gemini and NotebookLM for D&D Campaigns, I'm going to try it out building a RimWorld Notebook for querying things with specific GitHub repos. I've ended up loading them as different projects in VSCode to search but it'll be interesting if using them as NotebookLM can give be better sources.
* Using the free Gemini Coding Assistant. Damn, should have done this from the start because it is generating code that at least compiles.
* [RimWorld Auto Documentation](https://github.com/Epicguru/Rimworld-Auto-Documentation) seems to be what was used to generate the RimWorld.lattemacchiato.dev website.
  * Getting an idea of taking that and having it spit out Markdown of the RimWorld APIs. Could set it up to run [DocFx](https://dotnet.github.io/docfx/docs/basic-concepts.html) to create Markdown while it's is creating HTML.
* The same author [EpicGuru](https://github.com/Epicguru) has a bunch of modder tools that I should investigate
  * [InGameWiki](https://github.com/Epicguru/InGameWiki/tree/master)
  * [BetterFloatMenu](https://github.com/Epicguru/BetterFloatMenu/tree/master)
    * I ended up incorporating this.
* Also came across [EditCompileReload](https://github.com/Zetrith/EditCompileReload) - which loads like a way to hotload DDLs under development which would be **great** for speeding up testing.

## Week #12

* Markdown to steam BB code .NET tool
```bash
# also needed to download .NET 7.0
dotnet tool install -g Converter.MarkdownToBBCodeSteam.Tool
```
```json
{
    // See https://go.microsoft.com/fwlink/?LinkId=733558
    // for the documentation about the tasks.json format
    "version": "2.0.0",
    "tasks": [
        {
            "label": "Convert to Steam bbcode",
            "type": "shell",
            "command": "markdown_to_bbcodesteam -i ${file} -o ${file}.bbcode",
            "group": "build"
        }
    ]
}
```

## Week #14

* [Mlie's rimworld modding tools](https://github.com/emipa606/RimworldModdingHelpers)
* [ModMixer](https://modmixer.com/) wait, is this thing saying it can test your mods?
* [RimSage MCP](https://rimsage.com/) MCP server for rimworld
* [RiMCP Hybrid](https://github.com/h7lu/RiMCP_hybrid) MCP server for rimworld
* [FlatIcons](https://www.flaticon.com/free-icons)

## Week #17 - June 8th, 2026

* [waif2x upscaler](https://github.com/Moebytes/Waifu2x-Upscaler) apparently this is good for upscaling low res pixel graphics.

## Week #20 - June 27th, 2026

* [Code Grapher](https://github.com/colbymchenry/codegraph) supports C# and lets the LLM understand the code base with less effort.
* I should try GLM 5.2 and z.ai. Although I've heard it's better the closer to in the "middle of distribution" you are for LLM tasks.

