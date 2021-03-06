﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace HelperProject.HelperLibrary
{
    public class Utils
    {

        public static bool IsDirectory(string path)
        {
            FileAttributes attr = File.GetAttributes(path);

            return attr.HasFlag(FileAttributes.Directory);
        }

        public static Type[] GetTypesInNamespace(string assemblyClassName, string nameSpace)
        {
            Assembly assembly = GetAssemblyNameOfClass(assemblyClassName);

            return
              assembly.GetTypes()
                      .Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal))
                      .ToArray();
        }

        public static Assembly GetAssemblyNameOfClass(string className)
        {
            Type objectType = GetTypeByClassName("Plugin");

            return objectType.Assembly;
        }

        public static Type GetTypeByClassName(string className)
        {
            return (from asm in AppDomain.CurrentDomain.GetAssemblies()
                    from type in asm.GetTypes()
                    where type.IsClass && type.Name == className
                    select type).Single();
        }

        public static bool HasInheritedClass(string baseClass, string inheritCheck)
        {
            Type t = Type.GetType(baseClass);
            if (t.BaseType.Name == inheritCheck)
            {
                return true;
            }

            return false;
        }

        public static void CopyDirectoryContent(string sourcePath, string destinationPath)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourcePath);
            DirectoryInfo diTarget = new DirectoryInfo(destinationPath);

            CopyAll(diSource, diTarget);
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            target.Create();

            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            foreach (DirectoryInfo di in source.GetDirectories())
            {
                DirectoryInfo nextTarget = target.CreateSubdirectory(di.Name);
                CopyAll(di, nextTarget);
            }
        }

        public static void DeleteDirWithContent(string folderPath)
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete();
            }
            Directory.Delete(folderPath);
        }
    }
}
