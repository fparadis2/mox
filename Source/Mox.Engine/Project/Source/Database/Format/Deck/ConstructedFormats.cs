﻿using Mox.Database;

namespace Mox
{
    internal abstract class ConstructedDeckFormat : IDeckFormat
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public virtual int MinimumCardCount { get { return 40; } } // TODO: Put back at 60... all my example decks are 40-card starter decks

        public virtual bool Validate(IDeck deck)
        {
            return deck != null && deck.Cards.Count >= MinimumCardCount;
        }
    }

    internal class StandardDeckFormat : ConstructedDeckFormat
    {
        public override string Name
        {
            get { return "Standard"; }
        }

        public override string Description
        {
            get { return "Standard is a constructed format where only the newest sets can be used."; }
        }
    }

    internal class ModernDeckFormat : ConstructedDeckFormat
    {
        public override string Name
        {
            get { return "Modern"; }
        }

        public override string Description
        {
            get { return "Modern is a constructed format that allows expansion sets and core sets from Eighth Edition forward."; }
        }
    }

    internal class LegacyDeckFormat : ConstructedDeckFormat
    {
        public override string Name
        {
            get { return "Legacy"; }
        }

        public override string Description
        {
            get { return "Legacy is a constructed format that allows any card from any set to be used, expect for a list of specifically banned cards."; }
        }
    }
}