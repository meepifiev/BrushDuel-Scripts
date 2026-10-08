using System;
using _Project.Application.Factories;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Project.Infrastructure.Factories
{
    public class InjectObjectFactory : IInjectObjectFactory
    {
        private readonly IObjectResolver _resolver;

        public InjectObjectFactory(IObjectResolver resolver)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        public T Create<T>(T prefab, Transform parent) where T : Component
        {
            return _resolver.Instantiate(prefab, parent);
        }
    }
}
