using System.Threading.Tasks;
namespace Wpc_SutilBox.Core
{
    public interface ILocalizationService
    {
        Task SetLanguageAsync(string language);
        string CurrentLanguage { get; }
    }
}

