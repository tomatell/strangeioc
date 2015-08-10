using System;
using System.Collections.Generic;
using System.Reflection;
using strange.extensions.reflector.api;
using strange.framework.api;
using strange.framework.impl;
using System.Collections;
using System.Linq;
using System.Reflection.Emit;


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

        public static List<T> GetMembers<T>(Type type, BindingFlags flags)
            where T : MemberInfo
        {
            var results = new List<T>();

            var info = type.GetTypeInfo();
            bool inParent = false;
            while (true)
            {
                foreach (T member in info.DeclaredMembers.Where(v => typeof(T).IsAssignableFrom(v.GetType())))
                {
                    if (member.CheckBindings(flags, inParent))
                        results.Add(member);
                }

                // constructors never walk the hierarchy...
                if (typeof(T) == typeof(ConstructorInfo))
                    break;

                // up...
                if (info.BaseType == null)
                    break;
                info = info.BaseType.GetTypeInfo();
                inParent = true;
            }

            return results;
        }
        public static bool IsAssignableFrom(this Type type, Type toCheck)
        {
            return type.GetTypeInfo().IsAssignableFrom(toCheck.GetTypeInfo());
        }

        public static bool CheckBindings(this MemberInfo member, BindingFlags flags, bool inParent)
        {
            if ((member.IsStatic() & (flags & BindingFlags.Static) == BindingFlags.Static) ||
                (!(member.IsStatic()) & (flags & BindingFlags.Instance) == BindingFlags.Instance))
            {
                // if we're static and we're in parent, and we haven't specified flatten hierarchy, we can't match...
                if (inParent && (int)(flags & BindingFlags.FlattenHierarchy) == 0 && member.IsStatic())
                    return false;

                if ((member.IsPublic() & (flags & BindingFlags.Public) == BindingFlags.Public) ||
                    (!(member.IsPublic()) & (flags & BindingFlags.NonPublic) == BindingFlags.NonPublic))
                {
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }
        public static bool IsStatic(this MemberInfo member)
        {
            if (member is MethodBase)
                return ((MethodBase)member).IsStatic;
            else if (member is PropertyInfo)
            {
                PropertyInfo prop = (PropertyInfo)member;
                return (prop.GetMethod != null && prop.GetMethod.IsStatic) || (prop.SetMethod != null && prop.SetMethod.IsStatic);
            }
            else if (member is FieldInfo)
                return ((FieldInfo)member).IsStatic;
            else if (member is EventInfo)
            {
                EventInfo evt = (EventInfo)member;
                return (evt.AddMethod != null && evt.AddMethod.IsStatic) || (evt.RemoveMethod != null && evt.RemoveMethod.IsStatic);
            }
            else
                throw new NotSupportedException(string.Format("Cannot handle '{0}'.", member.GetType()));
        }

        public static bool IsPublic(this MemberInfo member)
        {
            if (member is MethodBase)
                return ((MethodBase)member).IsPublic;
            else if (member is PropertyInfo)
            {
                PropertyInfo prop = (PropertyInfo)member;
                return (prop.GetMethod != null && prop.GetMethod.IsPublic) || (prop.SetMethod != null && prop.SetMethod.IsPublic);
            }
            else if (member is FieldInfo)
                return ((FieldInfo)member).IsPublic;
            else if (member is EventInfo)
            {
                EventInfo evt = (EventInfo)member;
                return (evt.AddMethod != null && evt.AddMethod.IsPublic) || (evt.RemoveMethod != null && evt.RemoveMethod.IsPublic);
            }
            else
                throw new NotSupportedException(string.Format("Cannot handle '{0}'.", member.GetType()));
        }

        public static int MetadataToken(this MemberInfo member)
        {
            // @mbrit - 2012-06-01 - no idea what to do with this...
            return member.GetHashCode();
        }
        public static MethodInfo[] GetMethods(this Type type, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public)
        {
            return GetMembers<MethodInfo>(type, flags).ToArray();
        }
#endif
    }
}
