/**
 *
 * \ingroup LibCs
 *
 * \copyright
 *   Copyright (c) 2008-2018 SpringCard - www.springcard.com
 *   All right reserved
 *
 * \author
 *   Johann.D et al. / SpringCard
 *
 */
/*
 * Read LICENSE.txt for license details and restrictions.
 */
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SpringCard.LibCs
{
    public static class AppCheck
    {
        private static bool doneVerifyAssemblies = false;
        private static List<Assembly> loadedAssemblies = new List<Assembly>();
        private static List<string> missingAssemblyNames = new List<string>();

        public static bool VerifyAssemblies(bool missingAssemblyIsFatal)
        {
            return VerifyAssemblies(true, missingAssemblyIsFatal);
        }

        public static bool VerifyAssemblies(bool withVersions, bool missingAssemblyIsFatal)
        {
            if (doneVerifyAssemblies)
                return true;

            bool result = true;
            loadedAssemblies.Clear();
            missingAssemblyNames.Clear();
            List<string> loadedAssemblyNames = new List<string>();
            Queue<Assembly> assembliesToCheck = new Queue<Assembly>();

            assembliesToCheck.Enqueue(Assembly.GetEntryAssembly());

            while (assembliesToCheck.Count > 0)
            {
                Assembly assemblyToCheck = assembliesToCheck.Dequeue();
                foreach (AssemblyName assemblyEntry in assemblyToCheck.GetReferencedAssemblies())
                {
                    string assemblyName;
                    if (withVersions)
                        assemblyName = assemblyEntry.FullName;
                    else
                        assemblyName = assemblyEntry.Name;

                    if (!loadedAssemblyNames.Contains(assemblyName))
                    {
                        try
                        {
                            Assembly loadedAssembly = Assembly.Load(assemblyEntry);
                            assembliesToCheck.Enqueue(loadedAssembly);
                            loadedAssemblies.Add(loadedAssembly);
                            loadedAssemblyNames.Add(assemblyName);
                        }
                        catch (Exception e)
                        {
                            bool acceptMissing = false;
                            bool silentMissing = false;
                            if (assemblyName.StartsWith("System"))
                                acceptMissing = true;
                            if (assemblyName.StartsWith("Microsoft"))
                                acceptMissing = true;
                            if (assemblyName.StartsWith("Windows"))
                                acceptMissing = true;
                            if (!assemblyToCheck.FullName.StartsWith("SpringCard"))
                                silentMissing = true;
                            if ((assemblyEntry.Version.Major == 255) && (assemblyEntry.Version.Minor == 255) && (assemblyEntry.Version.Revision == 255) && (assemblyEntry.Version.Build == 255))
                                silentMissing = true;

                            if (!acceptMissing)
                            {
                                result = false;
                                Logger.Fatal("Assembly {0} referenced by {1} is missing", assemblyName, assemblyToCheck.FullName);
                                if (!missingAssemblyNames.Contains(assemblyName))
                                    missingAssemblyNames.Add(assemblyName);
                                if (missingAssemblyIsFatal) throw e;
                            }
                            else if (!silentMissing)
                            {
                                Logger.Trace("Assembly {0} referenced by {1} is missing", assemblyName, assemblyToCheck.FullName);
                            }
                        }
                    }
                }
            }

            doneVerifyAssemblies = result;
            return result;
        }

        public static bool VerifyAssemblies()
        {
            return VerifyAssemblies(false);
        }

        public static string[] GetLoadedAssemblyNames()
        {
            List<string> result = new List<string>();

            foreach (Assembly assembly in loadedAssemblies)
            {
                if (!result.Contains(assembly.Location))
                    result.Add(Path.GetFileName(assembly.Location));
            }

            return result.ToArray();
        }

        public static string[] GetMissingAssemblyNames()
        {
            return missingAssemblyNames.ToArray();
        }


        public static Type[] EnumClasses()
        {
            List<Type> result = new List<Type>();

            VerifyAssemblies(false);
            foreach (Assembly assembly in loadedAssemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    result.Add(type);
                }
            }

            return result.ToArray();
        }


    }
}