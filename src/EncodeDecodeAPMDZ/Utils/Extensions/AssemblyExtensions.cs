using System;
using System.Reflection;

namespace EncodeDecodeAPMDZ.Utils.Extensions
{
    internal static class AssemblyExtensions
    {
        //---------------------------------------------------------------------
        public static Version AssemblyVersion(this Assembly assembly)
        {
            if (assembly == null)
            { throw new ArgumentNullException(nameof(assembly)); }

            return assembly.GetName().Version;
        }
        public static string AssemblyProduct(this Assembly assembly)
        {
            if (assembly == null)
            { throw new ArgumentNullException(nameof(assembly)); }

            var attributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            return (attributes.Length != 0) ? ((AssemblyProductAttribute)attributes[0]).Product : string.Empty;
        }
        public static string AssemblyCopyright(this Assembly assembly)
        {
            if (assembly == null)
            { throw new ArgumentNullException(nameof(assembly)); }

            var attributes = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            return (attributes.Length != 0) ? ((AssemblyCopyrightAttribute)attributes[0]).Copyright : string.Empty;
        }
        public static string AssemblyCompany(this Assembly assembly)
        {
            if (assembly == null)
            { throw new ArgumentNullException(nameof(assembly)); }

            var attributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
            return (attributes.Length != 0) ? ((AssemblyCompanyAttribute)attributes[0]).Company : string.Empty;
        }
        //---------------------------------------------------------------------
    }
}