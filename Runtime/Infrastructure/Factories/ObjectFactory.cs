using _Project.Application.Factories;
using UnityEngine;

namespace _Project.Infrastructure.Factories
{
    public class ObjectFactory : IObjectFactory
    {
        public T Create<T>(T prefab, Transform parent) where T : Component
        {
            return Object.Instantiate(prefab, parent);
        }
    }
}
