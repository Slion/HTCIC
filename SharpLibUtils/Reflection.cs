﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SharpLib.Utils
{
    public class Reflection
    {

        /// <summary>
        /// Returned array can contain null entries.
        /// We had to add that try after moving to Squirrel since GetTypes can throw an exception.
        /// That was only an issue in the deployed application. It worked just fine without in debug.
        /// </summary>
        /// <param name="aAssembly"></param>
        /// <returns></returns>
        public static Type[] TryGetTypes(Assembly aAssembly)
        {
            // See the docs for Assembly.GetTypes
            try
            {
                return aAssembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types;
            }            
        }

        /// <summary>
        /// Get a list of all the concrete types derived from the given type in all loaded assembly.
        /// That includes the given type itself if it's intanciable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<Type> GetConcreteClassesDerivedFrom<T>() where T : class
        {
            List<Type> objects = new List<Type>();

            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {                
                foreach (Type type in TryGetTypes(asm)
                        .Where(myType => myType != null && myType.IsClass && !myType.IsAbstract && (myType.IsSubclassOf(typeof(T)) || myType == typeof(T))))
                {
                    objects.Add(type);
                }
            }

            return objects;
        }

        /// <summary>
        /// Get a dictionary of all the concrete types derived from the given type in all loaded assembly.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IDictionary<string, Type> GetConcreteClassesDerivedFromByName<T>() where T : class
        {
            Dictionary<string, Type> objects = new Dictionary<string, Type>();
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in TryGetTypes(asm)
                .Where(myType => myType != null && myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
                {
                    objects.Add(type.Name, type);
                }
            }
            return objects;
        }

        /// <summary>
        /// Get a list of an instance of all the types derived from the given type in all loaded assembly.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetConcreteClassesInstanceDerivedFrom<T>() where T : class
        {
            List<T> objects = new List<T>();
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in TryGetTypes(asm)
                .Where(myType => myType != null && myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
                {
                    objects.Add((T)Activator.CreateInstance(type));
                }
            }

            if (objects.Count>0
                && objects[0] is IComparable)
            {
                objects.Sort();
            }

            return objects;
        }

        /// <summary>
        /// Get a dictionary of an instance of all the types derived from the given type in all loaded assembly.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IDictionary<string, T> GetConcreteClassesInstanceDerivedFromByName<T>() where T : class
        {
            Dictionary<string, T> objects = new Dictionary<string, T>();
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in TryGetTypes(asm)
                .Where(myType => myType != null && myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
                {
                    objects.Add(type.Name, (T)Activator.CreateInstance(type));
                }
            }


            return objects;
        }



        /// <summary>
        /// Get the attribute instance matching the given attribute type from the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="aType"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(Type aType) where T : class
        {
            object[] attrs = aType.GetCustomAttributes(true);
            foreach (object attr in attrs)
            {
                T attribute = attr as T;
                if (attribute != null)
                {
                    return attribute;
                }
            }

            return null;
        }

        /// <summary>
        /// Get a list of all the types derived from the given type in all loaded assembly.
        /// </summary>
        /// <param name="baseType"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetDerivedTypes<T>() where T: class
        {
            List<Type> types = new List<Type>();
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in TryGetTypes(asm)
                .Where(myType => myType != null && myType.IsClass && myType.IsSubclassOf(typeof(T))))
                {
                    types.Add(type);
                }
            }
            return types;
        }
    }
}
