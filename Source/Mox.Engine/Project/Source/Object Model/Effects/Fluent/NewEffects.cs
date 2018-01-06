// Copyright (c) François Paradis
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

using Mox.Abilities;
using Mox.Effects;

namespace Mox
{
    /// <summary>
    /// Used to add effects on objects.
    /// </summary>
    public static class AddEffect
    {
        #region Host & Condition

        /// <summary>
        /// Adds an effect on a specific object.
        /// </summary>
        public static ILocalEffectHost<T> On<T>(T obj)
            where T : GameObject
        {
            return new LocalEffectHost<T>(obj);
        }

        /// <summary>
        /// Adds an effect on all cards in the battlefield, given the condition.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static IEffectHost<Card> OnCards(Game game, Condition condition)
        {
            return new GlobalEffectHost(game, condition, game.Zones.Battlefield);
        }

        /// <summary>
        /// Adds an effect on all creatures in the battlefield, given the condition.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static IEffectHost<Card> OnCreatures(Game game, Condition condition)
        {
            return OnCards(game, Condition.Is(Type.Creature) & condition);
        }

        #endregion
    }

    public static class EffectExtensions
    {
        #region Effect

        /// <summary>
        /// Modifies the color of the card.
        /// </summary>
        /// <returns></returns>
        public static IEffectCreator ChangeColor(this IEffectHost<Card> host, Color newColor)
        {
            return new EffectCreator(host, new ChangeColorEffect(newColor));
        }

        /// <summary>
        /// Gains ABC until XYZ
        /// </summary>
        /// <typeparam name="TAbility"></typeparam>
        /// <param name="host"></param>
        /// <returns></returns>
        public static IEffectCreator GainAbility<TAbility>(this ILocalEffectHost<Card> host)
            where TAbility : Ability, new()
        {
            return new AbilityEffectCreator<TAbility>(host);
        }

        /// <summary>
        /// Modifies the controller of the card.
        /// </summary>
        /// <returns></returns>
        public static IEffectCreator GainControl(this IEffectHost<Card> host, Player newController)
        {
            return new EffectCreator(host, new ChangeControllerEffect(newController));
        }

        /// <summary>
        /// Modifies power / toughness (+1/+1, +3/-2, etc...)
        /// </summary>
        /// <returns></returns>
        public static IEffectCreator ModifyPowerAndToughness(this IEffectHost<Card> host, int power, int toughness)
        {
            return new EffectCreator(host, new ModifyPowerAndToughnessEffect(power, toughness));
        }

        /// <summary>
        /// Switches power / toughness
        /// </summary>
        /// <returns></returns>
        public static IEffectCreator SwitchPowerAndToughness(this IEffectHost<Card> host)
        {
            return new EffectCreator(host, new SwitchPowerAndToughnessEffect());
        }

        /// <summary>
        /// Sets power / toughness
        /// </summary>
        /// <returns></returns>
        public static IEffectCreator SetPowerAndToughness(this IEffectHost<Card> host, int power, int toughness)
        {
            return new EffectCreator(host, new SetPowerAndToughnessEffect(power, toughness));
        }

        /// <summary>
        /// Sets power / toughness
        /// </summary>
        /// <returns></returns>
        public static IEffectCreator SetPowerAndToughness(this IEffectHost<Card> host, Func<Object, PowerAndToughness> provider, params PropertyBase[] invalidateProperties)
        {
            return new EffectCreator(host, new SetLazyPowerAndToughnessEffect(provider, invalidateProperties));
        }

        #endregion

        #region Scope

        /// <summary>
        /// Effect is never removed.
        /// </summary>
        /// <param name="creator"></param>
        public static void Forever(this IEffectCreator creator)
        {
            creator.Create();
        }

        /// <summary>
        /// Effect is removed at the end of turn.
        /// </summary>
        /// <param name="creator"></param>
        public static void UntilEndOfTurn(this IEffectCreator creator)
        {
            creator.CreateScoped<UntilEndOfTurnScope>();
        }

        public static IDisposable DuringScope(this IEffectCreator creator)
        {
            Object effectInstance = creator.Create();
            return new DisposableHelper(effectInstance.Remove);
        }

        #endregion
    }

    public interface IEffectHost
    {
        Object Create(EffectBase effect);

        Object CreateScoped<TScope>(EffectBase effect) where TScope : IObjectScope, new();
    }

    public interface IEffectHost<T> : IEffectHost
        where T : GameObject
    {
    }

    public interface ILocalEffectHost<T> : IEffectHost<T>
        where T : GameObject
    {
        T Target { get; }
    }

    public interface IEffectCreator
    {
        #region Methods

        Object Create();

        Object CreateScoped<TScope>() where TScope : IObjectScope, new();

        #endregion
    }
}

namespace Mox.Effects
{
    internal class LocalEffectHost<T> : ILocalEffectHost<T>
        where T : GameObject
    {
        private readonly T m_target;

        public LocalEffectHost(T target)
        {
            m_target = target;
        }

        public T Target
        {
            get { return m_target; }
        }

        public Object Create(EffectBase effect)
        {
            return m_target.Manager.CreateLocalEffect(m_target, effect);
        }

        public Object CreateScoped<TScope>(EffectBase effect) where TScope : IObjectScope, new()
        {
            return m_target.Manager.CreateScopedLocalEffect<TScope>(m_target, effect);
        }
    }

    internal class GlobalEffectHost : IEffectHost<Card>
    {
        private readonly Game m_game;
        private readonly Condition m_condition;
        private readonly Zone m_zone;

        public GlobalEffectHost(Game game, Condition condition, Zone zone)
        {
            m_game = game;
            m_condition = condition;
            m_zone = zone;
        }

        public Object Create(EffectBase effect)
        {
            return m_game.CreateTrackingEffect(effect, m_condition, m_zone);
        }

        public Object CreateScoped<TScope>(EffectBase effect) where TScope : IObjectScope, new()
        {
            return m_game.CreateScopedTrackingEffect<TScope>(effect, m_condition, m_zone);
        }
    }

    internal class EffectCreator : IEffectCreator
    {
        #region Variables

        private readonly IEffectHost m_host;
        private readonly EffectBase m_effect;

        #endregion

        #region Constructor

        public EffectCreator(IEffectHost host, EffectBase effect)
        {
            m_host = host;
            m_effect = effect;
        }

        #endregion

        #region Properties

        public EffectBase Effect
        {
            get { return m_effect; }
        }

        #endregion

        #region Methods

        public Object Create()
        {
            return m_host.Create(Effect);
        }

        public Object CreateScoped<TScope>()
             where TScope : IObjectScope, new()
        {
            return m_host.CreateScoped<TScope>(Effect);
        }

        #endregion
    }

    internal class AbilityEffectCreator<TAbility> : IEffectCreator
        where TAbility : Ability, new()
    {
        #region Variables

        private readonly ILocalEffectHost<Card> m_host;

        #endregion

        #region Constructor

        public AbilityEffectCreator(ILocalEffectHost<Card> host)
        {
            m_host = host;
        }

        #endregion

        #region Methods

        public Object Create()
        {
            return m_host.Target.Manager.CreateAbility<TAbility>(m_host.Target);
        }

        public Object CreateScoped<TScope>()
             where TScope : IObjectScope, new()
        {
            return m_host.Target.Manager.CreateScopedAbility<TAbility, TScope>(m_host.Target);
        }

        #endregion
    }
}
