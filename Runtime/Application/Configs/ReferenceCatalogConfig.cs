using System;
using UnityEngine;

namespace _Project.Application.Configs
{
    [CreateAssetMenu(fileName = "ReferenceCatalogConfig", menuName = "BrushDuel/Reference Catalog", order = 0)]
    public class ReferenceCatalogConfig : ScriptableObject
    {
        [SerializeField] private ReferenceConfig[] _references;

        public int Count => _references.Length;

        public ReferenceConfig GetAt(int index)
        {
            return _references[index];
        }

        public ReferenceConfig GetById(string id)
        {
            for (int i = 0; i < _references.Length; i++)
            {
                if (_references[i].Id == id)
                {
                    return _references[i];
                }
            }

            throw new ArgumentOutOfRangeException(nameof(id));
        }
    }
}
