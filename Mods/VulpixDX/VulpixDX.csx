#load "../../Stubs/RTDX.csx"

using System;
using SkyEditor.RomEditor.Domain.Rtdx.Constants;
using SkyEditor.RomEditor.Domain.Rtdx.Structures;
using SkyEditor.RomEditor.Infrastructure;
using PortraitFlags = SkyEditor.RomEditor.Domain.Rtdx.Structures.PokemonGraphicsDatabase.PokemonGraphicsDatabaseEntry.EnabledPortraitsFlags;

Console.WriteLine("Vulpixifying starters");
var starterModel = Rom.GetStarters();
foreach (var starter in starterModel.Starters)
{
    starter.PokemonId = CreatureIndex.ROKON; // Vulpix
    starter.Move1 = WazaIndex.HIKKAKU; // Scratch
    starter.Move2 = WazaIndex.HINOKO; // Ember
    starter.Move3 = WazaIndex.SHIPPOWOFURU; // Tail Whip
    starter.Move4 = WazaIndex.DENKOUSEKKA; // Quick Attack
}

Console.WriteLine("Vulpixifying dungeon enemies");
var dungeons = Rom.GetDungeons().Dungeons;
foreach (var dungeon in dungeons)
{
    // It crashes when editing some of the dungeons, filter those out by the empty name
    if (!string.IsNullOrEmpty(dungeon.DungeonName.Trim()))
    {
        foreach (var pokemon in dungeon?.Balance?.WildPokemon?.Stats)
        {
            pokemon.CreatureIndex = CreatureIndex.ROKON;
        }
    }
}

var fixedPokemon = Rom.GetFixedPokemon().Entries;
foreach (var pokemon in fixedPokemon)
{
    pokemon.PokemonId = CreatureIndex.ROKON;
}

Console.WriteLine("Vulpixifying actors");
var actorDatabase = Rom.GetMainExecutable().ActorDatabase;
foreach (var actor in actorDatabase.ActorDataList)
{
    if (actor.PokemonIndexEditable)
    {
        actor.PokemonIndex = CreatureIndex.ROKON;
    }
}

Console.WriteLine("Vulpixifying graphics");
var graphicsDatabase = Rom.GetPokemonGraphicsDatabase();
foreach (var entry in graphicsDatabase.Entries)
{
    entry.PortraitSheetName = "rokon";
    // This doesn't seem to do anything :(
    entry.EnabledPortraits = PortraitFlags.Normal | PortraitFlags.Happy | PortraitFlags.Pain;

    // Workaround for cases where the actor Pokémon index isn't editable
    entry.ModelName = "rokon_00";
    entry.AnimationName = "4leg_beast_p2_00";
}

// Since the portrait flags don't prevent unused portraits from showing up,
// I've photoshopped the default portrait into the other slots.
Rom.WriteFile("romfs/Data/StreamingAssets/ab/rokon.ab", Mod.ReadResourceArray("Resources/rokon.ab"));

// Change all names to Vulpix so that the game doesn't show wrong names
// when actors are referenced in scripts
Console.WriteLine("Vulpixifying Pokémon names");

var creatures = Enum.GetValues(typeof(CreatureIndex)).Cast<CreatureIndex>().ToArray();
var messageBin = new MessageBinEntry(Rom.GetUSMessageBin().GetFile("common.bin"));
var strings = messageBin.Strings;

void VulpixifyString(int hash)
{
    strings[hash].Clear();
    strings[hash].Add(new MessageBinEntry.MessageBinString
    {
        Hash = hash,
        Value = "Vulpix",
    });
}

foreach (CreatureIndex creature in creatures)
{
    if (creature == default || creature == CreatureIndex.END)
    {
        continue;
    }

    int hash = (int) Enum.Parse(typeof(TextIDHash), "POKEMON_NAME__POKEMON_" + creature.ToString());
    VulpixifyString(hash);
}

Rom.GetUSMessageBin().SetFile("common.bin", messageBin.ToByteArray());
