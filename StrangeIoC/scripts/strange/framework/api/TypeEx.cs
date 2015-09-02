﻿using System;
using System.Collections.Generic;
using System.Reflection;
using strange.extensions.reflector.api;
using strange.framework.api;
using strange.framework.impl;
using System.Collections;
using System.Linq;
using System.Reflection.Emit;

#if NETFX_CORE
namespace System.Reflection
{
    public static class TypeEx
    {
        public static Type[] EmptyTypes = { };

        public static Assembly Assembly(this Type type)
        {
            return type.GetTypeInfo().Assembly;
        }

        public static Assembly GetExecutingAssembly(this Type type)
        {
            return type.GetTypeInfo().Assembly;
        }

        public static bool IsGenericType(this Type type)
        {
            return type.GetTypeInfo().IsGenericType;
        }

        public static bool IsGenericTypeDefinition(this Type type)
        {
            return type.GetTypeInfo().IsGenericTypeDefinition;
        }

        public static bool IsAssignableFrom(this Type type, Type toCheck)
        {
            return type.GetTypeInfo().IsAssignableFrom(toCheck.GetTypeInfo());
        }

        public static bool IsInstanceOfType(this Type type, object toCheck)
        {
            throw new NotImplementedException("This operation has not been implemented.");
        }

        public static bool IsSubclassOf(this Type type, Type toCheck)
        {
            return type.GetTypeInfo().IsSubclassOf(toCheck);
        }

