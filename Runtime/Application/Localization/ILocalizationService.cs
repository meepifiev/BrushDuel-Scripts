namespace _Project.Application.Localization
{
    public interface ILocalizationService
    {
        string Get(string key);

        string Localize(string russian, string english);
    }
}
