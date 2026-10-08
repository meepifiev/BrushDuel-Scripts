using Cysharp.Threading.Tasks;

namespace _Project.Application.Platform
{
    public interface IPlatformInitializer
    {
        UniTask InitializeAsync();
    }
}
