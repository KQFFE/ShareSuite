using System;
using System.Collections.Generic;
using System.Reflection;
using RoR2;
using UnityEngine;

namespace ShareSuite
{
    public static class ChatHandler
    {
        public static void UnHook()
        {
            On.RoR2.Chat.SendPlayerConnectedMessage -= SendIntroMessage; 
        }

        public static void Hook()
        {
            On.RoR2.Chat.SendPlayerConnectedMessage += SendIntroMessage; 
        }

        // ReSharper disable twice ArrangeTypeMemberModifiers
        private const string GrayColor = "7e91af";
        private const string RedColor = "ed4c40";
        private const string LinkColor = "5cb1ed";
        private const string ErrorColor = "ff0000";

        private const string NotSharingColor = "f07d6e";

        // Red (previously bc2525) / Blue / Yellow / Green / Orange / Cyan / Pink / Deep Purple
        private static readonly string[] PlayerColors =
            {"f23030", "2083fc", "f1f41a", "4dc344", "f27b0c", "3cdede", "db46bd", "9400ea"};

        public static void SendIntroMessage(On.RoR2.Chat.orig_SendPlayerConnectedMessage orig, NetworkUser user)
        {
            orig(user);

            if (ShareSuite.LastMessageSent.Value.Equals(ShareSuite.MessageSendVer)) return;

            ShareSuite.LastMessageSent.Value = ShareSuite.MessageSendVer;

            var notRepeatedMessage = $"<color=#{GrayColor}>(This message will </color><color=#{RedColor}>NOT</color>"
                                     + $"<color=#{GrayColor}> display again!) </color>";
            var message = $"<color=#{GrayColor}>Hey there! Thanks for installing </color>"
                          + $"<color=#{RedColor}>ShareSuite 2.9.0 CommandHelper Beta</color><color=#{GrayColor}>!"
                          + " This release may still contain problems. Have fun!</color>";
            var clickChatBox = $"<color=#{RedColor}>(Click the chat box to view the full message)</color>";

            var timer = new System.Timers.Timer(5000); // Send messages after 5 seconds
            timer.Elapsed += delegate
            {
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage { baseToken = notRepeatedMessage });
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage { baseToken = message });
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage { baseToken = clickChatBox });
            };
            timer.AutoReset = false;
            timer.Start();
        }

        public static void SendRichPickupMessage(CharacterMaster player, PickupIndex pickupIndex)
        {
            var body = player.hasBody ? player.GetBody() : null;
            var pickupDef = PickupCatalog.GetPickupDef(pickupIndex);

            if (!GeneralHooks.IsMultiplayer() || body == null || !ShareSuite.RichMessagesEnabled.Value)
            {
                // The original SendPickupMessage is gone, we'll just send a simple one if needed.
                if (ShareSuite.RichMessagesEnabled.Value) SendPickupMessage(player, pickupIndex);
                return;
            }

            var pickupColor = pickupDef.baseColor;
            var pickupName = Language.GetString(pickupDef.nameToken);
            var playerColor = GetPlayerColor(player.playerCharacterMasterController);
            var itemCount = player.inventory.GetItemCount(pickupDef.itemIndex); // GetItemCount is fine for display purposes

            if (pickupDef.coinValue > 0)
            {
                var coinMessage =
                    $"<color=#{playerColor}>{body.GetUserName()}</color> <color=#{GrayColor}>picked up</color> " +
                    $"<color=#{ColorUtility.ToHtmlStringRGB(pickupColor)}>" +
                    $"{(string.IsNullOrEmpty(pickupName) ? "???" : pickupName)} ({pickupDef.coinValue})</color> <color=#{GrayColor}>for themselves.</color>";

                Chat.SendBroadcastChat(new Chat.SimpleChatMessage { baseToken = coinMessage });

                return;
            }

            if (Blacklist.HasItem(pickupDef.itemIndex) || !ItemSharingHooks.IsValidItemPickup(pickupDef.pickupIndex))
            {
                var singlePickupMessage =
                    $"<color=#{playerColor}>{body.GetUserName()}</color> <color=#{GrayColor}>picked up</color> " +
                    $"<color=#{ColorUtility.ToHtmlStringRGB(pickupColor)}>" +
                    $"{(string.IsNullOrEmpty(pickupName) ? "???" : pickupName)} ({itemCount})</color> <color=#{GrayColor}>for themselves. </color>" +
                    $"<color=#{NotSharingColor}>(Item Set to NOT be Shared)</color>";
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage { baseToken = singlePickupMessage });
                return;
            }

            var pickupMessage =
                $"<color=#{playerColor}>{body.GetUserName()}</color> <color=#{GrayColor}>picked up</color> " +
                $"<color=#{ColorUtility.ToHtmlStringRGB(pickupColor)}>" +
                $"{(string.IsNullOrEmpty(pickupName) ? "???" : pickupName)} ({itemCount})</color> <color=#{GrayColor}>for themselves</color>" +
                $"{ItemPickupFormatter(body)}<color=#{GrayColor}>.</color>";
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage { baseToken = pickupMessage });
        }

        public static void SendRichCauldronMessage(CharacterMaster player, PickupIndex index)
        {
            var body = player.GetBody();

            if (!GeneralHooks.IsMultiplayer() ||
                body == null ||
                !ShareSuite.RichMessagesEnabled.Value)
            {
                return;
            }

            var pickupDef = PickupCatalog.GetPickupDef(index);
            var pickupColor = pickupDef.baseColor;
            var pickupName = Language.GetString(pickupDef.nameToken);
            var playerColor = GetPlayerColor(player.playerCharacterMasterController);
            var itemCount = player.inventory.GetItemCount(pickupDef.itemIndex); // GetItemCount is fine for display purposes

            var pickupMessage =
                $"<color=#{playerColor}>{body.GetUserName()}</color> <color=#{GrayColor}>traded for</color> " +
                $"<color=#{ColorUtility.ToHtmlStringRGB(pickupColor)}>" +
                $"{(string.IsNullOrEmpty(pickupName) ? "???" : pickupName)} ({itemCount})</color><color=#{GrayColor}>.</color>";
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage { baseToken = pickupMessage });
        }

        public static void SendRichRandomizedPickupMessage(CharacterMaster origPlayer, PickupDef origPickup,
            Dictionary<CharacterMaster, PickupIndex> pickupIndices)
        {
            if (!GeneralHooks.IsMultiplayer() || !ShareSuite.RichMessagesEnabled.Value)
            {
                if (ShareSuite.RichMessagesEnabled.Value) SendPickupMessage(origPlayer, origPickup.pickupIndex);

                return;
            }

            // If nobody got a randomized item
            if (pickupIndices.Count == 1)
            {
                SendRichPickupMessage(origPlayer, origPickup.pickupIndex);
                return;
            }

            var remainingPlayers = pickupIndices.Count;
            var pickupMessage = "";

            foreach (var index in pickupIndices)
            {
                var currentPickupDef = PickupCatalog.GetPickupDef(index.Value);
                var pickupColor = currentPickupDef.baseColor;
                var pickupName = Language.GetString(currentPickupDef.nameToken);
                var playerColor = GetPlayerColor(index.Key.playerCharacterMasterController);
                var itemCount =
                    index.Key.inventory.GetItemCount(currentPickupDef.itemIndex); // GetItemCount is fine for display purposes

                if (remainingPlayers != pickupIndices.Count)
                {
                    if (remainingPlayers > 1)
                    {
                        pickupMessage += $"<color=#{GrayColor}>,</color> ";
                    }
                    else if (remainingPlayers == 1)
                    {
                        pickupMessage += $"<color=#{GrayColor}>, and</color> ";
                    }
                }

                remainingPlayers--;

                pickupMessage +=
                    $"<color=#{playerColor}>{index.Key.playerCharacterMasterController.GetDisplayName()}</color> " +
                    $"<color=#{GrayColor}>got</color> " +
                    $"<color=#{ColorUtility.ToHtmlStringRGB(pickupColor)}>" +
                    $"{(string.IsNullOrEmpty(pickupName) ? "???" : pickupName)} ({itemCount})</color>";
            }

            Chat.SendBroadcastChat(new Chat.SimpleChatMessage { baseToken = pickupMessage });
        }

        private static string ItemPickupFormatter(CharacterBody body)
        {
            // Initialize an int for the amount of players eligible to receive the item
            var eligiblePlayers = GetEligiblePlayers(body);

            // If there's nobody else, return " and No-one Else"
            if (eligiblePlayers < 1) return $" <color=#{GrayColor}>and no-one else</color>";

            // If there's only one other person in the lobby
            if (eligiblePlayers == 1)
            {
                // Loop through every player in the lobby
                foreach (var playerCharacterMasterController in PlayerCharacterMasterController.instances)
                {
                    var master = playerCharacterMasterController.master;

                    // If they don't have a body or are the one who picked up the item, go to the next person
                    if (!master.hasBody || master.GetBody() == body) continue;

                    // Get the player color
                    var playerColor = GetPlayerColor(playerCharacterMasterController);

                    // If the player is alive OR dead and deadplayersgetitems is on, return their name
                    return $" <color=#{GrayColor}>and</color> " + $"<color=#{playerColor}>" +
                           playerCharacterMasterController.GetDisplayName() + "</color>";
                }

                // Shouldn't happen ever, if something's borked
                return $"<color=#{ErrorColor}>???</color>";
            }

            // Initialize the return string
            var returnStr = "";

            // Loop through every player in the lobby
            foreach (var playerCharacterMasterController in PlayerCharacterMasterController.instances)
            {
                var master = playerCharacterMasterController.master;
                // If they don't have a body or are the one who picked up the item, go to the next person
                if (!master.hasBody || master.GetBody() == body) continue;

                // If the player is dead/deadplayersgetitems is off, continue and add nothing
                if (master.IsDeadAndOutOfLivesServer() && !ShareSuite.DeadPlayersGetItems.Value) continue;

                // Get the player color
                var playerColor = GetPlayerColor(playerCharacterMasterController);

                // If the amount of players remaining is more then one (not the last)
                if (eligiblePlayers > 1)
                {
                    returnStr += $"<color=#{GrayColor}>,</color> ";
                }
                else if (eligiblePlayers == 1) // If it is the last player remaining
                {
                    returnStr += $"<color=#{GrayColor}>, and</color> ";
                }

                eligiblePlayers--;

                // If the player is alive OR dead and deadplayersgetitems is on, add their name to returnStr
                returnStr += $"<color=#{playerColor}>" + playerCharacterMasterController.GetDisplayName() + "</color>";
            }

            // Return the string
            return returnStr;
        }

        // Returns the player color as a hex string w/o the #
        private static string GetPlayerColor(PlayerCharacterMasterController controllerMaster)
        {
            var playerLocation = PlayerCharacterMasterController.instances.IndexOf(controllerMaster);
            return PlayerColors[playerLocation % 8];
        }

        private static int GetEligiblePlayers(CharacterBody body)
        {
            var eligiblePlayers = 0;

            foreach (var playerCharacterMasterController in PlayerCharacterMasterController.instances)
            {
                var master = playerCharacterMasterController.master;
                // If they don't have a body or are the one who picked up the item, go to the next person
                if (!master.hasBody || master.GetBody() == body) continue;

                // If the player is alive, add one to eligablePlayers
                if (!master.IsDeadAndOutOfLivesServer() || ShareSuite.DeadPlayersGetItems.Value)
                {
                    eligiblePlayers++;
                }
            }

            return eligiblePlayers;
        }

        public delegate void SendPickupMessageDelegate(CharacterMaster master, PickupIndex pickupIndex);

        public static void SendPickupMessage(CharacterMaster master, PickupIndex pickupIndex) 
        {
            var pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
            var message = new Chat.SubjectFormatChatMessage { subjectAsCharacterBody = master.GetBody(), baseToken = "PLAYER_PICKUP", paramTokens = new[] { pickupDef.nameToken } };
            Chat.SendBroadcastChat(message);
        }
    }
}