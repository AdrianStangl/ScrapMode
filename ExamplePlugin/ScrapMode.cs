using BepInEx;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using System.Linq;
using System;
using UnityEngine;
using System.Collections.Generic;
using BepInEx.Configuration;
using System.Collections;

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
        public static float InteractibleCountMultiplier { get; set; } = 3f;

        // Dict holding ConfigEntry and default value for an interactable name
        public static Dictionary<string, float> InteractibleToBind = new Dictionary<string, float>()
        {
            // Chests
            { "iscChest1",  1.0f},
            { "iscChest2",  1.0f},
            { "iscChest1Stealthed",  0.0f},
            { "iscCategoryChestDamage",  0.0f},
            { "iscCategoryChestHealing",  0.0f},
            { "iscCategoryChestUtility",  0.0f},
            { "iscCategoryChest2Damage",  0.0f},
            { "iscCategoryChest2Healing",  0.0f},
            { "iscCategoryChest2Utility",  0.0f},
            { "iscGoldChest",  1.0f},
            // Shops
            { "iscTripleShop", 0.0f},
            { "iscTripleShopLarge", 0.0f},
            { "iscTripleShopEquipment", 0.0f},
            // Addaptive chest
            { "iscCasinoChest", 1.0f},
            // Barrell
            { "iscBarrel1", 1.0f},
            // Misc
            { "iscLunarChest", 1.0f},
            { "iscEquipmentBarrel", 1.0f},
            { "iscScrapper", 1.0f},
            { "iscRadarTower", 1.0f},
            // Printer
            { "iscDuplicator", 2.0f},
            { "iscDuplicatorLarge", 2.0f},
            { "iscDuplicatorWild", 2.0f},
            { "iscDuplicatorMilitary", 2.0f},
            // Drones
            { "iscBrokenTurret1", 1.0f},
            { "iscBrokenDrone1", 1.0f},
            { "iscBrokenDrone2", 1.0f},
            { "iscBrokenEmergencyDrone", 1.0f},
            { "iscBrokenMissileDrone", 1.0f},
            { "iscBrokenEquipmentDrone", 1.0f},
            { "iscBrokenFlameDrone", 1.0f},
            { "iscBrokenMegaDrone", 1.0f},
            // Shrines
            { "iscShrineChance", 0.0f},
            { "iscShrineCombat", 0.0f},
            { "iscShrineBlood", 0.0f},
            { "iscShrineBoss", 1.5f},
            { "iscShrineHealing", 0.0f},
            { "iscShrineRestack", 0.0f},
            { "iscShrineGoldshoresAccess", 0.0f},
            { "iscShrineCleanse", 0.0f},
            { "iscVoidCamp", 0.0f},
            { "iscVoidChest", 0.0f},
            { "iscVoidTriple", 0.0f},
            { "iscVoidCoinBarrel", 0.0f}
        };

        public void Awake()
        {
            InnitArtifact();

            On.RoR2.Run.BuildDropTable += Run_BuildDropTable;
            On.RoR2.SceneDirector.GenerateInteractableCardSelection += SceneDirector_GenerateInteractableCardSelection;
            On.RoR2.ChestBehavior.ItemDrop += ChestBehavior_ItemDrop;
            On.RoR2.BossGroup.DropRewards += BossGroup_DropRewards;
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

            // Add only scrap to the propper Item Tier list
            self.availableTier1DropList.Clear();
            self.availableTier2DropList.Clear();
            self.availableTier3DropList.Clear();
            self.availableTier1DropList.Add(PickupCatalog.FindPickupIndex(whiteScrap));
            self.availableTier2DropList.Add(PickupCatalog.FindPickupIndex(greenScrap));
            self.availableTier3DropList.Add(PickupCatalog.FindPickupIndex(redScrap));

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

        private void ChestBehavior_ItemDrop(On.RoR2.ChestBehavior.orig_ItemDrop orig, ChestBehavior self)
        {
            // Artifact is off
            if (!RunArtifactManager.instance.IsArtifactEnabled(myArtifact))
            {
                orig(self);
                return;
            }
            PickupIndex item = self.dropPickup;
            
            self.dropPickup = GetScrapForDropPickupTier(item);
            orig(self);
        }

        private PickupIndex GetScrapForDropPickupTier(PickupIndex item)
        {
            ItemTier tier = PickupCatalog.GetPickupDef(item).itemTier;
            return tier switch
            {
                ItemTier.Tier1 => PickupCatalog.FindPickupIndex(RoR2Content.Items.ScrapWhite.itemIndex),
                ItemTier.Tier2 => PickupCatalog.FindPickupIndex(RoR2Content.Items.ScrapGreen.itemIndex),
                ItemTier.Tier3 => PickupCatalog.FindPickupIndex(RoR2Content.Items.ScrapRed.itemIndex),
                ItemTier.Boss => PickupCatalog.FindPickupIndex(RoR2Content.Items.ScrapYellow.itemIndex),
                _ => item
            };
        }

        private void BossGroup_DropRewards(On.RoR2.BossGroup.orig_DropRewards orig, BossGroup self)
        {
            // Artifact is off
            if (!RunArtifactManager.instance.IsArtifactEnabled(myArtifact))
            {
                orig(self);
                return;
            }
            ItemIndex yellowScrap = RoR2Content.Items.ScrapYellow.itemIndex;
            self.bossDrops.Clear();
            self.bossDrops.Add(PickupCatalog.FindPickupIndex(yellowScrap));
            self.bossDropTables.Clear();
            //orig(self);
            //Logger.LogInfo($"boss drop tables: {self.bossDropTables}");
            //foreach(var item in self.bossDropTables)
            //    Logger.LogInfo($"Pickupdroptable in bossdroptable: {item}");

            //foreach(var item in self.bossDrops)
            //    Logger.LogInfo($"boss drop in bossDrops: {item}");

            // Original game code
            if (!Run.instance)
            {
                Debug.LogError("No valid run instance!");
                return;
            }
            if (self.rng == null)
            {
                Debug.LogError("RNG is null!");
                return;
            }
            int participatingPlayerCount = Run.instance.participatingPlayerCount;
            if (participatingPlayerCount != 0)
            {
                if (self.dropPosition)
                {
                    PickupIndex pickupIndex = PickupIndex.none;    // The normal roll of the game
                    if (self.dropTable)
                    {
                        pickupIndex = self.dropTable.GenerateDrop(self.rng);
                    }
                    else
                    {
                        List<PickupIndex> list = Run.instance.availableTier2DropList;
                        if (self.forceTier3Reward)
                        {
                            list = Run.instance.availableTier3DropList;
                        }
                        pickupIndex = self.rng.NextElementUniform<PickupIndex>(list);
                    }
                    pickupIndex = GetScrapForDropPickupTier(pickupIndex);  // Replace it with scrap
                    int num = 1 + self.bonusRewardCount;
                    if (self.scaleRewardsByPlayerCount)
                    {
                        num *= participatingPlayerCount;
                    }
                    float angle = 360f / (float)num;
                    Vector3 vector = Quaternion.AngleAxis((float)UnityEngine.Random.Range(0, 360), Vector3.up) * (Vector3.up * 40f + Vector3.forward * 5f);
                    Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
                    bool flag = self.bossDrops != null && self.bossDrops.Count > 0;
                    bool flag2 = self.bossDropTables != null && self.bossDropTables.Count > 0;
                    int i = 0;
                    while (i < num)
                    {
                        PickupIndex pickupIndex2 = pickupIndex;
                        if (self.bossDrops != null && ((flag || flag2) && self.rng.nextNormalizedFloat <= self.bossDropChance))
                        {
                            if (flag2)
                            {
                                PickupDropTable pickupDropTable = self.rng.NextElementUniform<PickupDropTable>(self.bossDropTables);
                                if (pickupDropTable != null)
                                {
                                    pickupIndex2 = pickupDropTable.GenerateDrop(self.rng);
                                }
                            }
                            else
                            {
                                pickupIndex2 = self.rng.NextElementUniform<PickupIndex>(self.bossDrops);
                            }
                        }
                        PickupDropletController.CreatePickupDroplet(pickupIndex2, self.dropPosition.position, vector);
                        i++;
                        vector = rotation * vector;
                    }
                    return;
                }
                Debug.LogWarning("dropPosition not set for BossGroup! No item will be spawned.");
            }
        }

        /// <summary>
        /// Changes the weights of the interactables. Influences the amount of interactables spawned in the scene
        /// </summary>
        /// <returns>Returns the weighted selection card deck with the new weights</returns>
        private WeightedSelection<DirectorCard> SceneDirector_GenerateInteractableCardSelection(On.RoR2.SceneDirector.orig_GenerateInteractableCardSelection orig, SceneDirector self)
        {
            // Artifact is off
            if (!RunArtifactManager.instance.IsArtifactEnabled(myArtifact))
            {
                return orig(self);
            }
            Logger.LogInfo("Started Generating Interactable CardSelection...");
            self.interactableCredit = (int)((float)self.interactableCredit * Mathf.Clamp(InteractibleCountMultiplier, 0f, 100f));
            WeightedSelection<DirectorCard> weightedSelection = orig(self);

            for (int i = 0; i < weightedSelection.Count; i++)
            {
                if (weightedSelection == null)
                    return weightedSelection;

                string name = weightedSelection.choices[i].value.spawnCard.name;
                bool entryExists = InteractibleToBind.TryGetValue(name.Replace("Sandy", "").Replace("Snowy", ""), out float dictValue);
                Logger.LogInfo($"Current choice is: {name} Got found: {entryExists}");
                if (entryExists)
                {
                    if (dictValue < 0f) 
                        dictValue = 0f;

                    WeightedSelection<DirectorCard>.ChoiceInfo[] choices2 = weightedSelection.choices;
                    Logger.LogInfo($"value of {name} is {dictValue}");
                    choices2[i].weight = choices2[i].weight * dictValue;
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
    }
}
