using UI.Infrastructure.Repository;

namespace UI.Infrastructure.Hubs
{
    public interface IChatHub
    {
        Task DeleteDialogs(IEnumerable<KeyValuePair<SheetDialogKey, long>> sheetsDialogues);

        Task AddDialogs(IEnumerable<KeyValuePair<SheetDialogKey, NewMessage>> sheetsDialogues);

        Task UpdateDialogs(IEnumerable<KeyValuePair<SheetDialogKey, long>> oldSheetsDialogues, IEnumerable<KeyValuePair<SheetDialogKey, NewMessage>> newSheetsDialogues);

        Task ReplyToNewMessage(int sheetId, int idInterlocutor, long idLastMessage, long idNewMessage);
    }
}
