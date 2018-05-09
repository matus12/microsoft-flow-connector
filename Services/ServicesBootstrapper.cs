using Contracts;
using Contracts.Services;
using Unity;
using Unity.Lifetime;

namespace Services
{
    public class ServicesBootstrapper : IBootstrapper
    {
        public IUnityContainer RegisterTypes(IUnityContainer container)
            => container
                .RegisterType<IFlowInfoService, Services.FlowInfoService>(new ContainerControlledLifetimeManager());
    }
}
