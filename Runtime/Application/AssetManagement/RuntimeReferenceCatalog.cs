using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using _Project.Application.Configs;
using Cysharp.Threading.Tasks;

namespace _Project.Application.AssetManagement
{
    public class RuntimeReferenceCatalog : IReferenceCatalog
    {
        private ReferenceConfig[] _references = Array.Empty<ReferenceConfig>();

        public int Count => _references.Length;

        public UniTask InitAsync(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }

        public UniTask<ReferenceConfig> GetAtAsync(int index, CancellationToken cancellationToken)
        {
            return UniTask.FromResult(_references[index]);
        }

        public UniTask<ReferenceConfig> GetByIdAsync(string id, CancellationToken cancellationToken)
        {
            return UniTask.FromResult(GetById(id));
        }

        public ReferenceConfig GetAt(int index)
        {
            return _references[index];
        }

        public ReferenceConfig GetById(string id)
        {
            if (TryGetById(id, out ReferenceConfig reference))
            {
                return reference;
            }

            throw new ArgumentOutOfRangeException(nameof(id));
        }

        public bool TryGetById(string id, out ReferenceConfig reference)
        {
            for (int i = 0; i < _references.Length; i++)
            {
                if (_references[i].Id == id)
                {
                    reference = _references[i];
                    return true;
                }
            }

            reference = null;
            return false;
        }

        public void Set(IEnumerable<ReferenceConfig> references)
        {
            _references = references
                .Where(reference => reference != null)
                .OrderBy(reference => reference.Id, StringComparer.Ordinal)
                .ToArray();
        }
    }
}
