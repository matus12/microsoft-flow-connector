using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Hosting;
using System.Web.Http.Metadata;
using Unity;
using Unity.Exceptions;

namespace Flow.Resolver
{
    internal sealed class UnityResolver : IDependencyResolver
    {
        private const string ModelValidatorCacheException = "System.Web.Http.Validation.IModelValidatorCache";

        private static IEnumerable<string> ExcludedTypes
        {
            get
            {
                yield return typeof(IHostBufferPolicySelector).FullName;
                yield return typeof(IHttpControllerSelector).FullName;
                yield return typeof(IHttpControllerActivator).FullName;
                yield return typeof(IHttpActionSelector).FullName;
                yield return typeof(IHttpActionInvoker).FullName;
                yield return typeof(IExceptionHandler).FullName;
                yield return typeof(IContentNegotiator).FullName;
                yield return typeof(ModelMetadataProvider).FullName;
                yield return ModelValidatorCacheException;
            }
        }

        private readonly IUnityContainer _container;
        private bool _disposed;

        public UnityResolver(IUnityContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public object GetService(Type serviceType)
        {
            try
            {
                return _container.Resolve(serviceType);
            }
            catch (ResolutionFailedException)
                when (ExcludedTypes.Contains(serviceType?.FullName))
            {
                return null;
            }
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            try
            {
                return _container.ResolveAll(serviceType);
            }
            catch (ResolutionFailedException)
                when (ExcludedTypes.Contains(serviceType?.FullName))
            {
                return Enumerable.Empty<object>();
            }
        }

        public IDependencyScope BeginScope()
        {
            var child = _container.CreateChildContainer();
            return new UnityResolver(child);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _container.Dispose();

            _disposed = true;
        }
    }
}