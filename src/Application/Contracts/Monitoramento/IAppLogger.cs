namespace Application.Contracts.Monitoramento;

public interface IAppLogger
{
    void LogDebug(string messageTemplate, params object[] args);
    void LogInformation(string messageTemplate, params object[] args);
    void LogWarning(string messageTemplate, params object[] args);
    void LogError(string messageTemplate, params object[] args);
    void LogError(Exception ex, string messageTemplate, params object[] args);
    void LogCritical(string messageTemplate, params object[] args);
    void LogCritical(Exception ex, string messageTemplate, params object[] args);
    IAppLogger ComPropriedade(string key, object? value);
}