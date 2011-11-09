namespace Mox.Flow
{
    public interface ISpellEffectPart
    {
    }

    public static class SpellEffectPartExtensions
    {
        private const string SpellToken = "SpellEffectPart_Spell";

        public static Spell PeekSpell(this ISpellEffectPart part, NewPart.Context context)
        {
            return context.PeekArgument<Spell>(SpellToken).Resolve(context.Game, false);
        }

        public static Spell PopSpell(this ISpellEffectPart part, NewPart.Context context)
        {
            return context.PopArgument<Spell>(SpellToken).Resolve(context.Game, false);
        }

        internal static void PushSpell(this ISpellEffectPart part, NewPart.Context context, Spell spell)
        {
            context.PushArgument(spell, SpellToken);
        }
    }
}