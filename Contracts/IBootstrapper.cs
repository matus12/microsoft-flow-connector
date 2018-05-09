using Unity;

namespace Contracts
{
    public interface IBootstrapper
    {
        IUnityContainer RegisterTypes(IUnityContainer container);
    }
}
