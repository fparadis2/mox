using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Mox
{
    public interface IObjectManipulator
    {
        void ComputeHash(Object obj, Hash hash, ObjectHash hasher);
    }

    public static class ObjectManipulators
    {
        #region Singleton

        private static readonly Dictionary<Type, IObjectManipulator> ms_manipulators = new Dictionary<Type, IObjectManipulator>();
        private static readonly ReadWriteLock ms_lock = ReadWriteLock.CreateNoRecursion();

        public static IObjectManipulator GetManipulator(Object obj)
        {
            Type type = obj.GetType();
            IObjectManipulator manipulator = null;

            using (ms_lock.Read)
            {
                if (ms_manipulators.TryGetValue(type, out manipulator))
                    return manipulator;
            }

            using (ms_lock.Write)
            {
                if (!ms_manipulators.TryGetValue(type, out manipulator))
                {
                    manipulator = new Manipulator(type);
                    ms_manipulators.Add(type, manipulator);
                }
                return manipulator;
            }
        }

        #endregion

        #region Generation

        private class Manipulator : IObjectManipulator
        {
            private delegate void ComputeHashDelegate(object instance, Hash hash, ObjectHash hasher);

            private readonly ComputeHashDelegate m_computeHashDelegate;

            public Manipulator(Type objectType)
            {
                m_computeHashDelegate = Generate_ComputeHash(objectType);
            }

            public void ComputeHash(Object instance, Hash hash, ObjectHash hasher)
            {
                m_computeHashDelegate(instance, hash, hasher);
            }

            private static ComputeHashDelegate Generate_ComputeHash(Type objectType)
            {
                // Generate something like:
                // public void ComputeHash(object instance, Hash hash, ObjectHash hasher)
                // {
                //     MyClass obj = (MyClass)instance;
                //     hash.Add(obj.MyField);
                // }

                DynamicMethod method = new DynamicMethod("ComputeHash", typeof(void), new[] { typeof(object), typeof(Hash), typeof(ObjectHash) }, objectType, true);
                
                ILGenerator ilGenerator = method.GetILGenerator();

                var instanceLocal = ilGenerator.DeclareLocal(objectType);
                var hashArgument = OpCodes.Ldarg_1;
                var hasherArgument = OpCodes.Ldarg_2;

                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Castclass, objectType);
                ilGenerator.Emit(OpCodes.Stloc, instanceLocal);

                foreach (var property in PropertyBase.GetAllProperties(objectType))
                {
                    if (property.Flags.HasFlag(PropertyFlags.IgnoreHash))
                        continue;

                    if (property.ValueType == typeof(int))
                    {
                        ilGenerator.Emit(hashArgument);

                        ilGenerator.Emit(OpCodes.Ldloc, instanceLocal);
                        ilGenerator.Emit(OpCodes.Ldfld, property.BackingField);

                        ilGenerator.Emit(OpCodes.Call, GetMethod<Hash>(h => h.Add((int)0)));
                    }
                    else if (property.ValueType == typeof(int[]))
                    {
                        ilGenerator.Emit(hashArgument);

                        ilGenerator.Emit(OpCodes.Ldloc, instanceLocal);
                        ilGenerator.Emit(OpCodes.Ldfld, property.BackingField);

                        ilGenerator.Emit(OpCodes.Call, GetStaticMethod(() => HashArrayOfInt(null, null)));
                    }
                    else if (property.ValueType == typeof(bool))
                    {
                        ilGenerator.Emit(hashArgument);

                        ilGenerator.Emit(OpCodes.Ldloc, instanceLocal);
                        ilGenerator.Emit(OpCodes.Ldfld, property.BackingField);

                        ilGenerator.Emit(OpCodes.Call, GetMethod<Hash>(h => h.Add(false)));
                    }
                    else if (property.ValueType == typeof(string))
                    {
                        ilGenerator.Emit(hashArgument);

                        ilGenerator.Emit(OpCodes.Ldloc, instanceLocal);
                        ilGenerator.Emit(OpCodes.Ldfld, property.BackingField);

                        ilGenerator.Emit(OpCodes.Call, GetMethod<Hash>(h => h.Add(string.Empty)));
                    }
                    else if (property.ValueType.IsEnum)
                    {
                        ilGenerator.Emit(hashArgument);

                        ilGenerator.Emit(OpCodes.Ldloc, instanceLocal);
                        ilGenerator.Emit(OpCodes.Ldfld, property.BackingField);

                        ilGenerator.Emit(OpCodes.Call, GetMethod<Hash>(h => h.Add((int)0)));
                    }
                    else if (typeof (Object).IsAssignableFrom(property.ValueType))
                    {
                        ilGenerator.Emit(hashArgument);
                        ilGenerator.Emit(hasherArgument);

                        ilGenerator.Emit(OpCodes.Ldloc, instanceLocal);
                        ilGenerator.Emit(OpCodes.Ldfld, property.BackingField);

                        ilGenerator.Emit(OpCodes.Call, GetStaticMethod(() => HashObjectValue(null, null, null)));
                    }
                    else if (typeof (IHashable).IsAssignableFrom(property.ValueType))
                    {
                        ilGenerator.Emit(hashArgument);
                        ilGenerator.Emit(hasherArgument);

                        ilGenerator.Emit(OpCodes.Ldloc, instanceLocal);
                        ilGenerator.Emit(OpCodes.Ldfld, property.BackingField);

                        if (property.ValueType.IsValueType)
                            ilGenerator.Emit(OpCodes.Box, property.ValueType);

                        ilGenerator.Emit(OpCodes.Call, GetStaticMethod(() => HashIHashable(null, null, null)));
                    }
                    else
                    {
                        throw new NotImplementedException(string.Format("Hashing {0} is not implemented (in property {1}.{2})", property.ValueType, property.OwnerType.Name, property.Name));
                    }
                }

                ilGenerator.Emit(OpCodes.Ret);

                return (ComputeHashDelegate)method.CreateDelegate(typeof(ComputeHashDelegate));
            }
        }

        private static MethodInfo GetMethod<TObject>(Expression<Action<TObject>> expression)
        {
            var methodExpression = (MethodCallExpression)expression.Body;
            return methodExpression.Method;
        }

        private static MethodInfo GetStaticMethod(Expression<Action> expression)
        {
            var methodExpression = (MethodCallExpression)expression.Body;
            return methodExpression.Method;
        }

        private static void HashObjectValue(Hash hash, ObjectHash hasher, Object value)
        {
            hash.Add(hasher.Hash(value));
        }

        private static void HashIHashable(Hash hash, ObjectHash hasher, IHashable value)
        {
            if (ReferenceEquals(value, null))
            {
                hash.Add(0);
            }
            else
            {
                value.ComputeHash(hash, hasher);
            }
        }

        private static void HashArrayOfInt(Hash hash, int[] array)
        {
            if (array == null)
                return;

            foreach (int value in array)
                hash.Add(value);
        }

        #endregion
    }
}
