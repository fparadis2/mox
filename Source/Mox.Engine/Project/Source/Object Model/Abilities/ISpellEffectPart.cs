using Mox.Abilities;

namespace Mox.Flow
{
    public interface ISpellEffectPart
    {
    }

    public static class SpellEffectPartExtensions
    {
        private const string SpellToken = "SpellEffectPart_Spell";

        public static Spell PeekSpell(this ISpellEffectPart part, Sequencer sequencer)
        {
            return sequencer.PeekArgument<Spell>(SpellToken).Resolve(sequencer.Game, false);
        }

        public static Spell PopSpell(this ISpellEffectPart part, Part.Context context)
        {
            return context.PopArgument<Spell>(SpellToken).Resolve(context.Game, false);
        }

        internal static void PushSpell(this ISpellEffectPart part, Part.Context context, Spell spell)
        {
            context.PushArgument(spell, SpellToken);
        }
    }
}