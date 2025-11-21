﻿using System;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using R2API;
using ShareSuite.Networking;
using UnityEngine;
using UnityEngine.Networking;



// ReSharper disable UnusedMember.Local

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace ShareSuite
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.funkfrog_sipondo.sharesuite", "ShareSuite", "1.15.1")]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class ShareSuite : BaseUnityPlugin
    {
        #region ConfigWrapper init

        // Update this when we want to send a new message
        public static string MessageSendVer = "1.15.1";

        public static ConfigEntry<bool>
            ModIsEnabled,
            MoneyIsShared,
            WhiteItemsShared,
            GreenItemsShared,
            RedItemsShared,
            EquipmentShared,
            LunarItemsShared,
            BossItemsShared,
            VoidItemsShared,
            RichMessagesEnabled,
            DropBlacklistedEquipmentOnShare,
            PrinterCauldronFixEnabled,
            DeadPlayersGetItems,
            OverridePlayerScalingEnabled,
            OverrideBossLootScalingEnabled,
            OverrideVoidFieldLootScalingEnabled,
            OverrideSimulacrumLootScalingEnabled,
            SacrificeFixEnabled,
            MoneyScalarEnabled,
            RandomizeSharedPickups,
            LunarItemsRandomized,
            BossItemsRandomized,
            VoidItemsRandomized,
            OverrideMultiplayerCheck;

        public static ConfigEntry<int> BossLootCredit, VoidFieldLootCredit, SimulacrumLootCredit, InteractablesOffset;
        public static ConfigEntry<double> InteractablesCredit, MoneyScalar;
        public static ConfigEntry<string> ItemBlacklist, EquipmentBlacklist, LastMessageSent;
        public static ConfigEntry<short> NetworkMessageType;

        private bool _previouslyEnabled;

        #endregion

        public void Update()
        {
            if (!ModIsEnabled.Value
                || !MoneyIsShared.Value
                || MoneySharingHooks.MapTransitionActive
                || !GeneralHooks.IsMultiplayer()) return;

            NetworkHandler.RegisterHandlers();

            if (!NetworkServer.active) return;

            foreach (var playerCharacterMasterController in PlayerCharacterMasterController.instances)
            {
                if (playerCharacterMasterController.master.IsDeadAndOutOfLivesServer()) continue;
                if (playerCharacterMasterController.master.money == MoneySharingHooks.SharedMoneyValue) continue;
                playerCharacterMasterController.master.money = (uint) MoneySharingHooks.SharedMoneyValue;
            }
        }

        public static int DefaultMaxScavItemDropCount = 0;

                public void Awake()

                {

                    InitConfig();



                    CommandHelper.RegisterCommands(RoR2.Console.instance);

                    // R2API.Utils.CommandHelper.AddToConsoleWhenReady(); // This is for newer R2API, the old CommandHelper is different

                    //CommandHelper.Add("ss_Enabled", CcModIsEnabled, "Toggles mod."); // CommandHelper.Add is obsolete

                    //CommandHelper.Add("ss_MoneyIsShared", CcMoneyIsShared, "Modifies whether money is shared or not.");

                    //CommandHelper.Add("ss_MoneyScalarEnabled", CcMoneyScalarEnabled, "Modifies whether the money scalar is enabled.");

                    //CommandHelper.Add("ss_MoneyScalar", CcMoneyScalar, "Modifies percent of gold earned when money sharing is on.");

                    //CommandHelper.Add("ss_WhiteItemsShared", CcWhiteShared, "Modifies whether white items are shared or not.");

                    //CommandHelper.Add("ss_GreenItemsShared", CcGreenShared, "Modifies whether green items are shared or not.");

                    //CommandHelper.Add("ss_RedItemsShared", CcRedShared, "Modifies whether red items are shared or not.");

                    //CommandHelper.Add("ss_EquipmentShared", CcEquipmentShared, "Modifies whether equipment is shared or not.");

                    //CommandHelper.Add("ss_LunarItemsShared", CcLunarShared, "Modifies whether lunar items are shared or not.");

                    //CommandHelper.Add("ss_BossItemsShared", CcBossShared, "Modifies whether boss items are shared or not.");

                    //CommandHelper.Add("ss_VoidItemsShared", CcVoidItemsShared, "Modifies whether void items are shared or not.");

                    //CommandHelper.Add("ss_RichMessagesEnabled", CcMessagesEnabled, "Modifies whether rich messages are enabled or not.");

                    //CommandHelper.Add("ss_DropBlacklistedEquipmentOnShare", CcDropBlacklistedEquipmentOnShare, "Changes the way shared equipment handles blacklisted equipment.");

                    //CommandHelper.Add("ss_RandomizeSharedPickups", CcRandomizeSharedPickups, "Randomizes pickups per player.");

                    //CommandHelper.Add("ss_PrinterCauldronFix", CcPrinterCauldronFix, "Modifies whether printers and cauldrons should not duplicate items.");

                    //CommandHelper.Add("ss_OverridePlayerScaling", CcDisablePlayerScaling, "Modifies whether interactable count should scale based on player count.");

                    //CommandHelper.Add("ss_InteractablesCredit", CcInteractablesCredit, "Modifies amount of interactables when player scaling is overridden.");

                    //CommandHelper.Add("ss_InteractablesOffset", CcInteractablesOffset, "Modifies amount of interactables when player scaling is overridden.");

                    //CommandHelper.Add("ss_OverrideBossLootScaling", CcBossLoot, "Modifies whether boss loot should scale based on player count.");

                    //CommandHelper.Add("ss_BossLootCredit", CcBossLootCredit, "Modifies amount of boss item drops.");

                    //CommandHelper.Add("ss_OverrideVoidFieldLoot", CcVoidFieldLoot, "Modifies whether Void Field loot should scale based on player count.");

                    //CommandHelper.Add("ss_OverrideSimulacrumLoot", CcSimulacrumLoot, "Modifies whether Simulacrum loot should scale based on player count.");

                    //CommandHelper.Add("ss_VoidFieldLootCredit", CcVoidFieldCredit, "Modifies amount of Void Field item drops.");

                    //CommandHelper.Add("ss_SimulacrumLootCredit", CcSimulacrumCredit, "Modifies amount of Simulacrum item drops.");

                    //CommandHelper.Add("ss_SacrificeFixEnabled", CcSacrificeFixEnabled, "Modifies whether items are shared to dead players.");

                    //CommandHelper.Add("ss_DeadPlayersGetItems", CcDeadPlayersGetItems, "Modifies whether items are shared to dead players.");

        

                    //On.RoR2.Networking.GameNetworkManager.OnClientConnect += (self, user, t) => { };

        

                    #region Hook registration

        

                    // Register all the hooks

                    ReloadHooks();

                    MoneySharingHooks.SharedMoneyValue = 15;

        

                    #endregion

                }

        

                private void ReloadHooks(object _ = null, EventArgs __ = null)

                {

                    if (_previouslyEnabled && !ModIsEnabled.Value)

                    {

                        GeneralHooks.UnHook();

                        MoneySharingHooks.UnHook();

                        ItemSharingHooks.UnHook();

                        EquipmentSharingHooks.UnHook();

                        ChatHandler.UnHook();

                        _previouslyEnabled = false;

                    }

        

                    if (!_previouslyEnabled && ModIsEnabled.Value)

                    {

                        _previouslyEnabled = true;

                        GeneralHooks.Hook();

                        MoneySharingHooks.Hook();

                        ItemSharingHooks.Hook();

                        EquipmentSharingHooks.Hook();

                        ChatHandler.Hook();

                    }

                }

        

                private void InitConfig()

                {

                    ModIsEnabled = Config.Bind(

                        "Settings",

                        "ModEnabled",

                        true,

                        "Toggles whether or not the mod is enabled. If turned off while in-game, it will unhook " +

                        "everything and reset the game to it's default behaviors."

                    );

                    ModIsEnabled.SettingChanged += ReloadHooks;

        

                    MoneyIsShared = Config.Bind(

                        "Settings",

                        "MoneyShared",

                        true,

                        "Toggles money sharing between teammates. Every player gains money together and spends it " +

                        "from one central pool of money."

                    );

        

                    WhiteItemsShared = Config.Bind(

                        "Settings",

                        "WhiteItemsShared",

                        true,

                        "Toggles item sharing for common (white color) items."

                    );

        

                    GreenItemsShared = Config.Bind(

                        "Settings",

                        "GreenItemsShared",

                        true,

                        "Toggles item sharing for rare (green color) items."

                    );

        

                    RedItemsShared = Config.Bind(

                        "Settings",

                        "RedItemsShared",

                        true,

                        "Toggles item sharing for legendary (red color) items."

                    );

        

                    EquipmentShared = Config.Bind(

                        "Settings",

                        "EquipmentShared",

                        false,

                        "Toggles item sharing for equipment."

                    );

        

                    LunarItemsShared = Config.Bind(

                        "Settings",

                        "LunarItemsShared",

                        false,

                        "Toggles item sharing for Lunar (blue color) items."

                    );

        

                    BossItemsShared = Config.Bind(

                        "Settings",

                        "BossItemsShared",

                        true,

                        "Toggles item sharing for boss (yellow color) items."

                    );

        

                    VoidItemsShared = Config.Bind(

                        "Settings",

                        "VoidItemsShared",

                        false,

                        "Toggles item sharing for void (purple/corrupted) items."

                    );

        

                    RichMessagesEnabled = Config.Bind(

                        "Settings",

                        "RichMessagesEnabled",

                        true,

                        "Toggles detailed item pickup messages with information on who picked the item up and" +

                        " who all received the item."

                    );

        

                    DropBlacklistedEquipmentOnShare = Config.Bind(

                        "Balance",

                        "DropBlacklistedEquipmentOnShare",

                        false,

                        "Changes the way shared equipment handles blacklisted equipment. If true," +

                        " blacklisted equipment will be dropped on the ground once a new equipment is shared" +

                        ". If false, blacklisted equipment is left untouched when new equipment is shared."

                    );

        

                    RandomizeSharedPickups = Config.Bind(

                        "Balance",

                        "RandomizeSharedPickups",

                        false,

                        "When enabled each player (except the player who picked up the item) will get a randomized item of the same rarity."

                    );

        

                    LunarItemsRandomized = Config.Bind(

                        "Balance",

                        "LunarItemsRandomized",

                        true,

                        "Toggles randomizing Lunar items in RandomizeSharedPickups mode."

                    );

        

                    BossItemsRandomized = Config.Bind(

                        "Balance",

                        "BossItemsRandomized",

                        false,

                        "Toggles randomizing Boss items in RandomizeSharedPickups mode."

                    );

        

                    VoidItemsRandomized = Config.Bind(

                        "Balance",

                        "VoidItemsRandomized",

                        false,

                        "Toggles randomizing Void items in RandomizeSharedPickups mode."

                    );

        

                    PrinterCauldronFixEnabled = Config.Bind(

                        "Balance",

                        "PrinterCauldronFix",

                        true,

                        "Toggles 3D printer and Cauldron item dupe fix by giving the item directly instead of" +

                        " dropping it on the ground."

                    );

        

                    DeadPlayersGetItems = Config.Bind(

                        "Balance",

                        "DeadPlayersGetItems",

                        false,

                        "Toggles whether or not dead players should get copies of picked up items."

                    );

        

                    OverridePlayerScalingEnabled = Config.Bind(

                        "Balance",

                        "OverridePlayerScaling",

                        false,

                        "Toggles override of the scalar of interactables (chests, shrines, etc) that spawn in the world to your configured credit."

                    );

        

                    InteractablesCredit = Config.Bind(

                        "Balance",

                        "InteractablesCredit",

                        1d,

                        "If player scaling via this mod is enabled, the amount of players the game should think are playing in terms of chest spawns."

                    );

        

                    InteractablesOffset = Config.Bind(

                        "Balance",

                        "InteractablesOffset",

                        0,

                        "If player scaling via this mod is enabled, the offset from base scaling for interactables credit (e.g. 100 would add 100 interactables credit (not 100 interactables), can also be negative)."

                    );

        

                    OverrideBossLootScalingEnabled = Config.Bind(

                        "Balance",

                        "OverrideBossLootScaling",

                        true,

                        "Toggles override of the scalar of boss loot drops to your configured balance."

                    );

        

                    BossLootCredit = Config.Bind(

                        "Balance",

                        "BossLootCredit",

                        1,

                        "Specifies the amount of boss items dropped when the boss drop override is true."

                    );

        

                    OverrideVoidFieldLootScalingEnabled = Config.Bind(

                        "Balance",

                        "OverrideVoidLootScaling",

                        true,

                        "Toggles override of the scalar of Void Field loot drops to your configured balance."

                    );

        

                    OverrideSimulacrumLootScalingEnabled = Config.Bind(

                        "Balance",

                        "OverrideSimulacrumLootScaling",

                        true,

                        "Toggles override of the scalar of Simulacrum loot drops to your configured balance."

                    );

        

                    OverrideMultiplayerCheck = Config.Bind(

                        "Debug",

                        "OverrideMultiplayerCheck",

                        false,

                        "Forces ShareSuite to think that the game is running in a multiplayer instance."

                    );

        

                    LastMessageSent = Config.Bind(

                        "Debug",

                        "LastMessageSent",

                        "none",

                        "Keeps track of the last mod version that sent you a message. Prevents spam, don't touch."

                    );

        

                    VoidFieldLootCredit = Config.Bind(

                        "Balance",

                        "VoidFieldLootCredit",

                        1,

                        "Specifies the amount of items dropped from completed Void Fields when the Void Field scaling override is true."

                    );

        

                    SimulacrumLootCredit = Config.Bind(

                        "Balance",

                        "SimulacrumLootCredit",

                        1,

                        "Specifies the amount of items dropped after each Simulacrum round when the Simulacrum scaling override is true."

                    );

        

                    SacrificeFixEnabled = Config.Bind(

                        "Balance",

                        "SacrificeFixEnabled",

                        true,

                        "Toggles the reduction in sacrifice loot drops to be balanced with shared items enabled."

                    );

        

                    MoneyScalarEnabled = Config.Bind(

                        "Settings",

                        "MoneyScalarEnabled",

                        false,

                        "Toggles the money scalar, set MoneyScalar to an amount to fine-tune the amount of gold " +

                        "you recieve."

                    );

        

                    MoneyScalar = Config.Bind(

                        "Settings",

                        "MoneyScalar",

                        1D,

                        "Modifies player count used in calculations of gold earned when money sharing is on."

                    );

        

                    ItemBlacklist = Config.Bind(

                        "Settings",

                        "ItemBlacklist",

                        "BeetleGland,TreasureCache,TitanGoldDuringTP,TPHealingNova,ArtifactKey,FreeChest,RoboBallBuddy,MinorConstructOnKill",

                        "Items (by internal name) that you do not want to share, comma separated. Please find the item \"Code Names\" at: https://github.com/risk-of-thunder/R2Wiki/wiki/Item-&-Equipment-IDs-and-Names"

                    );

                    ItemBlacklist.SettingChanged += (o, e) => Blacklist.Recalculate();

        

                    EquipmentBlacklist = Config.Bind(

                        "Settings",

                        "EquipmentBlacklist",

                        "",

                        "Equipment (by internal name) that you do not want to share, comma separated. Please find the \"Code Names\"s at: https://github.com/risk-of-thunder/R2Wiki/wiki/Item-&-Equipment-IDs-and-Names"

                    );

                    EquipmentBlacklist.SettingChanged += (o, e) => Blacklist.Recalculate();

        

                    NetworkMessageType = Config.Bind(

                        "Settings",

                        "NetworkMessageType",

                        (short)1021,

                        "The identifier for network message for this mod. Must be unique across all mods."

                    );

                }

        

                #region CommandParser

        

        #pragma warning disable IDE0051 //Commands usually aren't called from code.

        

                //TODO Add more information when you send the commands with no args

        

                // ModIsEnabled

                private static void CcModIsEnabled(ConCommandArgs args)

                {

                    if (args.Count == 0)

                    {

                        Debug.Log(ModIsEnabled.Value);

                        return;

                    }

        

                    var valid = TryGetBool(args[0]);

                    if (!valid.HasValue)

                        Debug.Log("Couldn't parse to boolean.");

                    else

                    {

                        ModIsEnabled.Value = valid.Value;

                        Debug.Log($"Mod status set to {ModIsEnabled.Value}.");

                    }

                }

        

                // MoneyIsShared

                private static void CcMoneyIsShared(ConCommandArgs args)

                {

                    if (args.Count == 0)

                    {

                        Debug.Log(MoneyIsShared.Value);

                        return;

                    }

        

                    var valid = TryGetBool(args[0]);

                    if (!valid.HasValue)

                        Debug.Log("Couldn't parse to boolean.");

                    else

                    {

                        if (MoneyIsShared.Value != valid.Value)

                        {

                            if (MoneyIsShared.Value && !valid.Value)

                            {

                                IL.EntityStates.GoldGat.GoldGatFire.FireBullet -= MoneySharingHooks.RemoveGoldGatMoneyLine;

                            }

                            else

                            {

                                if (GeneralHooks.IsMultiplayer())

                                    IL.EntityStates.GoldGat.GoldGatFire.FireBullet += MoneySharingHooks.RemoveGoldGatMoneyLine;

                            }

                        }

        

                        MoneyIsShared.Value = valid.Value;

                        Debug.Log($"Money sharing status set to {MoneyIsShared.Value}.");

                    }

                }

        

                // MoneyScalarEnabled

                private static void CcMoneyScalarEnabled(ConCommandArgs args)

                {

                    if (args.Count == 0)

                    {

                        Debug.Log(MoneyScalarEnabled.Value);

                        return;

                    }

        

                    var valid = TryGetBool(args[0]);

                    if (!valid.HasValue)

                        Debug.Log("Couldn't parse to boolean.");

                    else

                    {

                        MoneyScalarEnabled.Value = valid.Value;

                        Debug.Log($"Money sharing scalar status set to {MoneyScalarEnabled.Value}.");

                    }

                }

        

                // MoneyScalar

                private static void CcMoneyScalar(ConCommandArgs args)

                {

                    if (args.Count == 0)

                    {

                        Debug.Log(MoneyScalar.Value);

                        return;

                    }

        

                    var valid = args.TryGetArgDouble(0);

                    if (!valid.HasValue)

                        Debug.Log("Couldn't parse to a number.");

                    else

                    {

                        MoneyScalar.Value = valid.Value;

                        Debug.Log($"Mod status set to {MoneyScalar.Value}.");

                    }

                }

        

                // WhiteItemsShared

                private static void CcWhiteShared(ConCommandArgs args)

                {

                    if (args.Count == 0)

                    {

                        Debug.Log(WhiteItemsShared.Value);

                        return;

                    }

        

                    var valid = TryGetBool(args[0]);

                    if (!valid.HasValue)

                        Debug.Log("Couldn't parse to boolean.");

                    else

                    {

                        WhiteItemsShared.Value = valid.Value;

                        Debug.Log($"White items sharing set to {WhiteItemsShared.Value}.");

                    }

                }

        

                // GreenItemsShared

                private static void CcGreenShared(ConCommandArgs args)

                {

                    if (args.Count == 0)

                    {

                        Debug.Log(GreenItemsShared.Value);

                        return;

                    }

        

                    var valid = TryGetBool(args[0]);

                    if (!valid.HasValue)

                        Debug.Log("Couldn't parse to boolean.");

                    else

                    {

                        GreenItemsShared.Value = valid.Value;

                        Debug.Log($"Green items sharing set to {GreenItemsShared.Value}.");

                    }

                }

        

                // RedItemsShared

                private static void CcRedShared(ConCommandArgs args)

                {

                    if (args.Count == 0)

                    {

                        Debug.Log(RedItemsShared.Value);

                        return;

                    }

        

                    var valid = TryGetBool(args[0]);

                    if (!valid.HasValue)

                        Debug.Log("Couldn't parse to boolean.");

                    else

                    {

                        RedItemsShared.Value = valid.Value;

                        Debug.Log($"Red item sharing set to {RedItemsShared.Value}.");

                    }

                }

        

                // EquipmentShared

                private static void CcEquipmentShared(ConCommandArgs args)

                {

                    if (args.Count == 0)

                    {

                        Debug.Log(EquipmentShared.Value);

                        return;

                    }

        

                    var valid = TryGetBool(args[0]);

                    if (!valid.HasValue)

                        Debug.Log("Couldn't parse to boolean.");

                    else

                    {

                        EquipmentShared.Value = valid.Value;

                        Debug.Log($"Equipment sharing set to {EquipmentShared.Value}.");

                    }

                }

        

                // LunarItemsShared

                private static void CcLunarShared(ConCommandArgs args)

                {

                    if (args.Count == 0)

                    {

                        Debug.Log(LunarItemsShared.Value);

                        return;

                    }

        

                    var valid = TryGetBool(args[0]);

                    if (!valid.HasValue)

                        Debug.Log("Couldn't parse to boolean.");

                    else

                    {

                        LunarItemsShared.Value = valid.Value;

                        Debug.Log($"Lunar item sharing set to {LunarItemsShared.Value}.");

                    }

                }

        

                // BossItemsShared

                private static void CcBossShared(ConCommandArgs args)

                {

                    if (args.Count == 0)

                    {

                        Debug.Log(BossItemsShared.Value);

                        return;

                    }

        

                    var valid = TryGetBool(args[0]);

                    if (!valid.HasValue)

                        Debug.Log("Couldn't parse to boolean.");

                    else

                    {

                        BossItemsShared.Value = valid.Value;

                        Debug.Log($"Boss item sharing set to {BossItemsShared.Value}.");

                    }

                }

        

                // BossItemsShared

                private static void CcVoidItemsShared(ConCommandArgs args)

                {

                    if (args.Count == 0)

                    {

                        Debug.Log(VoidItemsShared.Value);

                        return;

                    }

        

                    var valid = TryGetBool(args[0]);

                    if (!valid.HasValue)

                        Debug.Log("Couldn't parse to boolean.");

                    else

                    {

                        VoidItemsShared.Value = valid.Value;

                        Debug.Log($"Void item sharing set to {VoidItemsShared.Value}.");

                    }

                }

        

                // RichMessagesEnabled

                private static void CcMessagesEnabled(ConCommandArgs args)

                {

                    if (args.Count == 0)

                    {

                        Debug.Log(RichMessagesEnabled.Value);

                        return;

                    }

        

                    var valid = TryGetBool(args[0]);

                    if (!valid.HasValue)

                        Debug.Log("Couldn't parse to boolean.");

                    else

                    {

                        RichMessagesEnabled.Value = valid.Value;

                        Debug.Log($"Rich Messages Enabled set to {RichMessagesEnabled.Value}.");

                    }

                }

        

                //DropBlacklistedEquipmentOnShare

                private static void CcDropBlacklistedEquipmentOnShare(ConCommandArgs args)

                {

                    if (args.Count == 0)

                    {

                        Debug.Log(DropBlacklistedEquipmentOnShare.Value);

                        return;

                    }

        

                    var valid = TryGetBool(args[0]);

                    if (!valid.HasValue)

                        Debug.Log("Couldn't parse to boolean.");

                    else

                    {

                        DropBlacklistedEquipmentOnShare.Value = valid.Value;

                        Debug.Log($"Drop Blacklisted Equipment on Share set to {DropBlacklistedEquipmentOnShare.Value}.");

                    }

                }

        

        

                //randomisepickups

                private static void CcRandomizeSharedPickups(ConCommandArgs args)

                {

                    if (args.Count == 0)

                    {

                        Debug.Log(RandomizeSharedPickups.Value);

                        return;

                    }

        

                    var valid = TryGetBool(args[0]);

                    if (!valid.HasValue)

                        Debug.Log("Couldn't parse to boolean.");

                    else

                    {

                        RandomizeSharedPickups.Value = valid.Value;

                        Debug.Log($"Randomize pickups per player set to {RandomizeSharedPickups.Value}.");

                    }

                }

        

                // PrinterCauldronFix

                private static void CcPrinterCauldronFix(ConCommandArgs args)

                {

                    if (args.Count == 0)

                    {

                        Debug.Log(PrinterCauldronFixEnabled.Value);

                        return;

                    }

        

                    var valid = TryGetBool(args[0]);

                    if (!valid.HasValue)

                        Debug.Log("Couldn't parse to boolean.");

                    else

                    {

                        PrinterCauldronFixEnabled.Value = valid.Value;

                        Debug.Log($"Printer and cauldron fix set to {PrinterCauldronFixEnabled.Value}.");

                    }

                }

        

                // DisablePlayerScaling

                private static void CcDisablePlayerScaling(ConCommandArgs args)

                {

                    if (args.Count == 0)

                    {

                        Debug.Log(OverridePlayerScalingEnabled.Value);

                        return;

                    }

        

                    var valid = TryGetBool(args[0]);

                    if (!valid.HasValue)

                        Debug.Log("Couldn't parse to boolean.");

                    else

                    {

                        OverridePlayerScalingEnabled.Value = valid.Value;

                        Debug.Log($"Player scaling disable set to {OverridePlayerScalingEnabled.Value}.");

                    }

                }

        

                // InteractablesCredit

                private static void CcInteractablesCredit(ConCommandArgs args)

                {

                    if (args.Count == 0)

                    {

                        Debug.Log(InteractablesCredit.Value);

                        return;

                    }

        

                    var valid = args.TryGetArgDouble(0);

                    if (!valid.HasValue)

                        Debug.Log("Couldn't parse to a number.");

                    else

                    {

                        InteractablesCredit.Value = valid.Value;

                        Debug.Log($"Interactible credit set to {InteractablesCredit.Value}.");

                    }

                }

        

                // InteractablesOffset

                private static void CcInteractablesOffset(ConCommandArgs args)

                {

                    if (args.Count == 0)

                    {

                        Debug.Log(InteractablesOffset.Value);

                        return;

                    }

        

                    var valid = args.TryGetArgInt(0);

                    if (!valid.HasValue)

                        Debug.Log("Couldn't parse to a number.");

                    else

                    {

                        InteractablesOffset.Value = valid.Value;

                        Debug.Log($"Interactible offset set to {InteractablesOffset.Value}.");

                    }

                }

        

                // DisableBossLootScaling

                private static void CcBossLoot(ConCommandArgs args)

                {

                    if (args.Count == 0)

                    {

                        Debug.Log(OverrideBossLootScalingEnabled.Value);

                        return;

                    }

        

                    var valid = TryGetBool(args[0]);

                    if (!valid.HasValue)

                        Debug.Log("Couldn't parse to boolean.");

                    else

                    {

                        OverrideBossLootScalingEnabled.Value = valid.Value;

                        Debug.Log($"Boss loot scaling disable set to {OverrideBossLootScalingEnabled.Value}.");

                    }

                }

        

                // BossLootCredit

                private static void CcBossLootCredit(ConCommandArgs args)

                {

                    if (args.Count == 0)

                    {

                        Debug.Log(BossLootCredit.Value);

                        return;

                    }

        

                    var valid = args.TryGetArgInt(0);

                    if (!valid.HasValue)

                        Debug.Log("Couldn't parse to an integer number.");

                    else

                    {

                        BossLootCredit.Value = valid.Value;

                        Debug.Log($"Boss loot credit set to {BossLootCredit.Value}.");

                    }

                }

        

                // DisableVoidFieldLootScaling

                private static void CcVoidFieldLoot(ConCommandArgs args)

                {

                    if (args.Count == 0)

                    {

                        Debug.Log(OverrideVoidFieldLootScalingEnabled.Value);

                        return;

                    }

        

                    var valid = TryGetBool(args[0]);

                    if (!valid.HasValue)

                        Debug.Log("Couldn't parse to boolean.");

                    else

                    {

                        if (OverrideVoidFieldLootScalingEnabled.Value != valid.Value)

                        {

                            if (OverrideVoidFieldLootScalingEnabled.Value && !valid.Value)

                            {

                                IL.RoR2.ArenaMissionController.EndRound -= ItemSharingHooks.ArenaDropEnable;

                            }

                            else

                            {

                                IL.RoR2.ArenaMissionController.EndRound += ItemSharingHooks.ArenaDropEnable;

                            }

                        }

        

                        OverrideVoidFieldLootScalingEnabled.Value = valid.Value;

                        Debug.Log($"Void Field loot scaling disable set to {OverrideVoidFieldLootScalingEnabled.Value}.");

                    }

                }

        

                // DisableSimulacrumLootScaling

                private static void CcSimulacrumLoot(ConCommandArgs args)

                {

                    if (args.Count == 0)

                    {

                        Debug.Log(OverrideSimulacrumLootScalingEnabled.Value);

                        return;

                    }

        

                    var valid = TryGetBool(args[0]);

                    if (!valid.HasValue)

                        Debug.Log("Couldn't parse to boolean.");

                    else

                    {

                        if (OverrideSimulacrumLootScalingEnabled.Value != valid.Value)

                        {

                            if (OverrideSimulacrumLootScalingEnabled.Value && !valid.Value)

                            {

                                IL.RoR2.InfiniteTowerWaveController.DropRewards -= ItemSharingHooks.SimulacrumArenaDropEnable;

                            }

                            else

                            {

                                IL.RoR2.InfiniteTowerWaveController.DropRewards += ItemSharingHooks.SimulacrumArenaDropEnable;

                            }

                        }

        

                        OverrideSimulacrumLootScalingEnabled.Value = valid.Value;

                        Debug.Log($"Void Field loot scaling disable set to {OverrideSimulacrumLootScalingEnabled.Value}.");

                    }

                }

        

                // VoidFieldLootCredit

                private static void CcVoidFieldCredit(ConCommandArgs args)

                {

                    if (args.Count == 0)

                    {

                        Debug.Log(VoidFieldLootCredit.Value);

                        return;

                    }

        

                    var valid = args.TryGetArgInt(0);

                    if (!valid.HasValue)

                        Debug.Log("Couldn't parse to an integer number.");

                    else

                    {

                        VoidFieldLootCredit.Value = valid.Value;

                        Debug.Log($"Void Field loot credit set to {VoidFieldLootCredit.Value}.");

                    }

                }

        

                // VoidFieldLootCredit

                private static void CcSimulacrumCredit(ConCommandArgs args)

                {

                    if (args.Count == 0)

                    {

                                                Debug.Log(SimulacrumLootCredit.Value);

                                                return;

                    }

        

                    var valid = args.TryGetArgInt(0);

                    if (!valid.HasValue)

                        Debug.Log("Couldn't parse to an integer number.");

                    else

                    {

                        SimulacrumLootCredit.Value = valid.Value;

                        Debug.Log($"Simulacrum loot credit set to {SimulacrumLootCredit.Value}.");

                    }

                }

        

                // Sacrifice Fix

                private static void CcSacrificeFixEnabled(ConCommandArgs args)

                {

                    if (args.Count == 0)

                    {

                        Debug.Log(SacrificeFixEnabled.Value);

                        return;

                    }

        

                    var valid = TryGetBool(args[0]);

                    if (!valid.HasValue)

                        Debug.Log("Couldn't parse to boolean.");

                    else

                    {

                        SacrificeFixEnabled.Value = valid.Value;

                        Debug.Log($"Sacrifice fix set to {SacrificeFixEnabled.Value}");

                    }

                }

        

                // DeadPlayersGetItems

                private static void CcDeadPlayersGetItems(ConCommandArgs args)

                {

                    if (args.Count == 0)

                    {

                        Debug.Log(DeadPlayersGetItems.Value);

                        return;

                    }

        

                    var valid = TryGetBool(args[0]);

                    if (!valid.HasValue)

                        Debug.Log("Couldn't parse to boolean.");

                    else

                    {

                        DeadPlayersGetItems.Value = valid.Value;

                        Debug.Log($"Dead player getting shared items set to {DeadPlayersGetItems.Value}");

                    }

                }

        

        //TODO re-introduce these as add/remove commands

        //        // ItemBlacklist

        //        private static void CcItemBlacklist(ConCommandArgs args)

        //        {

        //            if (args.Count == 0)

        //            {

        //                Debug.Log(ItemBlacklist.Value);

        //                return;

        //            }

        //

        //            var list = string.Join(",",

        //                from i in Enumerable.Range(0, args.Count)

        //                select args.TryGetArgInt(i) into num

        //                where num != null

        //                select num.Value);

        //            ItemBlacklist.Value = list;

        //        }

        //

        //        // ItemBlacklist

        //        private static void CcEquipmentBlacklist(ConCommandArgs args)

        //        {

        //            if (args.Count == 0)

        //            {

        //                Debug.Log(EquipmentBlacklist.Value);

        //                return;

        //            }

        //

        //            var list = string.Join(",",

        //                from i in Enumerable.Range(0, args.Count)

        //                select args.TryGetArgInt(i) into num

        //                where num != null

        //                select num.Value);

        //            ItemBlacklist.Value = list;

        //        }

        

                                        private static bool? TryGetBool(string arg)

        

                                        {

        

                                            string[] posStr = { "yes", "true", "1" };

        

                                            string[] negStr = { "no", "false", "0", "-1" };

        

                                

        

                                            if (posStr.Contains(arg.ToLower()))

        

                                            {

        

                                                return true;

        

                                            }

        

                                            if (negStr.Contains(arg.ToLower()))

        

                                            {

        

                                                return false;

        

                                            }

        

                                            return new bool?();

        

                                        }

        

                                

        

                                #pragma warning restore IDE0051

        

                                        #endregion CommandParser

        

                                    }

        

                                }

        

                                