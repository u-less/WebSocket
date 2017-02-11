using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyModel;
using System.Reflection;
using System.Collections.Concurrent;

namespace WS.Core.Base
{
    public class MsgMapping
    {
        static MsgMapping()
        {
            var assemblyNameList = DependencyContext.Default.GetDefaultAssemblyNames();//.Where(a => a.FullName.IndexOf("Coin") != -1);
            var msgType = typeof(IMsg);
            var cmdAttributeType = typeof(MsgAttribute);
            foreach (var assemblyName in assemblyNameList)
            {
                var assembly = Assembly.Load(assemblyName);
                var allType = assembly.GetExportedTypes().Where(t => msgType.IsAssignableFrom(t));
                foreach (var type in allType)
                {
                    var attributes = type.GetTypeInfo().GetCustomAttributes(cmdAttributeType, true).ToList();
                    if (attributes.Count() > 0)
                    {
                        var cmdAttribute = attributes[0] as MsgAttribute;
                        if (cmdAttribute != null)
                        {
                            msgCommandDict.Add(cmdAttribute.Command, type);
                            msgTypeDict.Add(type, cmdAttribute.Command);
                        }
                    }
                }
            }
        }
        static Dictionary<Type, UInt16> msgTypeDict = new Dictionary<Type, UInt16>();
        static Dictionary<UInt16, Type> msgCommandDict = new Dictionary<ushort, Type>();

        public static bool TryGetCommand(Type type, out UInt16 command)
        {
            return msgTypeDict.TryGetValue(type, out command);
        }
        public static bool TryGetMsgType(UInt16 command, out Type type)
        {
            return msgCommandDict.TryGetValue(command, out type);
        }
    }
}
