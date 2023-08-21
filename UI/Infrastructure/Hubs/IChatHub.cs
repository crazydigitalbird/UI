using System.Collections.Concurrent;
using System.Collections.Immutable;
using UI.Infrastructure.Repository;

namespace UI.Infrastructure.Hubs
{
    public interface IChatHub
    {
        Task DeleteDialogs(IEnumerable<KeyValuePair<SheetDialogKey, long>> sheetsDialogues);

        Task AddDialogs(ImmutableDictionary<SheetDialogKey, NewMessage> sheetsDialogues);

        Task UpdateDialogs(IEnumerable<KeyValuePair<SheetDialogKey, long>> oldSheetsDialogues, ImmutableDictionary<SheetDialogKey, NewMessage> newSheetsDialogues);

        Task ReplyToNewMessage(int sheetId, int idInterlocutor, long idLastMessage, long idNewMessage);

        Task ChangeNumberOfUsersOnline(ConcurrentDictionary<int, int> online);

        Task ChangeSheetsStatusOnline(ConcurrentDictionary<int, bool> sheetsIsOnline);
    }
}
