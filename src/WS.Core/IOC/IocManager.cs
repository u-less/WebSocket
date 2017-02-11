using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace WS.Core.IOC
{
    public class IocManager
    {
        static ContainerBuilder builder = new ContainerBuilder();
        static IContainer container = null;
        public void Build()
        {
            builder.RegisterTypeFromCurrentDomain();
            container = builder.Build();
        }
        public ContainerBuilder Builder
        {
            get
            {
                return builder;
            }
        }
        public IContainer Container
        {
            get
            {
                return container;
            }
        }
    }
}
