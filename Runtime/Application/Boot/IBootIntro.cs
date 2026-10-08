using System.Threading;
using Cysharp.Threading.Tasks;

namespace _Project.Application.Boot
{
    public interface IBootIntro
    {
        UniTask PlayAsync(CancellationToken cancellationToken);
    }
}
