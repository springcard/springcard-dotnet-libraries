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
        private static List<AssemblyName> missingAssemblies = new List<AssemblyName>();

        public static bool VerifyAssemblies(bool missingAssemblyIsFatal)
        {
            if (doneVerifyAssemblies)
                return true;

            bool result = true;
            loadedAssemblies.Clear();
            missingAssemblies.Clear();
            List<string> loadedAssemblyNames = new List<string>();
            Queue<Assembly> assembliesToCheck = new Queue<Assembly>();

            assembliesToCheck.Enqueue(Assembly.GetEntryAssembly());

            while (assembliesToCheck.Count > 0)
            {
                Assembly assemblyToCheck = assembliesToCheck.Dequeue();
                foreach (AssemblyName assemblyName in assemblyToCheck.GetReferencedAssemblies())
                {
                    if (!loadedAssemblyNames.Contains(assemblyName.FullName))
                    {
                        try
                        {
                            Assembly loadedAssembly = Assembly.Load(assemblyName);
                            assembliesToCheck.Enqueue(loadedAssembly);
                            loadedAssemblies.Add(loadedAssembly);
                            loadedAssemblyNames.Add(assemblyName.FullName);
                        }
                        catch (Exception e)
                        {
                            bool acceptMissing = false;
                            bool silentMissing = false;
                            if (assemblyName.FullName.StartsWith("System"))
                                acceptMissing = true;
                            if (assemblyName.FullName.StartsWith("Microsoft"))
                                acceptMissing = true;
                            if (assemblyName.FullName.StartsWith("Windows"))
                                acceptMissing = true;
                            if (!assemblyToCheck.FullName.StartsWith("SpringCard"))
                                silentMissing = true;
                            if ((assemblyName.Version.Major == 255) && (assemblyName.Version.Minor == 255) && (assemblyName.Version.Revision == 255) && (assemblyName.Version.Build == 255))
                                silentMissing = true;

                            if (!acceptMissing)
                            {
                                result = false;
                                Logger.Fatal("Assembly {0} referenced by {1} is missing", assemblyName, assemblyToCheck.FullName);
                                if (!missingAssemblies.Contains(assemblyName))
                                    missingAssemblies.Add(assemblyName);
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
            List<string> result = new List<string>();

            foreach (AssemblyName name in missingAssemblies)
            {
                if (!result.Contains(name.FullName))
                    result.Add(name.FullName);
            }

            return result.ToArray();
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