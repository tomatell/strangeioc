/*
 * Copyright 2013 ThirdMotion, Inc.
 *
 *	Licensed under the Apache License, Version 2.0 (the "License");
 *	you may not use this file except in compliance with the License.
 *	You may obtain a copy of the License at
 *
 *		http://www.apache.org/licenses/LICENSE-2.0
 *
 *		Unless required by applicable law or agreed to in writing, software
 *		distributed under the License is distributed on an "AS IS" BASIS,
 *		WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *		See the License for the specific language governing permissions and
 *		limitations under the License.
 */

/**
 * @class strange.extensions.reflector.impl.ReflectionBinder
 * 
 * Uses System.Reflection to create `ReflectedClass` instances.
 * 
 * Reflection is a slow process. This binder isolates the calls to System.Reflector 
 * and caches the result, meaning that Reflection is performed only once per class.
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using strange.extensions.reflector.api;
using strange.framework.api;
using strange.framework.impl;
using System.Collections;
using System.Linq;

namespace strange.extensions.reflector.impl
{
	public class ReflectionBinder : strange.framework.impl.Binder, IReflectionBinder
	{
		public ReflectionBinder ()
		{
		}

		public IReflectedClass Get<T> ()
		{
			return Get (typeof(T));
		}

		public IReflectedClass Get (Type type)
		{
			IBinding binding = GetBinding(type);
			IReflectedClass retv;
			if (binding == null)
			{
				binding = GetRawBinding ();
				IReflectedClass reflected = new ReflectedClass ();
				mapPreferredConstructor (reflected, binding, type);
				mapPostConstructors (reflected, binding, type);
				mapSetters (reflected, binding, type);
				binding.Bind (type).To (reflected);
				retv = binding.value as IReflectedClass;
				retv.PreGenerated = false;
			}
			else
			{
				retv = binding.value as IReflectedClass;
				retv.PreGenerated = true;
			}
			return retv;
		}

		public override IBinding GetRawBinding ()
		{
			IBinding binding = base.GetRawBinding ();
			binding.valueConstraint = BindingConstraintType.ONE;
			return binding;
		}

		private void mapPreferredConstructor(IReflectedClass reflected, IBinding binding, Type type)
		{
			ConstructorInfo constructor = findPreferredConstructor (type);
			if (constructor == null)
			{
				throw new ReflectionException("The reflector requires concrete classes.\nType " + type + " has no constructor. Is it an interface?", ReflectionExceptionType.CANNOT_REFLECT_INTERFACE);
			}
			ParameterInfo[] parameters = constructor.GetParameters();


			Type[] paramList = new Type[parameters.Length];
			object[] names = new object[parameters.Length];
			int i = 0;
			foreach (ParameterInfo param in parameters)
			{
				Type paramType = param.ParameterType;
				paramList [i] = paramType;
#if NETFX_CORE
				IEnumerable attributes = param.GetCustomAttributes(typeof(Name), false);
                foreach( var name in attributes ) {
                    names[i] = (Name)name;
                }
                i++;

#else
                object[] attributes = param.GetCustomAttributes(typeof(Name), false);

                if (attributes.Length > 0)
                {
                    names[i] = ((Name)attributes[0]).name;
                }
                i++;
#endif
			}
			reflected.Constructor = constructor;
			reflected.ConstructorParameters = paramList;
			reflected.ConstructorParameterNames = names;
		}

		//Look for a constructor in the order:
		//1. Only one (just return it, since it's our only option)
		//2. Tagged with [Construct] tag
		//3. The constructor with the fewest parameters
		private ConstructorInfo findPreferredConstructor(Type type)
		{
#if NETFX_CORE
            IEnumerable constructorsquery = type.GetTypeInfo().DeclaredConstructors;
            ConstructorInfo[] constructors = constructorsquery.Cast<ConstructorInfo>().ToArray();

#else
            ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.FlattenHierarchy |
                                                                        BindingFlags.Public |
                                                                        BindingFlags.Instance |
                                                                        BindingFlags.InvokeMethod);

#endif
			if (constructors.Length == 1)
			{
				return constructors [0];
			}
			int len;
			int shortestLen = int.MaxValue;
			ConstructorInfo shortestConstructor = null;
			foreach (ConstructorInfo constructor in constructors)
			{
#if NETFX_CORE
                IEnumerable taggedConstructorsquery = constructor.GetCustomAttributes(typeof(Construct), true);

                IEnumerable<Construct> ii = taggedConstructorsquery.OfType<Construct>();
                object[] taggedConstructors = ConvertListToObject<Construct>(ii);
#else
                object[] taggedConstructors = constructor.GetCustomAttributes(typeof(Construct), true);

#endif
				if (taggedConstructors.Length > 0)
				{
					return constructor;
				}
				len = constructor.GetParameters ().Length;
				if (len < shortestLen)
				{
					shortestLen = len;
					shortestConstructor = constructor;
				}
			}
			return shortestConstructor;
		}

		private void mapPostConstructors(IReflectedClass reflected, IBinding binding, Type type)
		{
#if NETFX_CORE
            IEnumerable methods = type.GetTypeInfo().DeclaredMethods;


#else
            MethodInfo[] methods = type.GetMethods(BindingFlags.FlattenHierarchy |
                                                         BindingFlags.Public |
                                                         BindingFlags.Instance |
                                                         BindingFlags.InvokeMethod);
#endif
			ArrayList methodList = new ArrayList ();
			foreach (MethodInfo method in methods)
			{
#if NETFX_CORE

                IEnumerable taggedquery = method.GetCustomAttributes(typeof(PostConstruct), true);
                IEnumerable<PostConstruct> ii = taggedquery.OfType<PostConstruct>();
                object[] tagged = ConvertListToObject<PostConstruct>(ii);
#else
                object[] tagged = method.GetCustomAttributes(typeof(PostConstruct), true);

#endif
				if (tagged.Length > 0)
				{
					methodList.Add (method);
				}
			}

			methodList.Sort (new PriorityComparer ());
			MethodInfo[] postConstructors = (MethodInfo[])methodList.ToArray (typeof(MethodInfo));
			reflected.postConstructors = postConstructors;
		}

#if NETFX_CORE
        bool HasPublicGetter(PropertyInfo pi)
        {
            if (!pi.CanRead)
                return false;
            MethodInfo getter = pi.GetMethod;
            return getter.IsPublic;
        }
#endif

        private void mapSetters(IReflectedClass reflected, IBinding binding, Type type)
		{
			KeyValuePair<Type, PropertyInfo>[] pairs = new KeyValuePair<Type, PropertyInfo>[0];
			object[] names = new object[0];

#if NETFX_CORE
            IEnumerable privateMembers = type.GetTypeInfo().DeclaredMembers;
#else
            MemberInfo[] privateMembers = type.FindMembers(MemberTypes.Property,
                                                    BindingFlags.FlattenHierarchy |
                                                    BindingFlags.SetProperty |
                                                    BindingFlags.NonPublic |
                                                    BindingFlags.Instance,
                                                    null, null);
#endif
			foreach (MemberInfo member in privateMembers)
			{
#if NETFX_CORE
                IEnumerable injectionsquery = member.GetCustomAttributes(typeof(Inject), true);
                IEnumerable<Inject> ii = injectionsquery.OfType<Inject>();
                object[] injections = ConvertListToObject<Inject>(ii);
#else
                object[] injections = member.GetCustomAttributes(typeof(Inject), true);

#endif
				if (injections.Length > 0)
				{
					throw new ReflectionException ("The class " + type.Name + " has a non-public Injection setter " + member.Name + ". Make the setter public to allow injection.", ReflectionExceptionType.CANNOT_INJECT_INTO_NONPUBLIC_SETTER);
				}
			}

#if NETFX_CORE
            IEnumerable members = type.GetTypeInfo().DeclaredMembers;

#else
            MemberInfo[] members = type.FindMembers(MemberTypes.Property,
                                                          BindingFlags.FlattenHierarchy |
                                                          BindingFlags.SetProperty |
                                                          BindingFlags.Public |
                                                          BindingFlags.Instance,
                                                          null, null);
#endif

			foreach (MemberInfo member in members)
			{
#if NETFX_CORE
                IEnumerable injectionsquery = member.GetCustomAttributes(typeof(Inject), true);
                IEnumerable<Inject> ii = injectionsquery.OfType<Inject>();
                object[] injections = ConvertListToObject<Inject>(ii);
#else
                object[] injections = member.GetCustomAttributes(typeof(Inject), true);

#endif
				if (injections.Length > 0)
				{
					Inject attr = injections [0] as Inject;
					PropertyInfo point = member as PropertyInfo;
					Type pointType = point.PropertyType;
					KeyValuePair<Type, PropertyInfo> pair = new KeyValuePair<Type, PropertyInfo> (pointType, point);
					pairs = AddKV (pair, pairs);

					object bindingName = attr.name;
					names = Add (bindingName, names);
				}
			}
			reflected.Setters = pairs;
			reflected.SetterNames = names;
		}

		/**
		 * Add an item to a list
		 */
		private object[] Add(object value, object[] list)
		{
			object[] tempList = list;
			int len = tempList.Length;
			list = new object[len + 1];
			tempList.CopyTo (list, 0);
			list [len] = value;
			return list;
		}

		/**
		 * Add an item to a list
		 */
		private  KeyValuePair<Type,PropertyInfo>[] AddKV(KeyValuePair<Type,PropertyInfo> value, KeyValuePair<Type,PropertyInfo>[] list)
		{
			KeyValuePair<Type,PropertyInfo>[] tempList = list;
			int len = tempList.Length;
			list = new KeyValuePair<Type,PropertyInfo>[len + 1];
			tempList.CopyTo (list, 0);
			list [len] = value;
			return list;
		}

        public object[] ToArray(IEnumerable<object> myEnumerable)
        {
            return new Buffer<object>(myEnumerable).ToArray();
        }

        public MemberInfo[] ToMemberInfoArray(IEnumerable<MemberInfo> myEnumerable)
        {
            return new Buffer<MemberInfo>(myEnumerable).ToArray();
        }

        public MethodInfo[] ToMethodInfoArray(IEnumerable<MethodInfo> myEnumerable)
        {
            return new Buffer<MethodInfo>(myEnumerable).ToArray();
        }

        public ConstructorInfo[] ToConstructorInfoArray(IEnumerable<ConstructorInfo> myEnumerable)
        {
            return new Buffer<ConstructorInfo>(myEnumerable).ToArray();
        }

        public object[] ConvertListToObject<T>(IEnumerable<T> DataSource)
        {
            int rows = DataSource.Count();

            //Create object array with rows/cols
            object[] excelarray = new object[rows];
            int i = 0;
            foreach (T data in DataSource) //Outer loop
            {
                //for (int j = 0; j < cols; j++) //Inner loop
                //{
                //    excelarray[i, j] = propertyInfos[j].GetValue(data, null);
                //}
                excelarray[i] = data;
                i++;
            }
            return excelarray;
        }
	}
	}

	class PriorityComparer : IComparer
	{
        public object[] ConvertListToObject<T>(IEnumerable<T> DataSource)
        {
            int rows = DataSource.Count();

            //Create object array with rows/cols
            object[] excelarray = new object[rows];
            int i = 0;
            foreach (T data in DataSource) //Outer loop
            {
                //for (int j = 0; j < cols; j++) //Inner loop
                //{
                //    excelarray[i, j] = propertyInfos[j].GetValue(data, null);
                //}
                excelarray[i] = data;
                i++;
            }
            return excelarray;
        }
		int IComparer.Compare( Object x, Object y )
		{

			int pX = getPriority (x as MethodInfo);
			int pY = getPriority (y as MethodInfo);

			return (pX < pY) ? -1 : 1;
		}

		private int getPriority(MethodInfo methodInfo)
		{
#if NETFX_CORE
            IEnumerable attrquery = methodInfo.GetCustomAttributes(typeof(PostConstruct), true);
            IEnumerable<PostConstruct> pi = attrquery.OfType<PostConstruct>();
            object[] pinjections = ConvertListToObject<PostConstruct>(pi);
            PostConstruct attr = pinjections[0] as PostConstruct;

#else
            PostConstruct attr = methodInfo.GetCustomAttributes(true)[0] as PostConstruct;
#endif
			int priority = attr.priority;
			return priority;
		}

	}


struct Buffer<TElement>
{
    internal TElement[] items;
    internal int count;
    internal Buffer(IEnumerable<TElement> source)
    {
        TElement[] array = null;
        int num = 0;
        ICollection<TElement> collection = source as ICollection<TElement>;
        if (collection != null)
        {
            num = collection.Count;
            if (num > 0)
            {
                array = new TElement[num];
                collection.CopyTo(array, 0);
            }
        }
        else
        {
            foreach (TElement current in source)
            {
                if (array == null)
                {
                    array = new TElement[4];
                }
                else
                {
                    if (array.Length == num)
                    {
                        TElement[] array2 = new TElement[checked(num * 2)];
                        Array.Copy(array, 0, array2, 0, num);
                        array = array2;
                    }
                }
                array[num] = current;
                num++;
            }
        }
        this.items = array;
        this.count = num;
    }
    public TElement[] ToArray()
    {
        if (this.count == 0)
        {
            return new TElement[0];
        }
        if (this.items.Length == this.count)
        {
            return this.items;
        }
        TElement[] array = new TElement[this.count];
        Array.Copy(this.items, 0, array, 0, this.count);
        return array;
    }

    
}