        public static MethodInfo GetMethod(this Type type, string name, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
        {
            return GetMember<MethodInfo>(type, name, flags);
        }

        public static MethodInfo GetMethod(this Type type, string name, Type[] parameters)
        {
            return type.GetMethod(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static, null, parameters, null);
        }

        public static MethodInfo GetMethod(this Type type, string name, BindingFlags flags, object binder, Type[] parameters, object[] modifiers)
        {
            return type.GetMethod(name, flags, null, CallingConventions.Any, parameters, modifiers);
        }

        public static MethodInfo GetMethod(this Type type, string name, BindingFlags flags, object binder, CallingConventions callConvention, Type[] parameters,
            object[] modifiers)
        {
            foreach (var method in type.GetMethods(flags))
            {
                if (method.Name == name && CheckParameters(method, parameters))
                    return method;
            }

            return null;
        }

        public static bool IsInterface(this Type type)
        {
            return type.GetTypeInfo().IsInterface;
        }

        public static bool IsClass(this Type type)
        {
            return type.GetTypeInfo().IsClass;
        }

        public static bool IsPublic(this Type type)
        {
            return type.GetTypeInfo().IsPublic;
        }

        public static bool IsArray(this Type type)
        {
            return type.GetTypeInfo().IsArray;
        }

        public static bool IsEnum(this Type type)
        {
            return type.GetTypeInfo().IsEnum;
        }

        public static bool IsNestedPublic(this Type type)
        {
            return type.GetTypeInfo().IsNestedPublic;
        }

        public static bool IsNestedAssembly(this Type type)
        {
            return type.GetTypeInfo().IsNestedAssembly;
        }

        public static bool IsNestedFamORAssem(this Type type)
        {
            return type.GetTypeInfo().IsNestedFamORAssem;
        }

        public static bool IsVisible(this Type type)
        {
            return type.GetTypeInfo().IsVisible;
        }

        public static bool IsAbstract(this Type type)
        {
            return type.GetTypeInfo().IsAbstract;
        }

        public static bool IsSealed(this Type type)
        {
            return type.GetTypeInfo().IsSealed;
        }

        public static bool IsPrimitive(this Type type)
        {
            return type.GetTypeInfo().IsPrimitive;
        }

        public static bool IsValueType(this Type type)
        {
            return type.GetTypeInfo().IsValueType;
        }

        public static Module Module(this Type type)
        {
            return type.GetTypeInfo().Module;
        }

        public static MethodBase DeclaringMethod(this Type type)
        {
            return type.GetTypeInfo().DeclaringMethod;
        }

        public static Type UnderlyingSystemType(this Type type)
        {
            // @mbrit - 2012-05-30 - this needs more science... UnderlyingSystemType isn't supported
            // in WinRT, but unclear why this was used...
            return type;
        }

        public static bool ContainsGenericParameters(this Type type)
        {
            return type.GetTypeInfo().ContainsGenericParameters;
        }

        public static GenericParameterAttributes GenericParameterAttributes(this Type type)
        {
            return type.GetTypeInfo().GenericParameterAttributes;
        }

        public static Type BaseType(this Type type)
        {
            return type.GetTypeInfo().BaseType;
        }

        public static Type[] GetInterfaces(this Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces.ToArray();
        }

        public static InterfaceMapping GetInterface(this Type type, Type interfaceType)
        {
            throw new NotImplementedException("This operation has not been implemented.");
        }

        public static object[] GetCustomAttributes(this Type type, bool inherit = false)
        {
            return type.GetTypeInfo().GetCustomAttributes(inherit).ToArray();
        }

        public static object[] GetCustomAttributes(this Type type, Type attributeType, bool inherit = false)
        {
            return type.GetTypeInfo().GetCustomAttributes(attributeType, inherit).ToArray();
        }

        public static ConstructorInfo[] GetConstructors(this Type type, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance)
        {
            return GetMembers<ConstructorInfo>(type, flags).ToArray();
        }

        public static PropertyInfo[] GetProperties(this Type type, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public)
        {
            return GetMembers<PropertyInfo>(type, flags).ToArray();
        }

        public static MethodInfo[] GetMethods(this Type type, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public)
        {
            return GetMembers<MethodInfo>(type, flags).ToArray();
        }

        public static FieldInfo[] GetFields(this Type type, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public)
        {
            return GetMembers<FieldInfo>(type, flags).ToArray();
        }

        public static EventInfo[] GetEvents(this Type type, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public)
        {
            return GetMembers<EventInfo>(type, flags).ToArray();
        }


        private static List<T> GetMembers<T>(Type type, BindingFlags flags)
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


        public static MemberInfo[]  FindMembers(this Type type, MemberTypes memberType, BindingFlags bindingAttr)
        {
             // Define the work arrays
            MethodInfo[] m = null;
            ConstructorInfo[] c = null;
            FieldInfo[] f = null;
            PropertyInfo[] p = null;
            EventInfo[] e = null;
            Type[] t = null;
            
            int i = 0;
            int cnt = 0;            // Total Matchs
            
            // Check the methods
            if ((memberType & MemberTypes.Method) != 0) {
                m = GetMethods(type, bindingAttr);
                cnt+=m.Length;
            }
            
            // Check the constructors
            if ((memberType & MemberTypes.Constructor) != 0) {
                c = GetConstructors(type, bindingAttr);
                cnt+=c.Length;
                System.Diagnostics.Debug.WriteLine("GetConstructors:-------------------------- "+c.ToString());
                
            }
            
            // Check the fields
            if ((memberType & MemberTypes.Field) != 0) {
                f = GetFields(type, bindingAttr);
                cnt+=f.Length;
                System.Diagnostics.Debug.WriteLine("GetFields:-------------------------- "+f.ToString());
                
            }
            
            // Check the Properties
            if ((memberType & MemberTypes.Property) != 0) {
                p = GetProperties(type, bindingAttr);
                cnt+=p.Length;
                System.Diagnostics.Debug.WriteLine("GetProperties:-------------------------- "+p.ToString());
                
            }
            
            // Check the Events
            if ((memberType & MemberTypes.Event) != 0) {
                e = GetEvents(type, bindingAttr);
                cnt+=e.Length;
                System.Diagnostics.Debug.WriteLine("GetEvents:-------------------------- "+e.ToString());
                
            }
            
            // Check the Types
            if ((memberType & MemberTypes.NestedType) != 0) {
                t = GetNestedTypes(type, bindingAttr);
                cnt+=t.Length;
                System.Diagnostics.Debug.WriteLine("GetNestedTypes:-------------------------- "+t.ToString());
                
                
            }
            
            // Allocate the Member Info
            MemberInfo[] ret = new MemberInfo[cnt];
            
            // Copy the Methods
            cnt = 0;
            if (m != null) {
                for (i=0;i<m.Length;i++)
                    if (m[i] != null)
                        ret[cnt++] = m[i];
            }
            
            // Copy the Constructors
            if (c != null) {
                for (i=0;i<c.Length;i++)
                    if (c[i] != null)
                        ret[cnt++] = c[i];
            }
            
            // Copy the Fields
            if (f != null) {
                for (i=0;i<f.Length;i++)
                    if (f[i] != null)
                        ret[cnt++] = f[i];
            }
            
            // Copy the Properties
            if (p != null) {
                System.Diagnostics.Debug.WriteLine("p.Length:-------------------------- "+p.Length);
                for (i=0;i<p.Length;i++)
                    if (p[i] != null)
                        ret[cnt++] = p[i];
            }
            
            // Copy the Events
            if (e != null) {
                for (i=0;i<e.Length;i++)
                    if (e[i] != null)
                        ret[cnt++] = e[i];
            }
            
            // Copy the Types
            if (t != null) {
                for (i=0;i<t.Length;i++)
                    if (t[i] != null)
                        ret[cnt++] = t[i].GetTypeInfo();
            }
            
            return ret;
        }

        public static Type[] GetNestedTypes(this Type type, BindingFlags flags) {
            // todo - flags are ignored

            Type[] t = null;
            int i = 0;
            foreach(TypeInfo ti in type.GetTypeInfo().DeclaredNestedTypes) {
                t[i] = ti.AsType();
                System.Diagnostics.Debug.WriteLine("TypeInfo ti:-------------------------- "+t.ToString());
                i++;
            }
            return t;
        }

        public static MemberTypes GetMemberType(this MemberInfo member)
        {
            if (member is FieldInfo)
                return MemberTypes.Field;
            if (member is ConstructorInfo)
                return MemberTypes.Constructor;
            if (member is PropertyInfo)
                return MemberTypes.Property;
            if (member is EventInfo)
                return MemberTypes.Event;
            if (member is MethodInfo)
                return MemberTypes.Method;

            var typeInfo = member as TypeInfo;
            
            if (!typeInfo.IsPublic && !typeInfo.IsNotPublic)
                return MemberTypes.NestedType;

            return MemberTypes.TypeInfo;
        } 

        public static ConstructorInfo GetConstructor(this Type type, Type[] types)
        {
            return type.GetConstructor(BindingFlags.Public, null, types, null);
        }

        public static ConstructorInfo GetConstructor(this Type type, BindingFlags flags, object binder, Type[] types, object[] modifiers)
        {
            // can't have static constructors...
            flags |= BindingFlags.Instance | BindingFlags.Static;
            flags ^= BindingFlags.Static;

            // walk...
            foreach (ConstructorInfo info in type.GetTypeInfo().DeclaredConstructors)
            {
                if (info.CheckBindings(flags, false) && CheckParameters(info, types))
                    return info;
            }

            return null;
        }

        private static bool CheckParameters(MethodBase method, Type[] parameters)
        {
            var methodParameters = method.GetParameters();
            if (methodParameters.Length == parameters.Length)
            {
                if (parameters.Length == 0)
                    return true;
                else
                {
                    for (int index = 0; index < parameters.Length; index++)
                    {
                        if (parameters[index] != methodParameters[index].ParameterType)
                            return false;
                    }

                    // ok...
                    return true;
                }
            }

            // nope...
            return false;
        }

        public static PropertyInfo GetProperty(this Type type, string name, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
        {
            return GetMember<PropertyInfo>(type, name, flags);
        }

        public static EventInfo GetEvent(this Type type, string name, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
        {
            return GetMember<EventInfo>(type, name, flags);
        }

        public static FieldInfo GetField(this Type type, string name, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)
        {
            return GetMember<FieldInfo>(type, name, flags);
        }

        private static T GetMember<T>(Type type, string name, BindingFlags flags)
            where T : MemberInfo
        {
            // walk...
            foreach (var member in GetMembers<T>(type, flags))
            {
                if (member.Name == name)
                    return (T)member;
            }

            return null;
        }

        public static Type GetInterface(this Type type, string name, bool ignoreCase = false)
        {
            // walk up the hierarchy...
            var info = type.GetTypeInfo();
            while (true)
            {
                foreach (var iface in type.GetInterfaces())
                {
                    if (ignoreCase)
                    {
                        // this matches just the name...
                        if (string.Compare(iface.Name, name, StringComparison.CurrentCultureIgnoreCase) == 0)
                            return iface;
                    }
                    else
                    {
                        if (iface.FullName == name || iface.Name == name)
                            return iface;
                    }
                }

                // up...
                if (info.BaseType == null)
                    break;
                info = info.BaseType.GetTypeInfo();
            }

            return null;
        }

        public static Type[] GetGenericArguments(this Type type)
        {
            return type.GetTypeInfo().GenericTypeArguments;
        }

        public static Type[] GetGenericParameterConstraints(this Type type)
        {
            return type.GetTypeInfo().GetGenericParameterConstraints();
        }

        public static object GetCustomAttribute<T>(this Type type, bool inherit = false)
            where T : Attribute
        {
            return type.GetTypeInfo().GetCustomAttribute<T>(inherit);
        }

        public static InterfaceMapping GetInterfaceMap(this Type type, Type interfaceType)
        {
            return type.GetTypeInfo().GetRuntimeInterfaceMap(interfaceType);
        }

        public static TypeCode GetTypeCode(this Type type)
        {
            if (type == null)
                return TypeCode.Empty;

            if (typeof(bool).IsAssignableFrom(type))
                return TypeCode.Boolean;
            else if (typeof(char).IsAssignableFrom(type))
                return TypeCode.Char;
            else if (typeof(sbyte).IsAssignableFrom(type))
                return TypeCode.SByte;
            else if (typeof(byte).IsAssignableFrom(type))
                return TypeCode.Byte;
            else if (typeof(short).IsAssignableFrom(type))
                return TypeCode.Int16;
            else if (typeof(ushort).IsAssignableFrom(type))
                return TypeCode.UInt16;
            else if (typeof(int).IsAssignableFrom(type))
                return TypeCode.Int32;
            else if (typeof(uint).IsAssignableFrom(type))
                return TypeCode.UInt32;
            else if (typeof(long).IsAssignableFrom(type))
                return TypeCode.Int64;
            else if (typeof(ulong).IsAssignableFrom(type))
                return TypeCode.UInt64;
            else if (typeof(float).IsAssignableFrom(type))
                return TypeCode.Single;
            else if (typeof(double).IsAssignableFrom(type))
                return TypeCode.Double;
            else if (typeof(decimal).IsAssignableFrom(type))
                return TypeCode.Decimal;
            else if (typeof(DateTime).IsAssignableFrom(type))
                return TypeCode.DateTime;
            else if (typeof(string).IsAssignableFrom(type))
                return TypeCode.String;
            else
                return TypeCode.Object;
        }

        public static Type[] FindInterfaces(this Type type, Func<Type, object, bool> filter, object criteria)
        {
            List<Type> results = new List<Type>();
            foreach (Type walk in type.GetInterfaces())
            {
                if (filter(type, criteria))
                    results.Add(walk);
            }

            return results.ToArray();
        }

        public static int MetadataToken(this Type type)
        {
            // @mbrit - 2012-06-01 - no idea what to do with this...
            return type.GetHashCode();
        }
    }

    public static class PropertyInfoExtender
    {
        public static MethodInfo GetGetMethod(this PropertyInfo prop, bool nonPublic = false)
        {
            // @mbrit - 2012-05-30 - non-public not supported in winrt...
            if (prop.GetMethod != null && (prop.GetMethod.IsPublic || nonPublic))
                return prop.GetMethod;
            else
                return null;
        }

        public static MethodInfo GetSetMethod(this PropertyInfo prop, bool nonPublic = false)
        {
            // @mbrit - 2012-05-30 - non-public not supported in winrt...
            if (prop.SetMethod != null && (prop.SetMethod.IsPublic || nonPublic))
                return prop.SetMethod;
            else
                return null;
        }

        public static Type ReflectedType(this PropertyInfo prop)
        {
            // this isn't right...
            return prop.DeclaringType;
        }
    }

    public static class ParameterInfoExtender
    {
        public static bool HasAttribute<T>(this ParameterInfo info)
            where T : Attribute
        {
            throw new NotImplementedException("This operation has not been implemented.");
        }
    }

    public static class MemberInfoExtender
    {
        public static MemberTypes MemberType(this MemberInfo member)
        {
            if (member is MethodInfo)
                return ((MethodInfo)member).MemberType();
            else
                throw new NotSupportedException(string.Format("Cannot handle '{0}'.", member.GetType()));
        }

        public static Type ReflectedType(this MemberInfo member)
        {
            // this isn't right...
            if (member is MethodInfo)
                return ((MethodInfo)member).ReflectedType();
            else
                throw new NotSupportedException(string.Format("Cannot handle '{0}'.", member.GetType()));
        }

        public static bool HasAttribute<T>(this MemberInfo member, bool inherit = false)
            where T : Attribute
        {
            return member.HasAttribute(typeof(T), inherit);
        }

        public static bool HasAttribute(this MemberInfo member, Type type, bool inherit = false)
        {
            throw new NotImplementedException("This operation has not been implemented.");
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
    }

    public static class MethodInfoExtender
    {
        public static MemberTypes MemberType(this MethodInfo method)
        {
            return MemberTypes.Method;
        }

        public static Type ReflectedType(this MethodInfo method)
        {
            // this isn't right...
            return method.DeclaringType;
        }

        public static MethodInfo GetBaseDefinition(this MethodInfo method)
        {
            var flags = BindingFlags.Instance;
            if (method.IsPublic)
                flags |= BindingFlags.Public;
            else
                flags |= BindingFlags.NonPublic;

            List<Type> parameters = new List<Type>();
            foreach (var parameter in method.GetParameters())
                parameters.Add(parameter.ParameterType);

            // get...
            var info = method.DeclaringType.GetTypeInfo();
            var found = new List<MethodInfo>();
            while (true)
            {
                // find...
                MethodInfo inParent = info.AsType().GetMethod(method.Name, flags, null, parameters.ToArray(), null);
                if (inParent != null)
                    found.Add(inParent);

                // up...
                if (info.BaseType == null)
                    break;
                info = info.BaseType.GetTypeInfo();
            }

            // return the last one...
            return found.Last();
        }

        public static bool IsAbstract(this MethodBase method)
        {
            return method.IsAbstract;
        }
    }

    public static class EventInfoExtender
    {
        public static MethodInfo GetAddMethod(this EventInfo e, bool nonPublic = false)
        {
            if (e.AddMethod != null && (e.AddMethod.IsPublic || nonPublic))
                return e.AddMethod;
            else
                return null;
        }

        public static MethodInfo GetRemoveMethod(this EventInfo e, bool nonPublic = false)
        {
            if (e.RemoveMethod != null && (e.RemoveMethod.IsPublic || nonPublic))
                return e.RemoveMethod;
            else
                return null;
        }
    }

    public static class ParameterInfoExtension
    {
        public static bool HasAttribute(this ParameterInfo param, Type type)
        {
            throw new NotImplementedException("This operation has not been implemented.");
        }
    }

    public static class AssemblyExtender
    {
        public static object[] GetAttributes<T>()
            where T : Attribute
        {
            throw new NotImplementedException("This operation has not been implemented.");
        }

        public static Type[] GetTypes(this Assembly asm)
        {
            var results = new List<Type>();
            foreach (var type in asm.DefinedTypes)
                results.Add(type.AsType());

            return results.ToArray();
        }
    }

    [Flags]
    public enum BindingFlags
    {
        Default = 0,
        IgnoreCase = 1,
        DeclaredOnly = 2,
        Instance = 4,
        Static = 8,
        Public = 16,
        NonPublic = 32,
        FlattenHierarchy = 64,
        InvokeMethod = 256,
        CreateInstance = 512,
        GetField = 1024,
        SetField = 2048,
        GetProperty = 4096,
        SetProperty = 8192,
        PutDispProperty = 16384,
        PutRefDispProperty = 32768,
        ExactBinding = 65536,
        SuppressChangeType = 131072,
        OptionalParamBinding = 262144,
        IgnoreReturn = 16777216,
    }

    [Flags]
    public enum MemberTypes
    {
        Constructor = 1,
        Event = 2,
        Field = 4,
        Method = 8,
        Property = 16,
        TypeInfo = 32,
        Custom = 64,
        NestedType = 128,
        All = 191,
    }
}

namespace System
{
    public enum TypeCode
    {
        Empty = 0,
        Object = 1,
        DBNull = 2,
        Boolean = 3,
        Char = 4,
        SByte = 5,
        Byte = 6,
        Int16 = 7,
        UInt16 = 8,
        Int32 = 9,
        UInt32 = 10,
        Int64 = 11,
        UInt64 = 12,
        Single = 13,
        Double = 14,
        Decimal = 15,
        DateTime = 16,
        String = 18,
    }
}
#endif