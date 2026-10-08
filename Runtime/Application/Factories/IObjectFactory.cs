using UnityEngine;

namespace _Project.Application.Factories
{
    public interface IObjectFactory
    {
        T Create<T>(T prefab, Transform parent) where T : Component;
    }
}
