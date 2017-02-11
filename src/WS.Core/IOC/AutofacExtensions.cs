using System;
using System.Linq;
using System.Collections.Generic;
using Autofac;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace WS.Core.IOC
{
    public static class AutofacExtensions
    {
        /// <summary>
        /// 注册指定文件目录下面的所有文件
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="fileFilter"></param>
        /// <param name="directoryPaths"></param>
        public static void RegisterTypeFromCurrentDomain(this ContainerBuilder builder)
        {
            var assemblyNameList = DependencyContext.Default.GetDefaultAssemblyNames();
            foreach (var assemblyName in assemblyNameList)
            {
                var assembly = Assembly.Load(assemblyName);
                RegisterTypeFromAssembly(builder, assembly);
            }
        }
        /// <summary>
        /// 注册指定文件路径的文件
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="fileFullName"></param>
        public static void RegisterTypeFromAssembly(this ContainerBuilder builder, Assembly assembly)
        {
            var allClass = from type in assembly.GetExportedTypes()
                           where type.GetTypeInfo().IsClass
                           select type;
            foreach (var c in allClass)
            {
                var exportAttrs = c.GetTypeInfo().GetCustomAttributes(typeof(IocExportAttribute), false).ToList();
                if (exportAttrs.Count > 0)
                {
                    var exportAttr = exportAttrs[0] as IocExportAttribute;
                    if (null != exportAttr.ContractKey)
                    {
                        if (exportAttr.SingleInstance)
                            builder.RegisterType(c).Keyed(exportAttr.ContractKey, exportAttr.ContractType).SingleInstance();
                        else
                            builder.RegisterType(c).Keyed(exportAttr.ContractKey, exportAttr.ContractType);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(exportAttr.ContractName))
                        {
                            if (exportAttr.SingleInstance)
                                builder.RegisterType(c).Named(exportAttr.ContractName, exportAttr.ContractType).SingleInstance();
                            else
                                builder.RegisterType(c).Named(exportAttr.ContractName, exportAttr.ContractType);
                        }
                        else
                        {
                            if (exportAttr.SingleInstance)
                                builder.RegisterType(c).As(exportAttr.ContractType).SingleInstance();
                            else
                                builder.RegisterType(c).As(exportAttr.ContractType);
                        }
                    }
                }
            }
        }
    }
}
