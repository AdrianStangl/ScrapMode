using BepInEx;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using System.Linq;
using System;
using UnityEngine;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace ScrapMode
{
    // This is an example plugin that can be put in
    // BepInEx/plugins/ExamplePlugin/ExamplePlugin.dll to test out.
    // It's a small plugin that adds a relatively simple item to the game,
    // and gives you that item whenever you press F2.

    // This attribute specifies that we have a dependency on a given BepInEx Plugin,
    // We need the R2API ItemAPI dependency because we are using for adding our item to the game.
    // You don't need this if you're not using R2API in your plugin,
    // it's just to tell BepInEx to initialize R2API before this plugin so it's safe to use R2API.
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency("com.bepis.r2api")]
    // This one is because we use a .language file for language tokens
    // More info in https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Assets/Localization/
    [BepInDependency(LanguageAPI.PluginGUID)]

    // This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    // This is the main declaration of our plugin class.
    // BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    // BaseUnityPlugin itself inherits from MonoBehaviour,
    // so you can use this as a reference for what you can declare and use in your plugin class
    // More information in the Unity Docs: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    public class ScrapMode : BaseUnityPlugin
    {
        // The Plugin GUID should be a unique ID for this plugin,
        // which is human readable (as it is used in places like the config).
        // If we see this PluginGUID as it is on thunderstore,
        // we will deprecate this mod.
        // Change the PluginAuthor and the PluginName !
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "adrian";
        public const string PluginName = "ScrapMode";
        public const string PluginVersion = "1.0.0";

        // We need our item definition to persist through our functions, and therefore make it a class field.
        private static ArtifactDef myArtifact;

        // Multiply the TOTAL number of spawnable interactibles.
        public static ConfigEntry<float> InteractibleCountMultiplier { get; set; }

        // Config Entries for every item
        #region INTERACTABLECONFIGS
        public static ConfigEntry<float> IscChest1 { get; set; }

        public static ConfigEntry<float> IscChest2 { get; set; }

        public static ConfigEntry<float> IscChest1Stealthed { get; set; }

        public static ConfigEntry<float> IscCategoryChestDamage { get; set; }

        public static ConfigEntry<float> IscCategoryChestHealing { get; set; }

        public static ConfigEntry<float> IscCategoryChestUtility { get; set; }

        public static ConfigEntry<float> IscCategoryChest2Damage { get; set; }

        public static ConfigEntry<float> IscCategoryChest2Healing { get; set; }

        public static ConfigEntry<float> IscCategoryChest2Utility { get; set; }

        public static ConfigEntry<float> IscGoldChest { get; set; }

        public static ConfigEntry<float> IscTripleShop { get; set; }

        public static ConfigEntry<float> IscTripleShopLarge { get; set; }
        public static ConfigEntry<float> IscTripleShopEquipment { get; set; }
        public static ConfigEntry<float> IscCasinoChest { get; set; }
        public static ConfigEntry<float> IscBarrel1 { get; set; }
        public static ConfigEntry<float> IscLunarChest { get; set; }
        public static ConfigEntry<float> IscEquipmentBarrel { get; set; }
        public static ConfigEntry<float> IscScrapper { get; set; }
        public static ConfigEntry<float> IscRadarTower { get; set; }
        public static ConfigEntry<float> IscDuplicator { get; set; }
        public static ConfigEntry<float> IscDuplicatorLarge { get; set; }
        public static ConfigEntry<float> IscDuplicatorWild { get; set; }
        public static ConfigEntry<float> IscDuplicatorMilitary { get; set; }
        public static ConfigEntry<float> IscBrokenTurret1 { get; set; }
        public static ConfigEntry<float> IscBrokenDrone1 { get; set; }
        public static ConfigEntry<float> IscBrokenDrone2 { get; set; }
        public static ConfigEntry<float> IscBrokenEmergencyDrone { get; set; }
        public static ConfigEntry<float> IscBrokenMissileDrone { get; set; }
        public static ConfigEntry<float> IscBrokenEquipmentDrone { get; set; }
        public static ConfigEntry<float> IscBrokenFlameDrone { get; set; }
        public static ConfigEntry<float> IscBrokenMegaDrone { get; set; }
        public static ConfigEntry<float> IscShrineChance { get; set; }
        public static ConfigEntry<float> IscShrineCombat { get; set; }
        public static ConfigEntry<float> IscShrineBlood { get; set; }
        public static ConfigEntry<float> IscShrineBoss { get; set; }
        public static ConfigEntry<float> IscShrineHealing { get; set; }
        public static ConfigEntry<float> IscShrineRestack { get; set; }
        public static ConfigEntry<float> IscShrineGoldshoresAccess { get; set; }
        public static ConfigEntry<float> IscShrineCleanse { get; set; }
        public static ConfigEntry<float> IscVoidCamp { get; set; }
        public static ConfigEntry<float> IscVoidChest { get; set; }
        public static ConfigEntry<float> IscVoidTriple { get; set; }
        public static ConfigEntry<float> IscVoidCoinBarrel { get; set; }
        #endregion INTERACTABLECONFIGS

        private static readonly List<string> Interactibles = new List<string>
        {
            "iscChest1",
            "iscChest2",
            "iscChest1Stealthed",
            "iscCategoryChestDamage",
            "iscCategoryChestHealing",
            "iscCategoryChestUtility",
            "iscCategoryChest2Damage",
            "iscCategoryChest2Healing",
            "iscCategoryChest2Utility",
            "iscGoldChest",
            "iscTripleShop",
            "iscTripleShopLarge",
            "iscTripleShopEquipment",
            "iscCasinoChest",
            "iscBarrel1",
            "iscLunarChest",
            "iscEquipmentBarrel",
            "iscScrapper",
            "iscRadarTower",
            "iscDuplicator",
            "iscDuplicatorLarge",
            "iscDuplicatorWild",
            "iscDuplicatorMilitary",
            "iscBrokenTurret1",
            "iscBrokenDrone1",
            "iscBrokenDrone2",
            "iscBrokenEmergencyDrone",
            "iscBrokenMissileDrone",
            "iscBrokenEquipmentDrone",
            "iscBrokenFlameDrone",
            "iscBrokenMegaDrone",
            "iscShrineChance",
            "iscShrineCombat",
            "iscShrineBlood",
            "iscShrineBoss",
            "iscShrineHealing",
            "iscShrineRestack",
            "iscShrineGoldshoresAccess",
            "iscShrineCleanse",
            "iscVoidCamp",
            "iscVoidChest",
            "iscVoidCoinBarrel"
        };

        // Dict holding ConfigEntry and default value for an interactable name
        public static Dictionary<string, (ConfigEntry<float> entry, float defaultValue)> InteractibleToBind = new Dictionary<string, (ConfigEntry<float> entry, float defaultValue)>()
        {
            // Chests
            { "iscChest1", (IscChest1, 1.0f) },
            { "iscChest2", (IscChest2, 1.0f) },
            { "iscChest1Stealthed", (IscChest1Stealthed, 0.0f) },
            { "iscCategoryChestDamage", (IscCategoryChestDamage, 0.0f) },
            { "iscCategoryChestHealing", (IscCategoryChestHealing, 0.0f) },
            { "iscCategoryChestUtility", (IscCategoryChestUtility, 0.0f) },
            { "iscCategoryChest2Damage", (IscCategoryChest2Damage, 0.0f) },
            { "iscCategoryChest2Healing", (IscCategoryChest2Healing, 0.0f) },
            { "iscCategoryChest2Utility", (IscCategoryChest2Utility, 0.0f) },
            { "iscGoldChest", (IscGoldChest, 1.0f) },
            // Shops
            { "iscTripleShop", (IscTripleShop, 0.0f) },
            { "iscTripleShopLarge", (IscTripleShopLarge, 0.0f) },
            { "iscTripleShopEquipment", (IscTripleShopEquipment, 0.0f) },
            // Addaptive chest
            { "iscCasinoChest", (IscCasinoChest, 1.0f) },
            // Barrell
            { "iscBarrel1", (IscBarrel1, 1.0f) },
            // Misc
            { "iscLunarChest", (IscLunarChest, 1.0f) },
            { "iscEquipmentBarrel", (IscEquipmentBarrel, 1.0f) },
            { "iscScrapper", (IscScrapper, 1.0f) },
            { "iscRadarTower", (IscRadarTower, 1.0f) },
            // Printer
            { "iscDuplicator", (IscDuplicator, 1.0f) },
            { "iscDuplicatorLarge", (IscDuplicatorLarge, 1.0f) },
            { "iscDuplicatorWild", (IscDuplicatorWild, 1.0f) },
            { "iscDuplicatorMilitary", (IscDuplicatorMilitary, 1.0f) },
            // Drones
            { "iscBrokenTurret1", (IscBrokenTurret1, 1.0f) },
            { "iscBrokenDrone1", (IscBrokenDrone1, 1.0f) },
            { "iscBrokenDrone2", (IscBrokenDrone2, 1.0f) },
            { "iscBrokenEmergencyDrone", (IscBrokenEmergencyDrone, 1.0f) },
            { "iscBrokenMissileDrone", (IscBrokenMissileDrone, 1.0f) },
            { "iscBrokenEquipmentDrone", (IscBrokenEquipmentDrone, 1.0f) },
            { "iscBrokenFlameDrone", (IscBrokenFlameDrone, 1.0f) },
            { "iscBrokenMegaDrone", (IscBrokenMegaDrone, 1.0f) },
            // Shrines
            { "iscShrineChance", (IscShrineChance, 0.0f) },
            { "iscShrineCombat", (IscShrineCombat, 0.0f) },
            { "iscShrineBlood", (IscShrineBlood, 0.0f) },
            { "iscShrineBoss", (IscShrineBoss, 1.5f) },
            { "iscShrineHealing", (IscShrineHealing, 0.0f) },
            { "iscShrineRestack", (IscShrineRestack, 0.0f) },
            { "iscShrineGoldshoresAccess", (IscShrineGoldshoresAccess, 0.0f) },
            { "iscShrineCleanse", (IscShrineCleanse, 0.0f) },
            { "iscVoidCamp", (IscVoidCamp, 0.0f) },
            { "iscVoidChest", (IscVoidChest, 0.0f) },
            { "iscVoidTriple", (IscVoidTriple, 0.0f) },
            { "iscVoidCoinBarrel", (IscVoidCoinBarrel, 0.0f) }
        };

        // Translate interactable name to readable name for config
        private static readonly Dictionary<string, string> InteractibleToLocalized = new Dictionary<string, string>
        {
            {"iscChest1", "Small Chest"},
            {"iscChest2", "Large Chest"},
            {"iscChest1Stealthed", "Invisible Chest"},
            {"iscCategoryChestDamage", "Damage Chest"},
            {"iscCategoryChestHealing", "Healing Chest"},
            {"iscCategoryChestUtility", "Utility Chest"},
            {"iscCategoryChest2Damage", "Large Damage Chest"},
            {"iscCategoryChest2Healing", "Large Healing Chest"},
            {"iscCategoryChest2Utility", "Large Utility Chest"},
            {"iscGoldChest", "Legendary Chest"},
            {"iscTripleShop", "Common Triple Shop"},
            {"iscTripleShopLarge", "Uncommon Triple Shop"},
            {"iscTripleShopEquipment", "Triple Equipment Shop"},
            {"iscCasinoChest", "Adaptive Chest"},
            {"iscBarrel1", "Barrel"},
            {"iscLunarChest", "Lunar Pod"},
            {"iscEquipmentBarrel", "Equipment Barrel"},
            {"iscScrapper", "Scrapper"},
            {"iscRadarTower", "Radio Scanner"},
            {"iscDuplicator", "Common 3D Printer"},
            {"iscDuplicatorLarge", "Uncommon 3D Printer"},
            {"iscDuplicatorWild", "Overgrown 3D Printer"},
            {"iscDuplicatorMilitary", "Mili-Tech Printer"},
            {"iscBrokenTurret1", "Gunner Turret"},
            {"iscBrokenDrone1", "Gunner Drone"},
            {"iscBrokenDrone2", "Healing Drone"},
            {"iscBrokenEmergencyDrone", "Emergency Drone"},
            {"iscBrokenMissileDrone", "Missile Drone"},
            {"iscBrokenEquipmentDrone", "Equipment Drone"},
            {"iscBrokenFlameDrone", "Incinerator Drone"},
            {"iscBrokenMegaDrone", "TC-280 Prototype"},
            {"iscShrineChance", "Shrine of Chance"},
            {"iscShrineCombat", "Shrine of Combat"},
            {"iscShrineBlood", "Shrine of Blood"},
            {"iscShrineBoss", "Shrine of the Mountain"},
            {"iscShrineHealing", "Shrine of the Woods"},
            {"iscShrineRestack", "Shrine of Order"},
            {"iscShrineGoldshoresAccess", "Altar of Gold"},
            {"iscShrineCleanse", "Cleansing Pool"},
            {"iscVoidCamp", "Void Seed"},
            {"iscVoidChest", "Void Cradle"},
            {"iscVoidTriple", "Void Potential"},
            {"iscVoidCoinBarrel", "Void Stalk"}
        };

        public void Awake()
        {
            InnitArtifact();
            ConfigSetup();

            On.RoR2.Run.BuildDropTable += Run_BuildDropTable;
            On.RoR2.SceneDirector.GenerateInteractableCardSelection += SceneDirector_GenerateInteractableCardSelection;
        }

        /// <summary>
        /// Setsup everything for the artifact and adds it to the artifact list
        /// </summary>    
        private void InnitArtifact()
        {
            myArtifact = ScriptableObject.CreateInstance<ArtifactDef>();
            myArtifact.nameToken = "Scrapmode";
            myArtifact.descriptionToken = "A new gamemode. Collect scrap, print items";
            myArtifact.smallIconDeselectedSprite = LoadIcon(Properties.Resources.artifactDeselect);
            myArtifact.smallIconSelectedSprite = LoadIcon(Properties.Resources.artifactSelect);

            ArtifactCatalog.getAdditionalEntries += (list) =>
            {
                list.Add(myArtifact);
            };
        }

        private void ConfigSetup()
        {
            Logger.LogInfo("starting config for scrap mode...");
            if (!RunArtifactManager.instance.IsArtifactEnabled(myArtifact))
            {
                return;
            }
            Logger.LogInfo("Artifact on, start binding");
            InteractibleCountMultiplier = base.Config.Bind<float>("!General", "Count multiplier", 1f, new ConfigDescription("Multiply the TOTAL number of spawnable interactibles. (Capped at 100).", null, Array.Empty<object>()));
            Logger.LogInfo("Count Multiplier binded");
            foreach (string key in Interactibles)
            {
                Logger.LogInfo($"binding {key}!");
                InteractibleToBind[key] = 
                    (
                    entry: base.Config.Bind<float>("Interactables", 
                           InteractibleToLocalized[key], 
                           InteractibleToBind[key].defaultValue, 
                           new ConfigDescription($"Multiply the weighted chance to spawn a/an {InteractibleToLocalized[key]}.", null, Array.Empty<object>())),
                    defaultValue: InteractibleToBind[key].defaultValue
                    );
            }

            Logger.LogInfo("Config loaded!");
        }

        /// <summary>
        /// Puts the scrap on the correct drop table
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        private void Run_BuildDropTable(On.RoR2.Run.orig_BuildDropTable orig, Run self)
        {
            orig(self);
            // Artifact is off
            if (!RunArtifactManager.instance.IsArtifactEnabled(myArtifact))
            {
                return;
            }

            // Store the scrap Itemindexes of all classes
            ItemIndex whiteScrap = RoR2Content.Items.ScrapWhite.itemIndex;
            ItemIndex greenScrap = RoR2Content.Items.ScrapGreen.itemIndex;
            ItemIndex redScrap = RoR2Content.Items.ScrapRed.itemIndex;
            ItemIndex yellowScrap = RoR2Content.Items.ScrapYellow.itemIndex;

            // Add only scrap to the propper Item Tier list
            self.availableTier1DropList.Clear();
            self.availableTier2DropList.Clear();
            self.availableTier3DropList.Clear();
            self.availableBossDropList.Clear();
            self.availableTier1DropList.Add(PickupCatalog.FindPickupIndex(whiteScrap));
            self.availableTier2DropList.Add(PickupCatalog.FindPickupIndex(greenScrap));
            self.availableTier3DropList.Add(PickupCatalog.FindPickupIndex(redScrap));
            self.availableBossDropList.Add(PickupCatalog.FindPickupIndex(yellowScrap));

            // Add the list to the chests with its probabilities
            self.smallChestDropTierSelector.Clear();
            self.smallChestDropTierSelector.AddChoice(self.availableTier1DropList, 0.8f);
            self.smallChestDropTierSelector.AddChoice(self.availableTier2DropList, 0.2f);
            self.smallChestDropTierSelector.AddChoice(self.availableTier3DropList, 0.01f);
            self.mediumChestDropTierSelector.Clear();
            self.mediumChestDropTierSelector.AddChoice(self.availableTier2DropList, 0.8f);
            self.mediumChestDropTierSelector.AddChoice(self.availableTier3DropList, 0.2f);
            self.largeChestDropTierSelector.Clear();
        }

        /// <summary>
        /// Changes the weights of the interactables. Influences the amount of interactables spawned in the scene
        /// </summary>
        /// <returns>Returns the weighted selection card deck with the new weights</returns>
        private WeightedSelection<DirectorCard> SceneDirector_GenerateInteractableCardSelection(On.RoR2.SceneDirector.orig_GenerateInteractableCardSelection orig, SceneDirector self)
        {
            Logger.LogInfo("Started Generating Interactable CardSelection...");
            self.interactableCredit = (int)((float)self.interactableCredit * Mathf.Clamp(1f, 0f, 100f));
            WeightedSelection<DirectorCard> weightedSelection = orig(self);
            Logger.LogInfo("Called the original method for Generating Interactable CardSelection!");

            // Artifact is off
            if (!RunArtifactManager.instance.IsArtifactEnabled(myArtifact))
            {
                return weightedSelection;
            }
            Logger.LogInfo("Scrapmode Artifact is on!");
            for (int i = 0; i < weightedSelection.Count; i++)
            {
                bool flag;
                if (weightedSelection == null)
                {
                    flag = false;
                }
                else
                {
                    WeightedSelection<DirectorCard>.ChoiceInfo[] choices = weightedSelection.choices;
                    flag = true;
                }
                bool flag2 = flag;
                if (flag2)
                {
                    Logger.LogInfo("Weighted selection exists!");
                    string name = weightedSelection.choices[i].value.spawnCard.name;
                    (ConfigEntry<float> entry, float defaultValue) bindEntry;
                    bool flag3 = InteractibleToBind.TryGetValue(name.Replace("Sandy", "").Replace("Snowy", ""), out bindEntry);
                    Logger.LogInfo($"Current choice is: {name} Got found: {flag3}");
                    Logger.LogInfo($"Bind entry: {bindEntry}");
                    if (flag3)   
                    {
                    //    bool flag4 = bindEntry.entry.Value < 0f;
                    //    if (flag4)
                    //    {
                    //        bindEntry.entry.Value = 0f;
                    //    }
                        WeightedSelection<DirectorCard>.ChoiceInfo[] choices2 = weightedSelection.choices;
                        int num = i;
                        Logger.LogInfo($"Default value of {name} is {bindEntry.defaultValue}");
                        choices2[num].weight = choices2[num].weight * bindEntry.defaultValue;
                    }
                }
            }
            Logger.LogInfo("Returning new weighted selection!");
            return weightedSelection;
        }

        /// <summary>
        /// Loads a Unity Sprite from resources.
        /// </summary>
        /// <param name="resourceBytes">The byte array of the resource.</param>
        /// <returns>The converted sprite</returns>
        private Sprite LoadIcon(Byte[] resourceBytes)
        {
            if (resourceBytes == null)
                throw new ArgumentNullException(nameof(resourceBytes));

            Texture2D iconTex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            iconTex.LoadImage(resourceBytes);

            return Sprite.Create(iconTex, new Rect(0f, 0f, iconTex.width, iconTex.height), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// Sends <paramref name="message"/> into the chat
        /// </summary>
        /// <param name="message">The message to be sent</param>
        private void SendChatMessage(string message)
        {
            if (NetworkUser.readOnlyInstancesList.Count > 0)
            {
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = message
                });
            }
        }
    }
}
