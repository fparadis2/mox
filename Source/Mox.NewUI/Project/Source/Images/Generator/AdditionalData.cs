﻿using System;
using System.Collections.Generic;

namespace Mox.UI
{
    internal static class AdditionalData
    {
        private static readonly Dictionary<string, string> ms_landsToColor = new Dictionary<string, string>();

        public static string GetColorForLand(string landName)
        {
            return ms_landsToColor[landName];
        }

        static AdditionalData()
        {
            ms_landsToColor.Add("Abandoned Outpost", "W");
            ms_landsToColor.Add("Adarkar Wastes", "WU");
            ms_landsToColor.Add("Adventurers' Guildhouse", "A");
            ms_landsToColor.Add("Ancient Den", "W");
            ms_landsToColor.Add("Ancient Spring", "WUB");
            ms_landsToColor.Add("Ancient Tomb", "C");
            ms_landsToColor.Add("An-Havva Township", "RGW");
            ms_landsToColor.Add("Archaeological Dig", "WUBRG");
            ms_landsToColor.Add("Arena", "A");
            ms_landsToColor.Add("Aysen Abbey", "GWU");
            ms_landsToColor.Add("Bad River", "A");
            ms_landsToColor.Add("Badlands", "BR");
            ms_landsToColor.Add("Balduvian Trading Post", "R");
            ms_landsToColor.Add("Barbarian Ring", "R");
            ms_landsToColor.Add("Barren Moor", "B");
            ms_landsToColor.Add("Battlefield Forge", "RW");
            ms_landsToColor.Add("Bayou", "BG");
            ms_landsToColor.Add("Bazaar of Baghdad", "A");
            ms_landsToColor.Add("Blasted Landscape", "C");
            ms_landsToColor.Add("Blinkmoth Nexus", "C");
            ms_landsToColor.Add("Blinkmoth Well", "C");
            ms_landsToColor.Add("Bloodstained Mire", "A");
            ms_landsToColor.Add("Bog Wreckage", "B");
            ms_landsToColor.Add("Boros Garrison", "RW");
            ms_landsToColor.Add("Boseiju, Who Shelters All", "C");
            ms_landsToColor.Add("Bottomless Vault", "B");
            ms_landsToColor.Add("Brushland", "GW");
            ms_landsToColor.Add("Cabal Coffers", "B");
            ms_landsToColor.Add("Cabal Pit", "B");
            ms_landsToColor.Add("Caldera Lake", "UR");
            ms_landsToColor.Add("Castle Sengir", "UBR");
            ms_landsToColor.Add("Cathedral of Serra", "A");
            ms_landsToColor.Add("Caves of Koilos", "WB");
            ms_landsToColor.Add("Centaur Garden", "G");
            ms_landsToColor.Add("Cephalid Coliseum", "U");
            ms_landsToColor.Add("Cinder Marsh", "BR");
            ms_landsToColor.Add("City of Brass", "WUBRG");
            ms_landsToColor.Add("City of Shadows", "C");
            ms_landsToColor.Add("City of Traitors", "C");
            ms_landsToColor.Add("Cloudcrest Lake", "WU");
            ms_landsToColor.Add("Cloudpost", "C");
            ms_landsToColor.Add("Coastal Tower", "WU");
            ms_landsToColor.Add("Contested Cliffs", "C");
            ms_landsToColor.Add("Coral Atoll", "U");
            ms_landsToColor.Add("Crosis's Catacombs", "UBR");
            ms_landsToColor.Add("Crystal Quarry", "WUBRG");
            ms_landsToColor.Add("Crystal Vein", "C");
            ms_landsToColor.Add("Darigaaz's Caldera", "BRG");
            ms_landsToColor.Add("Darksteel Citadel", "C");
            ms_landsToColor.Add("Darkwater Catacombs", "UB");
            ms_landsToColor.Add("Daru Encampment", "C");
            ms_landsToColor.Add("Desert", "C");
            ms_landsToColor.Add("Deserted Temple", "C");
            ms_landsToColor.Add("Diamond Valley", "A");
            ms_landsToColor.Add("Dimir Aqueduct", "UB");
            ms_landsToColor.Add("Dormant Volcano", "R");
            ms_landsToColor.Add("Drifting Meadow", "W");
            ms_landsToColor.Add("Dromar's Cavern", "WUB");
            ms_landsToColor.Add("Duskmantle, House of Shadow", "UB");
            ms_landsToColor.Add("Dust Bowl", "C");
            ms_landsToColor.Add("Dwarven Hold", "R");
            ms_landsToColor.Add("Dwarven Ruins", "R");
            ms_landsToColor.Add("Ebon Stronghold", "B");
            ms_landsToColor.Add("Eiganjo Castle", "W");
            ms_landsToColor.Add("Elephant Graveyard", "C");
            ms_landsToColor.Add("Elfhame Palace", "GW");
            ms_landsToColor.Add("Everglades", "B");
            ms_landsToColor.Add("Faerie Conclave", "U");
            ms_landsToColor.Add("Flood Plain", "A");
            ms_landsToColor.Add("Flooded Strand", "A");
            ms_landsToColor.Add("Forbidden Orchard", "WUBRG");
            ms_landsToColor.Add("Forbidding Watchtower", "W");
            ms_landsToColor.Add("Forest", "G");
            ms_landsToColor.Add("Forgotten Cave", "R");
            ms_landsToColor.Add("Forsaken City", "WUBRG");
            ms_landsToColor.Add("Fountain of Cho", "W");
            ms_landsToColor.Add("Gaea's Cradle", "G");
            ms_landsToColor.Add("Gemstone Mine", "WUBRG");
            ms_landsToColor.Add("Geothermal Crevice", "BRG");
            ms_landsToColor.Add("Ghitu Encampment", "R");
            ms_landsToColor.Add("Ghost Town", "C");
            ms_landsToColor.Add("Glacial Chasm", "A");
            ms_landsToColor.Add("Glimmervoid", "WUBRG");
            ms_landsToColor.Add("Goblin Burrows", "C");
            ms_landsToColor.Add("Gods' Eye, Gate to the Reikai", "C");
            ms_landsToColor.Add("Golgari Rot Farm", "BG");
            ms_landsToColor.Add("Grand Coliseum", "WUBRG");
            ms_landsToColor.Add("Grasslands", "A");
            ms_landsToColor.Add("Great Furnace", "R");
            ms_landsToColor.Add("Griffin Canyon", "C");
            ms_landsToColor.Add("Hall of the Bandit Lord", "C");
            ms_landsToColor.Add("Halls of Mist", "A");
            ms_landsToColor.Add("Hammerheim", "R");
            ms_landsToColor.Add("Havenwood Battleground", "G");
            ms_landsToColor.Add("Heart of Yavimaya", "G");
            ms_landsToColor.Add("Henge of Ramos", "WUBRG");
            ms_landsToColor.Add("Hickory Woodlot", "G");
            ms_landsToColor.Add("High Market", "C");
            ms_landsToColor.Add("Hollow Trees", "G");
            ms_landsToColor.Add("Icatian Store", "W");
            ms_landsToColor.Add("Ice Floe", "A");
            ms_landsToColor.Add("Irrigation Ditch", "GWU");
            ms_landsToColor.Add("Island", "U");
            ms_landsToColor.Add("Island of Wak-Wak", "A");
            ms_landsToColor.Add("Jungle Basin", "G");
            ms_landsToColor.Add("Karakas", "W");
            ms_landsToColor.Add("Karoo", "W");
            ms_landsToColor.Add("Karplusan Forest", "RG");
            ms_landsToColor.Add("Keldon Necropolis", "C");
            ms_landsToColor.Add("Kjeldoran Outpost", "W");
            ms_landsToColor.Add("Kor Haven", "C");
            ms_landsToColor.Add("Koskun Keep", "BRG");
            ms_landsToColor.Add("Krosan Verge", "C");
            ms_landsToColor.Add("Lake of the Dead", "B");
            ms_landsToColor.Add("Land Cap", "WU");
            ms_landsToColor.Add("Lantern-Lit Graveyard", "BR");
            ms_landsToColor.Add("Lava Tubes", "BR");
            ms_landsToColor.Add("Library of Alexandria", "C");
            ms_landsToColor.Add("Llanowar Wastes", "BG");
            ms_landsToColor.Add("Lonely Sandbar", "U");
            ms_landsToColor.Add("Lotus Vale", "WUBRG");
            ms_landsToColor.Add("Maze of Ith", "A");
            ms_landsToColor.Add("Maze of Shadows", "C");
            ms_landsToColor.Add("Mercadian Bazaar", "R");
            ms_landsToColor.Add("Meteor Crater", "WUBRG");
            ms_landsToColor.Add("Mikokoro, Center of the Sea", "C");
            ms_landsToColor.Add("Minamo, School at Water's Edge", "U");
            ms_landsToColor.Add("Miren, the Moaning Well", "C");
            ms_landsToColor.Add("Mirrodin's Core", "WUBRG");
            ms_landsToColor.Add("Mishra's Factory", "C");
            ms_landsToColor.Add("Mishra's Workshop", "C");
            ms_landsToColor.Add("Mogg Hollows", "RG");
            ms_landsToColor.Add("Mossfire Valley", "RG");
            ms_landsToColor.Add("Mountain", "R");
            ms_landsToColor.Add("Mountain Stronghold", "A");
            ms_landsToColor.Add("Mountain Valley", "A");
            ms_landsToColor.Add("Nantuko Monastery", "C");
            ms_landsToColor.Add("Nomad Stadium", "W");
            ms_landsToColor.Add("Oasis", "A");
            ms_landsToColor.Add("Oboro, Palace in the Clouds", "U");
            ms_landsToColor.Add("Okina, Temple to the Grandfathers", "GW");
            ms_landsToColor.Add("Overgrown Tomb", "BG");
            ms_landsToColor.Add("Peat Bog", "B");
            ms_landsToColor.Add("Pendelhaven", "G");
            ms_landsToColor.Add("Petrified Field", "C");
            ms_landsToColor.Add("Phyrexian Tower", "B");
            ms_landsToColor.Add("Pine Barrens", "BG");
            ms_landsToColor.Add("Pinecrest Ridge", "RG");
            ms_landsToColor.Add("Plains", "W");
            ms_landsToColor.Add("Plateau", "RW");
            ms_landsToColor.Add("Polluted Delta", "A");
            ms_landsToColor.Add("Polluted Mire", "B");
            ms_landsToColor.Add("Quicksand", "C");
            ms_landsToColor.Add("Rainbow Vale", "WUBRG");
            ms_landsToColor.Add("Rath's Edge", "C");
            ms_landsToColor.Add("Ravaged Highlands", "R");
            ms_landsToColor.Add("Reflecting Pool", "WUBRG");
            ms_landsToColor.Add("Remote Farm", "W");
            ms_landsToColor.Add("Remote Isle", "U");
            ms_landsToColor.Add("Rhystic Cave", "WUBRG");
            ms_landsToColor.Add("Riftstone Portal", "C");
            ms_landsToColor.Add("Riptide Laboratory", "C");
            ms_landsToColor.Add("Rishadan Port", "C");
            ms_landsToColor.Add("Rith's Grove", "RGW");
            ms_landsToColor.Add("River Delta", "UB");
            ms_landsToColor.Add("Rocky Tar Pit", "A");
            ms_landsToColor.Add("Rootwater Depths", "UB");
            ms_landsToColor.Add("Ruins of Trokair", "W");
            ms_landsToColor.Add("Rushwood Grove", "G");
            ms_landsToColor.Add("Sacred Foundry", "RW");
            ms_landsToColor.Add("Safe Haven", "A");
            ms_landsToColor.Add("Salt Flats", "WB");
            ms_landsToColor.Add("Salt Marsh", "UB");
            ms_landsToColor.Add("Sand Silos", "U");
            ms_landsToColor.Add("Sandstone Needle", "R");
            ms_landsToColor.Add("Saprazzan Cove", "U");
            ms_landsToColor.Add("Saprazzan Skerry", "U");
            ms_landsToColor.Add("Savannah", "GW");
            ms_landsToColor.Add("Scabland", "RW");
            ms_landsToColor.Add("School of the Unseen", "WUBRG");
            ms_landsToColor.Add("Scorched Ruins", "C");
            ms_landsToColor.Add("Scrubland", "WB");
            ms_landsToColor.Add("Seafarer's Quay", "A");
            ms_landsToColor.Add("Seafloor Debris", "U");
            ms_landsToColor.Add("Seaside Haven", "C");
            ms_landsToColor.Add("Seat of the Synod", "U");
            ms_landsToColor.Add("Secluded Steppe", "W");
            ms_landsToColor.Add("Selesnya Sanctuary", "GW");
            ms_landsToColor.Add("Serra's Sanctum", "W");
            ms_landsToColor.Add("Shadowblood Ridge", "BR");
            ms_landsToColor.Add("Sheltered Valley", "C");
            ms_landsToColor.Add("Shinka, the Bloodsoaked Keep", "R");
            ms_landsToColor.Add("Shivan Gorge", "C");
            ms_landsToColor.Add("Shivan Oasis", "RG");
            ms_landsToColor.Add("Shivan Reef", "UR");
            ms_landsToColor.Add("Shizo, Death's Storehouse", "B");
            ms_landsToColor.Add("Skycloud Expanse", "WU");
            ms_landsToColor.Add("Skyshroud Forest", "GU");
            ms_landsToColor.Add("Slippery Karst", "G");
            ms_landsToColor.Add("Smoldering Crater", "R");
            ms_landsToColor.Add("Snow-Covered Forest", "G");
            ms_landsToColor.Add("Snow-Covered Island", "U");
            ms_landsToColor.Add("Snow-Covered Mountain", "R");
            ms_landsToColor.Add("Snow-Covered Plains", "W");
            ms_landsToColor.Add("Snow-Covered Swamp", "B");
            ms_landsToColor.Add("Soldevi Excavations", "U");
            ms_landsToColor.Add("Sorrow's Path", "A");
            ms_landsToColor.Add("Spawning Pool", "B");
            ms_landsToColor.Add("Stalking Stones", "C");
            ms_landsToColor.Add("Starlit Sanctum", "C");
            ms_landsToColor.Add("Strip Mine", "C");
            ms_landsToColor.Add("Subterranean Hangar", "B");
            ms_landsToColor.Add("Sulfur Vent", "UBR");
            ms_landsToColor.Add("Sulfurous Springs", "BR");
            ms_landsToColor.Add("Sungrass Prairie", "GW");
            ms_landsToColor.Add("Sunhome, Fortress of the Legion", "C");
            ms_landsToColor.Add("Svogthos, the Restless Tomb", "C");
            ms_landsToColor.Add("Svyelunite Temple", "U");
            ms_landsToColor.Add("Swamp", "B");
            ms_landsToColor.Add("Taiga", "RG");
            ms_landsToColor.Add("Tainted Field", "WB");
            ms_landsToColor.Add("Tainted Isle", "UB");
            ms_landsToColor.Add("Tainted Peak", "BR");
            ms_landsToColor.Add("Tainted Wood", "BG");
            ms_landsToColor.Add("Tarnished Citadel", "WUBRG");
            ms_landsToColor.Add("Teferi's Isle", "U");
            ms_landsToColor.Add("Temple Garden", "GW");
            ms_landsToColor.Add("Temple of the False God", "C");
            ms_landsToColor.Add("Tendo Ice Bridge", "WUBRG");
            ms_landsToColor.Add("Terminal Moraine", "C");
            ms_landsToColor.Add("Terrain Generator", "C");
            ms_landsToColor.Add("Thalakos Lowlands", "WU");
            ms_landsToColor.Add("Thawing Glaciers", "A");
            ms_landsToColor.Add("The Tabernacle at Pendrell Vale", "A");
            ms_landsToColor.Add("Thran Quarry", "WUBRG");
            ms_landsToColor.Add("Timberland Ruins", "G");
            ms_landsToColor.Add("Timberline Ridge", "RG");
            ms_landsToColor.Add("Tinder Farm", "RGW");
            ms_landsToColor.Add("Tolaria", "U");
            ms_landsToColor.Add("Tolarian Academy", "U");
            ms_landsToColor.Add("Tomb of Urami", "B");
            ms_landsToColor.Add("Tower of the Magistrate", "C");
            ms_landsToColor.Add("Tranquil Garden", "GW");
            ms_landsToColor.Add("Tranquil Thicket", "G");
            ms_landsToColor.Add("Tree of Tales", "G");
            ms_landsToColor.Add("Treetop Village", "G");
            ms_landsToColor.Add("Treva's Ruins", "GWU");
            ms_landsToColor.Add("Tropical Island", "GU");
            ms_landsToColor.Add("Tundra", "WU");
            ms_landsToColor.Add("Underground River", "UB");
            ms_landsToColor.Add("Underground Sea", "UB");
            ms_landsToColor.Add("Undiscovered Paradise", "WUBRG");
            ms_landsToColor.Add("Unholy Citadel", "A");
            ms_landsToColor.Add("Unholy Grotto", "C");
            ms_landsToColor.Add("Untaidake, the Cloud Keeper", "C");
            ms_landsToColor.Add("Urborg", "B");
            ms_landsToColor.Add("Urborg Volcano", "BR");
            ms_landsToColor.Add("Urza's Mine", "C");
            ms_landsToColor.Add("Urza's Power Plant", "C");
            ms_landsToColor.Add("Urza's Tower", "C");
            ms_landsToColor.Add("Vault of Whispers", "B");
            ms_landsToColor.Add("Vec Townships", "GW");
            ms_landsToColor.Add("Veldt", "GW");
            ms_landsToColor.Add("Vitu-Ghazi, the City-Tree", "C");
            ms_landsToColor.Add("Volcanic Island", "UR");
            ms_landsToColor.Add("Volrath's Stronghold", "C");
            ms_landsToColor.Add("Wasteland", "C");
            ms_landsToColor.Add("Waterveil Cavern", "UB");
            ms_landsToColor.Add("Watery Grave", "UB");
            ms_landsToColor.Add("Winding Canyons", "C");
            ms_landsToColor.Add("Windswept Heath", "A");
            ms_landsToColor.Add("Wintermoon Mesa", "C");
            ms_landsToColor.Add("Wirewood Lodge", "C");
            ms_landsToColor.Add("Wizards' School", "WUB");
            ms_landsToColor.Add("Wooded Foothills", "A");
            ms_landsToColor.Add("Yavimaya Coast", "GU");
            ms_landsToColor.Add("Yavimaya Hollow", "C");
            ms_landsToColor.Add("Orzhova, the Church of Deals", "C");
            ms_landsToColor.Add("Gruul Turf", "RG");
            ms_landsToColor.Add("Izzet Boilerworks", "UR");
            ms_landsToColor.Add("Nivix, Aerie of the Firemind", "C");
            ms_landsToColor.Add("Orzhov Basilica", "WB");
            ms_landsToColor.Add("Godless Shrine", "WB");
            ms_landsToColor.Add("Skarrg, the Rage Pits", "C");
            ms_landsToColor.Add("Steam Vents", "UR");
            ms_landsToColor.Add("Stomping Ground", "RG");
            ms_landsToColor.Add("Blood Crypt", "BR");
            ms_landsToColor.Add("Breeding Pool", "GU");
            ms_landsToColor.Add("Ghost Quarter", "C");
            ms_landsToColor.Add("Hallowed Fountain", "WU");
            ms_landsToColor.Add("Novijen, Heart of Progress", "C");
            ms_landsToColor.Add("Pillar of the Paruns", "WUBRG");
            ms_landsToColor.Add("Prahv, Spires of Order", "C");
            ms_landsToColor.Add("Rakdos Carnarium", "BR");
            ms_landsToColor.Add("Rix Maadi, Dungeon Palace", "C");
            ms_landsToColor.Add("Simic Growth Chamber", "GU");
            ms_landsToColor.Add("Azorius Chancery", "WU");
            ms_landsToColor.Add("Arctic Flats", "GW");
            ms_landsToColor.Add("Boreal Shelf", "WU");
            ms_landsToColor.Add("Dark Depths", "A");
            ms_landsToColor.Add("Frost Marsh", "UB");
            ms_landsToColor.Add("Highland Weald", "RG");
            ms_landsToColor.Add("Mouth of Ronom", "C");
            ms_landsToColor.Add("Scrying Sheets", "C");
            ms_landsToColor.Add("Tresserhorn Sinks", "BR");
            ms_landsToColor.Add("Academy Ruins", "C");
            ms_landsToColor.Add("Calciform Pools", "WU");
            ms_landsToColor.Add("Dreadship Reef", "UB");
            ms_landsToColor.Add("Flagstones of Trokair", "W");
            ms_landsToColor.Add("Fungal Reaches", "RG");
            ms_landsToColor.Add("Gemstone Caverns", "WUBRG");
            ms_landsToColor.Add("Kher Keep", "C");
            ms_landsToColor.Add("Molten Slagheap", "BR");
            ms_landsToColor.Add("Saltcrusted Steppe", "GW");
            ms_landsToColor.Add("Swarmyard", "C");
            ms_landsToColor.Add("Terramorphic Expanse", "A");
            ms_landsToColor.Add("Urza's Factory", "C");
            ms_landsToColor.Add("Vesuva", "A");
            ms_landsToColor.Add("R&D's Secret Lair", "C");
            ms_landsToColor.Add("City of Ass", "WUBRG");
            ms_landsToColor.Add("Urborg, Tomb of Yawgmoth", "B");
            ms_landsToColor.Add("Dakmor Salvage", "B");
            ms_landsToColor.Add("Graven Cairns", "BR");
            ms_landsToColor.Add("Grove of the Burnwillows", "RG");
            ms_landsToColor.Add("Horizon Canopy", "GW");
            ms_landsToColor.Add("Keldon Megaliths", "R");
            ms_landsToColor.Add("Llanowar Reborn", "G");
            ms_landsToColor.Add("New Benalia", "W");
            ms_landsToColor.Add("Nimbus Maze", "WU");
            ms_landsToColor.Add("River of Tears", "UB");
            ms_landsToColor.Add("Tolaria West", "U");
            ms_landsToColor.Add("Zoetic Cavern", "C");
            ms_landsToColor.Add("Ancient Amphitheater", "RW");
            ms_landsToColor.Add("Auntie's Hovel", "BR");
            ms_landsToColor.Add("Gilt-Leaf Palace", "BG");
            ms_landsToColor.Add("Howltooth Hollow", "B");
            ms_landsToColor.Add("Mosswort Bridge", "G");
            ms_landsToColor.Add("Secluded Glen", "UB");
            ms_landsToColor.Add("Shelldock Isle", "U");
            ms_landsToColor.Add("Shimmering Grotto", "WUBRG");
            ms_landsToColor.Add("Spinerock Knoll", "R");
            ms_landsToColor.Add("Vivid Crag", "WUBRG");
            ms_landsToColor.Add("Vivid Creek", "WUBRG");
            ms_landsToColor.Add("Vivid Grove", "WUBRG");
            ms_landsToColor.Add("Vivid Marsh", "WUBRG");
            ms_landsToColor.Add("Vivid Meadow", "WUBRG");
            ms_landsToColor.Add("Wanderwine Hub", "WU");
            ms_landsToColor.Add("Windbrisk Heights", "W");
            ms_landsToColor.Add("Murmuring Bosk", "GWB");
            ms_landsToColor.Add("Mutavault", "C");
            ms_landsToColor.Add("Primal Beyond", "WUBRG");
            ms_landsToColor.Add("Rustic Clachan", "W");
            ms_landsToColor.Add("Fire-Lit Thicket", "RG");
            ms_landsToColor.Add("Leechridden Swamp", "B");
            ms_landsToColor.Add("Madblind Mountain", "R");
            ms_landsToColor.Add("Mistveil Plains", "W");
            ms_landsToColor.Add("Moonring Island", "U");
            ms_landsToColor.Add("Mystic Gate", "WU");
            ms_landsToColor.Add("Sapseep Forest", "G");
            ms_landsToColor.Add("Sunken Ruins", "UB");
            ms_landsToColor.Add("Wooded Bastion", "GW");
            ms_landsToColor.Add("Cascade Bluffs", "UR");
            ms_landsToColor.Add("Fetid Heath", "WB");
            ms_landsToColor.Add("Flooded Grove", "GU");
            ms_landsToColor.Add("Rugged Prairie", "RW");
            ms_landsToColor.Add("Springjack Pasture", "C");
            ms_landsToColor.Add("Twilight Mire", "BG");
            ms_landsToColor.Add("Bant Panorama", "C");
            ms_landsToColor.Add("Esper Panorama", "C");
            ms_landsToColor.Add("Grixis Panorama", "C");
            ms_landsToColor.Add("Jund Panorama", "C");
            ms_landsToColor.Add("Naya Panorama", "C");
            ms_landsToColor.Add("Savage Lands", "BRG");
            ms_landsToColor.Add("Seaside Citadel", "GWU");
            ms_landsToColor.Add("Jungle Shrine", "RGW");
            ms_landsToColor.Add("Crumbling Necropolis", "UBR");
            ms_landsToColor.Add("Arcane Sanctum", "WUB");
            ms_landsToColor.Add("Ancient Ziggurat", "WURBG");
            ms_landsToColor.Add("Exotic Orchard", "WURBG");
            ms_landsToColor.Add("Reliquary Tower", "C");
            ms_landsToColor.Add("Rupture Spire", "WURBG");
            ms_landsToColor.Add("Unstable Frontier", "C");
            ms_landsToColor.Add("Dragonskull Summit", "BR");
            ms_landsToColor.Add("Drowned Catacomb", "UB");
            ms_landsToColor.Add("Gargoyle Castle", "C");
            ms_landsToColor.Add("Glacial Fortress", "WU");
            ms_landsToColor.Add("Rootbound Crag", "RG");
            ms_landsToColor.Add("Sunpetal Grove", "GW");
            ms_landsToColor.Add("Akoum Refuge", "BR");
            ms_landsToColor.Add("Crypt of Agadeem", "B");
            ms_landsToColor.Add("Emeria, the Sky Ruin", "W");
            ms_landsToColor.Add("Graypelt Refuge", "GW");
            ms_landsToColor.Add("Jwar Isle Refuge", "UB");
            ms_landsToColor.Add("Kabira Crossroads", "W");
            ms_landsToColor.Add("Kazandu Refuge", "RG");
            ms_landsToColor.Add("Magosi, the Waterveil", "U");
            ms_landsToColor.Add("Oran-Rief, the Vastwood", "G");
            ms_landsToColor.Add("Piranha Marsh", "B");
            ms_landsToColor.Add("Sejiri Refuge", "WU");
            ms_landsToColor.Add("Soaring Seacliff", "U");
            ms_landsToColor.Add("Teetering Peaks", "R");
            ms_landsToColor.Add("Turntimber Grove", "G");
            ms_landsToColor.Add("Valakut, the Molten Pinnacle", "R");
            ms_landsToColor.Add("Arid Mesa", "A");
            ms_landsToColor.Add("Marsh Flats", "A");
            ms_landsToColor.Add("Misty Rainforest", "A");
            ms_landsToColor.Add("Scalding Tarn", "A");
            ms_landsToColor.Add("Verdant Catacombs", "A");
            ms_landsToColor.Add("Bojuka Bog", "B");
            ms_landsToColor.Add("Celestial Colonnade", "WU");
            ms_landsToColor.Add("Creeping Tar Pit", "UB");
            ms_landsToColor.Add("Dread Statuary", "C");
            ms_landsToColor.Add("Halimar Depths", "U");
            ms_landsToColor.Add("Khalni Garden", "G");
            ms_landsToColor.Add("Lavaclaw Reaches", "BR");
            ms_landsToColor.Add("Raging Ravine", "RG");
            ms_landsToColor.Add("Sejiri Steppe", "W");
            ms_landsToColor.Add("Smoldering Spires", "R");
            ms_landsToColor.Add("Stirring Wildwood", "GW");
            ms_landsToColor.Add("Tectonic Edge", "C");
        }
    }
}
