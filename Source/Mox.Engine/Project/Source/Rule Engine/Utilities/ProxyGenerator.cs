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
using System.Collections.Generic;
using System.Threading;
using Castle.Core.Interceptor;
using Castle.DynamicProxy;

using ProxyTypeWithTargetCache = System.Collections.Generic.Dictionary<System.Type, System.Type>;

namespace Mox
{
    internal static class ProxyGenerator<TProxy>
    {
        #region Variables

        private static readonly ProxyGenerator m_proxyGenerator = new ProxyGenerator();

        private static volatile ProxyTypeWithTargetCache m_proxyTypesWithTarget = new ProxyTypeWithTargetCache();
        private static System.Type m_proxyTypeWithoutTarget;

        #endregion

        #region Methods

        private static System.Type GetProxyTypeWithTarget(System.Type targetType)
        {
            ProxyTypeWithTargetCache cache = m_proxyTypesWithTarget;

            System.Type proxyType;
            if (!cache.TryGetValue(targetType, out proxyType))
            {
                return CopyOnWrite(cache, targetType, m_proxyGenerator.ProxyBuilder.CreateInterfaceProxyTypeWithTarget(typeof(TProxy), null, targetType, ProxyGenerationOptions.Default));
            }
            return proxyType;
        }

        private static System.Type CopyOnWrite(ProxyTypeWithTargetCache cache, System.Type key, System.Type value)
        {
            Dictionary<System.Type, System.Type> copy = new Dictionary<System.Type, System.Type>();
            cache.ForEach(kvp => copy.Add(kvp.Key, kvp.Value));
            copy.Add(key, value);
#pragma warning disable 420
            Interlocked.CompareExchange(ref m_proxyTypesWithTarget, copy, cache);
#pragma warning restore 420
            return value;
        }

        public static TProxy CreateInterfaceProxyWithTarget(TProxy target, params IInterceptor[] interceptors)
        {
            System.Type proxyType = GetProxyTypeWithTarget(target.GetType());
            return (TProxy)Activator.CreateInstance(proxyType, ConstructArguments(target, interceptors));
        }

        public static TProxy CreateInterfaceProxyWithoutTarget(params IInterceptor[] interceptors)
        {
            if (m_proxyTypeWithoutTarget == null)
            {
                m_proxyTypeWithoutTarget = m_proxyGenerator.ProxyBuilder.CreateInterfaceProxyTypeWithoutTarget(typeof(TProxy), null, ProxyGenerationOptions.Default);
            }

            return (TProxy)Activator.CreateInstance(m_proxyTypeWithoutTarget, ConstructArguments(new object(), interceptors));
        }

        private static object[] ConstructArguments(object target, IInterceptor[] interceptors)
        {
            return new[] { interceptors, target };
        }

        #endregion
    }
}
