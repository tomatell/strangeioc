using System;
using System.Collections.Generic;
using System.Reflection;
using strange.extensions.reflector.api;
using strange.framework.api;
using strange.framework.impl;
using System.Collections;
using System.Linq;

namespace strange.framework.api
{
    public static class TypeInfoEx
    {
#if NETFX_CORE
        public static MethodInfo[] GetMethods(this Type type)
        {
            var methods = new List<MethodInfo>();

            while (true)
            {
                methods.AddRange(type.GetTypeInfo().DeclaredMethods);

                Type type2 = type.GetTypeInfo().BaseType;

                if (type2 == null)
                {
                    break;
                }

                type = type2;
            }

            return methods.ToArray();
        }

        public static MethodInfo[] GetPublicMethods(this Type type)
        {
            var methods = new List<MethodInfo>();

            while (true)
            {
                methods.AddRange(type.GetTypeInfo().DeclaredMethods.Where(m => m.IsPublic));

                Type type2 = type.GetTypeInfo().BaseType;

                if (type2 == null)
                {
                    break;
                }

                type = type2;
            }

            return methods.ToArray();
        }

        public static MethodInfo[] GetPrivateMethods(this Type type)
        {
            var methods = new List<MethodInfo>();

            while (true)
            {
                methods.AddRange(type.GetTypeInfo().DeclaredMethods.Where(m => !m.IsPublic));

                Type type2 = type.GetTypeInfo().BaseType;

                if (type2 == null)
                {
                    break;
                }

                type = type2;
            }

            return methods.ToArray();
        }

        public static MemberInfo[] GetMembers(this Type type)
        {
            var members = new List<MemberInfo>();

            while (true)
            {
                members.AddRange(type.GetTypeInfo().DeclaredMembers.OfType<MethodBase>());

                Type type2 = type.GetTypeInfo().BaseType;

                if (type2 == null)
                {
                    break;
                }

                type = type2;
            }

            return members.ToArray();
        }

        public static MemberInfo[] GetPublicMembers(this Type type)
        {
            var members = new List<MemberInfo>();

            while (true)
            {
                members.AddRange(type.GetTypeInfo().DeclaredMembers.OfType<MethodBase>().Where(m => m.IsPublic));

                Type type2 = type.GetTypeInfo().BaseType;

                if (type2 == null)
                {
                    break;
                }

                type = type2;
            }

            return members.ToArray();
        }

        public static MemberInfo[] GetPrivateMembers(this Type type)
        {
            var members = new List<MemberInfo>();

            while (true)
            {
                members.AddRange(type.GetTypeInfo().DeclaredMembers.OfType<MethodBase>().Where(m => !m.IsPublic));

                Type type2 = type.GetTypeInfo().BaseType;

                if (type2 == null)
                {
                    break;
                }

                type = type2;
            }

            return members.ToArray();
        }

        public static ConstructorInfo[] GetPublicConstuctors(this Type type)
        {
            var constructors = new List<ConstructorInfo>();

            while (true)
            {
                constructors.AddRange(type.GetTypeInfo().DeclaredConstructors.Where(m => m.IsPublic));

                Type type2 = type.GetTypeInfo().BaseType;

                if (type2 == null)
                {
                    break;
                }

                type = type2;
            }

            return constructors.ToArray();
        }
#endif
    }
}
