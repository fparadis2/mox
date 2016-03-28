﻿using System;
using System.Collections.Generic;
using Mox.Database;
using Mox.Lobby;

namespace Mox.UI.Lobby
{
    public class LobbyUserSettings
    {
        public readonly List<LobbyFormatUserSettings> AllSettings = new List<LobbyFormatUserSettings>();

        public static LobbyFormatUserSettings GetFormatSettings(ILobby lobby)
        {
            if (lobby == null)
                return null;

            var format = lobby.Parameters.DeckFormat;
            if (format == null)
                return null;

            var settings = Settings.Get<LobbyUserSettings>();
            return settings.GetFormatSettings(format.Name);
        }

        public static void Save(ILobby lobby)
        {
            if (lobby == null)
                return;

            var format = lobby.Parameters.DeckFormat;
            if (format == null)
                return;

            var settings = Settings.Get<LobbyUserSettings>();
            var formatSettings = settings.GetOrCreateFormatSettings(format.Name);
            formatSettings.Save(lobby);

            Settings.Save<LobbyUserSettings>();
        }

        private LobbyFormatUserSettings GetFormatSettings(string deckFormat)
        {
            if (string.IsNullOrEmpty(deckFormat))
                return null;

            foreach (var settings in AllSettings)
            {
                if (string.Equals(settings.DeckFormat, deckFormat, StringComparison.OrdinalIgnoreCase))
                {
                    return settings;
                }
            }

            return null;
        }

        private LobbyFormatUserSettings GetOrCreateFormatSettings(string deckFormat)
        {
            var settings = GetFormatSettings(deckFormat);

            if (settings == null)
            {
                settings = new LobbyFormatUserSettings(deckFormat);
                AllSettings.Add(settings);
            }

            return settings;
        }

        public static void Load(LobbyViewModel lobby)
        {
            var settings = GetFormatSettings(lobby.Source);
            if (settings == null)
                return;

            settings.Load(lobby);
        }
    }

    public class LobbyFormatUserSettings
    {
        public readonly string DeckFormat;

        public string Deck;
        public readonly List<string> BotDecks = new List<string>();

        public LobbyFormatUserSettings(string deckFormat)
        {
            DeckFormat = deckFormat;
        }

        internal void Save(ILobby lobby)
        {
            BotDecks.Clear();
            Deck = null;

            foreach (var slot in lobby.Slots)
            {
                if (slot.PlayerId == lobby.LocalUserId)
                {
                    Deck = slot.DeckName;
                }
                else if (!slot.IsAssigned && lobby.LeaderId == lobby.LocalUserId)
                {
                    BotDecks.Add(slot.DeckName);
                }
            }
        }

        internal void Load(LobbyViewModel lobby)
        {
            int botIndex = 0;

            foreach (var slot in lobby.Slots)
            {
                if (slot.IsLocalPlayer)
                {
                    SetDeck(slot, Deck);
                }
                else if (lobby.IsLeader && slot.CanChangeSlot)
                {
                    if (botIndex < BotDecks.Count)
                    {
                        string botDeck = BotDecks[botIndex++];
                        SetDeck(slot, botDeck);
                    }
                }
            }
        }

        private static void SetDeck(LobbyPlayerSlotViewModel slot, string deckName)
        {
            if (string.IsNullOrEmpty(deckName))
                return;

            if (slot.Deck.Deck != null && string.Equals(slot.Deck.Deck.Name, deckName, StringComparison.OrdinalIgnoreCase))
                return;

            IDeck deck = MasterDeckLibrary.Instance.GetDeck(deckName);
            if (deck == null)
                return;

            slot.Deck = new LobbyPlayerSlotViewModel.DeckChoiceViewModel(deck);
        }
    }
}
