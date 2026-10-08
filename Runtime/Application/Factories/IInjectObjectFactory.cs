using UnityEngine;

namespace _Project.Application.Factories
{
    public interface IInjectObjectFactory
    {
        T Create<T>(T prefab, Transform parent) where T : Component;
    }
}
