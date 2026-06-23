using System.Collections.Concurrent;

namespace ApartmentRentalSystem.Bot.Services;

public class UserSessionService
{
    private readonly ConcurrentDictionary<long, UserSession> _sessions = new();

    /// <summary>
    /// Получить существующую сессию или создать новую
    /// </summary>
    public UserSession GetOrCreateSession(long chatId)
    {
        return _sessions.GetOrAdd(chatId, id => new UserSession(id));
    }

    /// <summary>
    /// Получить сессию (может вернуть null)
    /// </summary>
    public UserSession? GetSession(long chatId)
    {
        return _sessions.TryGetValue(chatId, out var session) ? session : null;
    }

    /// <summary>
    /// Удалить сессию
    /// </summary>
    public void RemoveSession(long chatId)
    {
        _sessions.TryRemove(chatId, out _);
    }

    /// <summary>
    /// Обновить сессию
    /// </summary>
    public void UpdateSession(long chatId, Action<UserSession> updateAction)
    {
        var session = GetOrCreateSession(chatId);
        updateAction(session);
    }
}