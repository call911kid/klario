using Klario.Providers.Contracts;

namespace Klario.BLL.Interfaces;

public interface IPostingAlertFormatter
{
    string Format(ExternalPosting posting);
}
