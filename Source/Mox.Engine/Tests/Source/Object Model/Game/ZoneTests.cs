﻿// Copyright (c) François Paradis
// This file is part of Mox, a card game simulator.
// 
// Mox is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// Mox is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Mox.  If not, see <http://www.gnu.org/licenses/>.
using System;
using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class ZoneTests : BaseGameTests
    {
        #region Variables

        private Zone m_zone;

        private EventSink<CardCollectionChangedEventArgs> m_cardCollectionChangedSink;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_zone = m_game.Zones.Battlefield;

            m_cardCollectionChangedSink = new EventSink<CardCollectionChangedEventArgs>();
            m_game.Zones.CardCollectionChanged += m_cardCollectionChangedSink;
        }

        public override void Teardown()
        {
            m_game.Zones.CardCollectionChanged -= m_cardCollectionChangedSink;

            base.Teardown();
        }

        #endregion

        #region Utilities

        private Card CreateCard(Player owner, Zone zone, string cardName = "My Card")
        {
            Card card = CreateCard(owner, cardName);
            card.Zone = zone;
            return card;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Constructor_values()
        {
            Assert.AreEqual("Battlefield", m_zone.Name);
        }

        [Test]
        public void Test_Default_zone_is_empty()
        {
            Assert.Collections.IsEmpty(m_zone.AllCards);
        }

        [Test]
        public void Test_Cards_of_the_zone_are_readonly()
        {
            Assert.IsTrue(m_zone.AllCards.IsReadOnly);
        }

        [Test]
        public void Test_Can_access_the_cards_controlled_by_each_player_in_the_zone()
        {
            Assert.Collections.IsEmpty(m_zone[m_playerA]);
            Assert.Collections.IsEmpty(m_zone[m_playerB]);
        }

        [Test]
        public void Test_Can_add_cards_to_the_zone()
        {
            Card card = CreateCard(m_playerA, m_game.Zones.PhasedOut);
            Assert.Collections.Contains(card, m_game.Zones.PhasedOut.AllCards);
            Assert.AreEqual(m_game.Zones.PhasedOut, card.Zone);
        }

        [Test]
        public void Test_Cards_are_in_the_collection_associated_with_their_controller()
        {
            Card card = CreateCard(m_playerB, m_game.Zones.PhasedOut);

            Assert.Collections.Contains(card, m_game.Zones.PhasedOut[m_playerB]);
            Assert.Collections.IsEmpty(m_game.Zones.PhasedOut[m_playerA]);
        }

        [Test]
        public void Test_Cards_change_collection_when_their_controller_changes()
        {
            Card card = CreateCard(m_playerA, m_game.Zones.PhasedOut);
            card.Controller = m_playerB;

            Assert.Collections.Contains(card, m_game.Zones.PhasedOut[m_playerB]);
            Assert.Collections.IsEmpty(m_game.Zones.PhasedOut[m_playerA]);
        }

        [Test]
        public void Test_Cards_change_controller_when_their_zone_changes()
        {
            Card card = CreateCard(m_playerA, m_game.Zones.PhasedOut);
            card.Controller = m_playerB;

            Assert.IsUndoRedoable(m_game.Controller, () =>
            {
                Assert.Collections.Contains(card, m_playerB.PhasedOut);
                Assert.Collections.IsEmpty(m_playerA.PhasedOut);
            },
            () => card.Zone = m_game.Zones.Library,
            () =>
            {
                Assert.Collections.Contains(card, m_playerA.Library);
                Assert.Collections.IsEmpty(m_playerB.PhasedOut);
            });
        }

        [Test]
        public void Test_Cards_retain_their_position_when_undoing_controller_change()
        {
            Card card1 = CreateCard(m_playerA, m_game.Zones.PhasedOut);
            Card card2 = CreateCard(m_playerA, m_game.Zones.PhasedOut);
            Card card3 = CreateCard(m_playerA, m_game.Zones.PhasedOut);

            Assert.IsUndoRedoable(m_game.Controller,
              () => Assert.Collections.AreEqual(new[] { card1, card2, card3 }, m_playerA.PhasedOut),
              () => card2.Controller = m_playerB,
              () =>
              {
                  Assert.Collections.AreEqual(new[] { card1, card3 }, m_playerA.PhasedOut);
                  Assert.Collections.AreEqual(new[] { card2 }, m_playerB.PhasedOut);
              });
        }

        [Test]
        public void Test_Cards_are_added_to_the_top_by_default()
        {
            Card card1 = CreateCard(m_playerA, m_game.Zones.PhasedOut);
            Card card2 = CreateCard(m_playerA, m_game.Zones.PhasedOut);

            Assert.AreEqual(card1, m_game.Zones.PhasedOut.AllCards[0]);
            Assert.AreEqual(card2, m_game.Zones.PhasedOut.AllCards[1]);
        }

        [Test]
        public void Test_Can_move_cards_from_one_zone_to_another()
        {
            Card card = CreateCard(m_playerA, m_game.Zones.PhasedOut);

            card.Zone = m_game.Zones.Exile;

            Assert.AreEqual(m_game.Zones.Exile, card.Zone);
            Assert.Collections.Contains(card, m_game.Zones.Exile.AllCards);
            Assert.Collections.Contains(card, m_game.Zones.Exile[m_playerA]);

            Assert.Collections.IsEmpty(m_game.Zones.PhasedOut.AllCards);
            Assert.Collections.IsEmpty(m_game.Zones.PhasedOut[m_playerA]);
        }

        [Test]
        public void Test_PropertyChanged_is_consistent_with_zone()
        {
            Card card = CreateCard(m_playerA, m_game.Zones.Hand);

            EventSink<PropertyChangedEventArgs> sink = new EventSink<PropertyChangedEventArgs>(card);
            bool called = false;

            sink.Callback += (s, e) =>
            {
                if (e.Property == Card.ZoneIdProperty)
                {
                    Assert.Collections.Contains(card, m_game.Zones.Battlefield.AllCards);
                    Assert.Collections.Contains(card, m_playerA.Battlefield);
                    called = true;
                }
            };

            try
            {
                card.PropertyChanged += sink;
                Assert.EventCalled(sink, () => card.Zone = m_game.Zones.Battlefield);
                Assert.That(called);
            }
            finally
            {
                card.PropertyChanged -= sink;
            }
        }

        [Test]
        public void Test_Can_moving_cards_from_one_zone_to_another_is_undoable()
        {
            Card card1 = CreateCard(m_playerA, m_game.Zones.PhasedOut);
            Card card2 = CreateCard(m_playerA, m_game.Zones.PhasedOut);
            Card card3 = CreateCard(m_playerA, m_game.Zones.PhasedOut);

            Assert.IsUndoRedoable(m_game.Controller,
              () => Assert.Collections.AreEqual(new[] { card1, card2, card3 }, m_playerA.PhasedOut),
              () => card2.Zone = m_game.Zones.Exile,
              () =>
              {
                  Assert.Collections.AreEqual(new[] { card1, card3 }, m_playerA.PhasedOut);
                  Assert.Collections.AreEqual(new[] { card2 }, m_playerA.Exile);
              });
        }

        [Test]
        public void Test_Adding_the_same_card_again_doesnt_do_anything()
        {
            Card card = CreateCard(m_playerA, m_game.Zones.PhasedOut);
            card.Zone = m_game.Zones.PhasedOut;

            Assert.Collections.AreEqual(new[] { card }, m_game.Zones.PhasedOut.AllCards);
            Assert.Collections.AreEqual(new[] { card }, m_game.Zones.PhasedOut[m_playerA]);
        }

        [Test]
        public void Test_ToString()
        {
            Assert.AreEqual("[Zone: Battlefield]", m_zone.ToString());
        }

        [Test]
        public void Test_When_removing_a_card_it_is_also_removed_from_the_zones()
        {
            Card card = CreateCard(m_playerA, m_game.Zones.PhasedOut);
            m_game.Cards.Remove(card);

            Assert.Collections.IsEmpty(m_game.Zones.PhasedOut.AllCards);
        }

        [Test]
        public void Test_When_undoing_the_removal_of_a_card_it_is_readded_to_its_zone_at_the_correct_index()
        {
            Card card1 = CreateCard(m_playerA, m_game.Zones.PhasedOut, "Card 1");
            Card card2 = CreateCard(m_playerA, m_game.Zones.PhasedOut, "Card 2");
            Card card3 = CreateCard(m_playerA, m_game.Zones.PhasedOut, "Card 3");

            object token = new object();

            card2.Manager.Controller.BeginTransaction(token);
            {
                m_game.Cards.Remove(card2);
            }
            card2.Manager.Controller.EndTransaction(true, token);

            Assert.Collections.AreEquivalent(new[] { card1, card2, card3 }, m_game.Zones.PhasedOut.AllCards); // All cards are not sorted
            Assert.Collections.AreEqual(new[] { card1, card2, card3 }, m_playerA.PhasedOut);
        }

        [Test]
        public void Test_Moving_works_even_if_zone_doesnt_change()
        {
            Card card1 = CreateCard(m_playerA, m_game.Zones.PhasedOut);
            Card card2 = CreateCard(m_playerA, m_game.Zones.PhasedOut);
            Card card3 = CreateCard(m_playerA, m_game.Zones.PhasedOut);

            m_playerA.PhasedOut.MoveToBottom(new[] {card3});

            Assert.Collections.AreEqual(new[] { card3, card1, card2 }, m_playerA.PhasedOut);
        }

        [Test]
        public void Test_Moving_a_card_to_where_its_already_at_does_not_produce_any_command()
        {
            Card card1 = CreateCard(m_playerA, m_game.Zones.PhasedOut);
            CreateCard(m_playerA, m_game.Zones.PhasedOut);
            CreateCard(m_playerA, m_game.Zones.PhasedOut);

            Assert.Produces(m_game.Controller, () => m_playerA.PhasedOut.MoveToBottom(new[] { card1 }), 0);
        }

        #region MoveToTop

        [Test]
        public void Test_MoveToTop_invalid_arguments()
        {
            Assert.Throws<ArgumentException>(() => m_game.Zones.Library[m_playerA].MoveToTop(null));
        }

        [Test]
        public void Test_MoveToTop()
        {
            Card otherCard = CreateCard(m_playerA, m_game.Zones.Hand);

            m_playerA.Hand.MoveToTop(new[] { m_card });

            Assert.Collections.IsEmpty(m_playerA.Library);
            Assert.Collections.AreEqual(new[] { otherCard, m_card }, m_playerA.Hand);
            Assert.Collections.AreEquivalent(new[] { otherCard, m_card }, m_game.Zones.Hand.AllCards);
        }

        [Test]
        public void Test_MoveToTop_with_multiple_cards()
        {
            Card otherCard = CreateCard(m_playerA);
            Assert.Collections.AreEqual(new[] { m_card, otherCard }, m_playerA.Library);

            Assert.IsAtomic(m_game.Controller, () => m_playerA.Hand.MoveToTop(m_playerA.Library)); // Make sure you can pass an enumerable that is modified during the operation

            Assert.Collections.IsEmpty(m_playerA.Library);
            Assert.Collections.AreEqual(new[] { m_card, otherCard }, m_playerA.Hand);
        }

        [Test]
        public void Test_MoveToTop_is_undoable()
        {
            Card otherCard = CreateCard(m_playerA);

            Assert.IsUndoRedoable(m_game.Controller,
                                  () =>
                                  {
                                      Assert.Collections.AreEqual(new[] { m_card, otherCard }, m_playerA.Library);
                                      Assert.Collections.IsEmpty(m_playerA.Hand);
                                  },
                                  () => m_playerA.Hand.MoveToTop(m_playerA.Library),
                                  () => Assert.Collections.IsEmpty(m_playerA.Library));
        }

        #endregion

        #region MoveToBottom

        [Test]
        public void Test_MoveToBottom_invalid_arguments()
        {
            Assert.Throws<ArgumentException>(() => m_game.Zones.Library[m_playerA].MoveToBottom(null));
        }

        [Test]
        public void Test_MoveToBottom()
        {
            Card otherCard = CreateCard(m_playerA, m_game.Zones.Hand);

            m_playerA.Hand.MoveToBottom(new[] { m_card });

            Assert.Collections.IsEmpty(m_playerA.Library);
            Assert.Collections.AreEqual(new[] { m_card, otherCard }, m_playerA.Hand);
            Assert.Collections.AreEquivalent(new[] { m_card, otherCard }, m_game.Zones.Hand.AllCards);
        }

        [Test]
        public void Test_Can_MoveToBottom_in_another_players_zone()
        {
            Card otherCard = CreateCard(m_playerA, m_game.Zones.Battlefield);
            otherCard.Controller = m_playerB;

            m_playerA.Library.MoveToBottom(new[] { m_card });
            Assert.Collections.AreEqual(new[] { m_card }, m_playerA.Library);
            Assert.Collections.AreEqual(new[] { m_card }, m_game.Zones.Library.AllCards);
        }

        [Test]
        public void Test_MoveToBottom_with_multiple_cards()
        {
            Card otherCard = CreateCard(m_playerA);
            Assert.Collections.AreEqual(new[] { m_card, otherCard }, m_playerA.Library);

            Assert.IsAtomic(m_game.Controller, () => m_playerA.Hand.MoveToBottom(m_playerA.Library)); // Make sure you can pass an enumerable that is modified during the operation

            Assert.Collections.IsEmpty(m_playerA.Library);
            Assert.Collections.AreEqual(new[] { otherCard, m_card }, m_playerA.Hand);
        }

        [Test]
        public void Test_MoveToBottom_is_undoable()
        {
            Card otherCard = CreateCard(m_playerA);

            Assert.IsUndoRedoable(m_game.Controller,
                                  () =>
                                  {
                                      Assert.Collections.AreEqual(new[] { m_card, otherCard }, m_playerA.Library);
                                      Assert.Collections.IsEmpty(m_playerA.Hand);
                                  },
                                  () => m_playerA.Hand.MoveToBottom(m_playerA.Library),
                                  () => Assert.Collections.IsEmpty(m_playerA.Library));
        }

        #endregion

        #region Shuffle

        [Test]
        public void Test_Shuffle()
        {
            Card otherCard = CreateCard(m_playerA, "Other");

            Assert.Collections.AreEqual(new[] { m_card, otherCard }, m_playerA.Library); // Sanity

            Assert.IsAtomic(m_game.Controller, () => m_playerA.Library.Shuffle());

            Assert.Collections.AreEqual(new[] { otherCard, m_card }, m_playerA.Library);
        }

        [Test]
        public void Test_Shuffle_is_undoable()
        {
            Card otherCard = CreateCard(m_playerA, "Other");

            Assert.IsUndoRedoable(m_game.Controller,
                                  () => Assert.Collections.AreEqual(new[] { m_card, otherCard }, m_playerA.Library),
                                  () => m_playerA.Library.Shuffle(),
                                  () => Assert.Collections.AreEqual(new[] { otherCard, m_card }, m_playerA.Library));
        }

        #endregion

        #region CardCollectionChanged event

        [Test]
        public void Test_Changing_the_zone_of_a_card_triggers_the_CardCollectionChanged_event()
        {
            Card card = CreateCard(m_playerA, m_game.Zones.Graveyard);

            Assert.EventCalledOnce(m_cardCollectionChangedSink, () =>
            {
                card.Zone = m_game.Zones.Battlefield;
            });

            Assert.AreEqual(CardCollectionChangedEventArgs.ChangeType.CardMoved, m_cardCollectionChangedSink.LastEventArgs.Type);
            Assert.AreEqual(m_playerA.Graveyard, m_cardCollectionChangedSink.LastEventArgs.OldCollection);
            Assert.AreEqual(m_playerA.Battlefield, m_cardCollectionChangedSink.LastEventArgs.NewCollection);
            Assert.AreEqual(card, m_cardCollectionChangedSink.LastEventArgs.Card);
            Assert.AreEqual(0, m_cardCollectionChangedSink.LastEventArgs.NewPosition);
        }

        [Test]
        public void Test_Changing_the_zone_of_a_card_triggers_the_CardCollectionChanged_event_when_undone()
        {
            Card card = CreateCard(m_playerA, m_game.Zones.Graveyard);

            Assert.Undo(m_game.Controller, () => card.Zone = m_game.Zones.Battlefield, undo =>
            {
                Assert.EventCalledOnce(m_cardCollectionChangedSink, () =>
                {
                    undo();
                });
            });

            Assert.AreEqual(CardCollectionChangedEventArgs.ChangeType.CardMoved, m_cardCollectionChangedSink.LastEventArgs.Type);
            Assert.AreEqual(m_playerA.Battlefield, m_cardCollectionChangedSink.LastEventArgs.OldCollection);
            Assert.AreEqual(m_playerA.Graveyard, m_cardCollectionChangedSink.LastEventArgs.NewCollection);
            Assert.AreEqual(card, m_cardCollectionChangedSink.LastEventArgs.Card);
            Assert.AreEqual(0, m_cardCollectionChangedSink.LastEventArgs.NewPosition);
        }

        [Test]
        public void Test_Changing_the_controller_of_a_card_triggers_the_CardCollectionChanged_event()
        {
            Card card = CreateCard(m_playerA, m_game.Zones.Battlefield);

            Assert.EventCalledOnce(m_cardCollectionChangedSink, () =>
            {
                card.Controller = m_playerB;
            });

            Assert.AreEqual(CardCollectionChangedEventArgs.ChangeType.CardMoved, m_cardCollectionChangedSink.LastEventArgs.Type);
            Assert.AreEqual(m_playerA.Battlefield, m_cardCollectionChangedSink.LastEventArgs.OldCollection);
            Assert.AreEqual(m_playerB.Battlefield, m_cardCollectionChangedSink.LastEventArgs.NewCollection);
            Assert.AreEqual(card, m_cardCollectionChangedSink.LastEventArgs.Card);
            Assert.AreEqual(0, m_cardCollectionChangedSink.LastEventArgs.NewPosition);
        }

        [Test]
        public void Test_Changing_the_controller_of_a_card_triggers_the_CardCollectionChanged_event_when_undone()
        {
            Card card = CreateCard(m_playerA, m_game.Zones.Battlefield);

            Assert.Undo(m_game.Controller, () => card.Controller = m_playerB, undo =>
            {
                Assert.EventCalledOnce(m_cardCollectionChangedSink, () =>
                {
                    undo();
                });
            });

            Assert.AreEqual(CardCollectionChangedEventArgs.ChangeType.CardMoved, m_cardCollectionChangedSink.LastEventArgs.Type);
            Assert.AreEqual(m_playerB.Battlefield, m_cardCollectionChangedSink.LastEventArgs.OldCollection);
            Assert.AreEqual(m_playerA.Battlefield, m_cardCollectionChangedSink.LastEventArgs.NewCollection);
            Assert.AreEqual(card, m_cardCollectionChangedSink.LastEventArgs.Card);
            Assert.AreEqual(0, m_cardCollectionChangedSink.LastEventArgs.NewPosition);
        }

        [Test]
        public void Test_Changing_the_controller_and_zone_of_a_card_triggers_the_CardCollectionChanged_event()
        {
            Card card = CreateCard(m_playerA, m_game.Zones.Battlefield);
            card.Controller = m_playerB;

            Assert.EventCalledOnce(m_cardCollectionChangedSink, () =>
            {
                card.Zone = m_game.Zones.Graveyard;
            });

            Assert.AreEqual(CardCollectionChangedEventArgs.ChangeType.CardMoved, m_cardCollectionChangedSink.LastEventArgs.Type);
            Assert.AreEqual(m_playerB.Battlefield, m_cardCollectionChangedSink.LastEventArgs.OldCollection);
            Assert.AreEqual(m_playerA.Graveyard, m_cardCollectionChangedSink.LastEventArgs.NewCollection);
            Assert.AreEqual(card, m_cardCollectionChangedSink.LastEventArgs.Card);
            Assert.AreEqual(0, m_cardCollectionChangedSink.LastEventArgs.NewPosition);
        }

        [Test]
        public void Test_Changing_the_controller_and_zone_of_a_card_triggers_the_CardCollectionChanged_event_when_undone()
        {
            Card card = CreateCard(m_playerA, m_game.Zones.Battlefield);
            card.Controller = m_playerB;

            Assert.Undo(m_game.Controller, () => card.Zone = m_game.Zones.Graveyard, undo =>
            {
                Assert.EventCalledOnce(m_cardCollectionChangedSink, () =>
                {
                    undo();
                });
            });

            Assert.AreEqual(CardCollectionChangedEventArgs.ChangeType.CardMoved, m_cardCollectionChangedSink.LastEventArgs.Type);
            Assert.AreEqual(m_playerA.Graveyard, m_cardCollectionChangedSink.LastEventArgs.OldCollection);
            Assert.AreEqual(m_playerB.Battlefield, m_cardCollectionChangedSink.LastEventArgs.NewCollection);
            Assert.AreEqual(card, m_cardCollectionChangedSink.LastEventArgs.Card);
            Assert.AreEqual(0, m_cardCollectionChangedSink.LastEventArgs.NewPosition);
        }

        [Test]
        public void Test_Move_triggers_the_CardCollectionChanged_event_once_per_card()
        {
            Card card1 = CreateCard(m_playerA, m_game.Zones.Graveyard);
            Card card2 = CreateCard(m_playerA, m_game.Zones.Graveyard);
            Card card3 = CreateCard(m_playerA, m_game.Zones.Graveyard);

            Assert.Collections.AreEqual(new[] { card1, card2, card3 }, m_playerA.Graveyard); // Sanity

            Assert.EventCalled(m_cardCollectionChangedSink, () =>
            {
                m_playerA.Graveyard.MoveToTop(new[] { card1, card2 });
            }, 2);

            Assert.AreEqual(CardCollectionChangedEventArgs.ChangeType.CardMoved, m_cardCollectionChangedSink.EventArgs[0].Type);
            Assert.AreEqual(m_playerA.Graveyard, m_cardCollectionChangedSink.EventArgs[0].OldCollection);
            Assert.AreEqual(m_playerA.Graveyard, m_cardCollectionChangedSink.EventArgs[0].NewCollection);
            Assert.AreEqual(card1, m_cardCollectionChangedSink.EventArgs[0].Card);
            Assert.AreEqual(2, m_cardCollectionChangedSink.EventArgs[0].NewPosition);

            Assert.AreEqual(CardCollectionChangedEventArgs.ChangeType.CardMoved, m_cardCollectionChangedSink.EventArgs[1].Type);
            Assert.AreEqual(m_playerA.Graveyard, m_cardCollectionChangedSink.EventArgs[1].OldCollection);
            Assert.AreEqual(m_playerA.Graveyard, m_cardCollectionChangedSink.EventArgs[1].NewCollection);
            Assert.AreEqual(card2, m_cardCollectionChangedSink.EventArgs[1].Card);
            Assert.AreEqual(2, m_cardCollectionChangedSink.EventArgs[1].NewPosition);
        }

        [Test]
        public void Test_Move_triggers_the_CardCollectionChanged_event_when_undone()
        {
            Card card1 = CreateCard(m_playerA, m_game.Zones.Graveyard);
            Card card2 = CreateCard(m_playerA, m_game.Zones.Graveyard);
            Card card3 = CreateCard(m_playerA, m_game.Zones.Graveyard);

            Assert.Collections.AreEqual(new[] { card1, card2, card3 }, m_playerA.Graveyard); // Sanity

            Assert.Undo(m_game.Controller, () => m_playerA.Graveyard.MoveToTop(new[] { card2 }), undo =>
            {
                Assert.EventCalledOnce(m_cardCollectionChangedSink, () =>
                {
                    undo();
                });
            });

            Assert.AreEqual(CardCollectionChangedEventArgs.ChangeType.CardMoved, m_cardCollectionChangedSink.LastEventArgs.Type);
            Assert.AreEqual(m_playerA.Graveyard, m_cardCollectionChangedSink.LastEventArgs.OldCollection);
            Assert.AreEqual(m_playerA.Graveyard, m_cardCollectionChangedSink.LastEventArgs.NewCollection);
            Assert.AreEqual(card2, m_cardCollectionChangedSink.LastEventArgs.Card);
            Assert.AreEqual(1, m_cardCollectionChangedSink.LastEventArgs.NewPosition);
        }

        [Test]
        public void Test_Shuffle_triggers_the_CardCollectionChanged_event()
        {
            Card otherCard = CreateCard(m_playerA, "Other");
            Assert.Collections.AreEqual(new[] { m_card, otherCard }, m_playerA.Library); // Sanity

            Assert.EventCalledOnce(m_cardCollectionChangedSink, () =>
            {
                m_playerA.Library.Shuffle();
            });

            Assert.AreEqual(CardCollectionChangedEventArgs.ChangeType.Shuffle, m_cardCollectionChangedSink.LastEventArgs.Type);
            Assert.AreEqual(m_playerA.Library, m_cardCollectionChangedSink.LastEventArgs.OldCollection);
            Assert.AreEqual(m_playerA.Library, m_cardCollectionChangedSink.LastEventArgs.NewCollection);
        }

        [Test]
        public void Test_Shuffle_triggers_the_CardCollectionChanged_event_when_undone()
        {
            Card otherCard = CreateCard(m_playerA, "Other");
            Assert.Collections.AreEqual(new[] { m_card, otherCard }, m_playerA.Library); // Sanity

            Assert.Undo(m_game.Controller, () => m_playerA.Library.Shuffle(), undo =>
            {
                Assert.EventCalledOnce(m_cardCollectionChangedSink, () =>
                {
                    undo();
                });
            });

            Assert.AreEqual(CardCollectionChangedEventArgs.ChangeType.Shuffle, m_cardCollectionChangedSink.LastEventArgs.Type);
            Assert.AreEqual(m_playerA.Library, m_cardCollectionChangedSink.LastEventArgs.OldCollection);
            Assert.AreEqual(m_playerA.Library, m_cardCollectionChangedSink.LastEventArgs.NewCollection);
        }

        #endregion

        #endregion
    }
}
