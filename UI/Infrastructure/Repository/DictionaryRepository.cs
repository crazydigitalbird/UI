using Core.Models.Sheets;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using UI.Models;

namespace UI.Infrastructure.Repository
{
    public class DictionaryChatRepository : IDictionaryRepository<SheetDialogKey, NewMessage>
    {
        private ConcurrentDictionary<SheetDialogKey, NewMessage> _dictionaryActive = new ConcurrentDictionary<SheetDialogKey, NewMessage>();

        private ConcurrentDictionary<SheetDialogKey, NewMessage> _dictionaryOnline = new ConcurrentDictionary<SheetDialogKey, NewMessage>();

        public ConcurrentDictionary<SheetDialogKey, NewMessage> Active
        {
            get => _dictionaryActive;
            set
            {
                _dictionaryActive = value;
            }
        }

        public ConcurrentDictionary<SheetDialogKey, NewMessage> Online
        {
            get => _dictionaryOnline;
            set
            {
                _dictionaryOnline = value;
            }
        }
    }

    public struct SheetDialogKey : IEqualityComparer<SheetDialogKey>
    {
        public int SheetId;
        public int IdInterlocutor;

        public SheetDialogKey(int sheetId, int idInterlocutor)
        {
            this.SheetId = sheetId;
            this.IdInterlocutor = idInterlocutor;
        }

        public bool Equals(SheetDialogKey x, SheetDialogKey y)
        {
            return x.SheetId == y.SheetId && x.IdInterlocutor == y.IdInterlocutor;
        }

        public int GetHashCode(SheetDialogKey key)
        {
           return HashCode.Combine(key.SheetId, key.IdInterlocutor);
        }
    }

    public class NewMessage : IEqualityComparer<NewMessage>
    {
        public SheetInfo SheetInfo { get; set; }
        public Dialogue Dialogue { get; set; }

        public bool Equals(NewMessage x, NewMessage y)
        {
            return x.Dialogue.IdUser == y.Dialogue?.IdUser && x.Dialogue.IdInterlocutor == y.Dialogue.IdInterlocutor && x.Dialogue.LastMessage.Id == y.Dialogue.LastMessage.Id;
        }

        public int GetHashCode(NewMessage message)
        {
            if(message == null)
            {
                throw new ArgumentNullException(nameof(NewMessage));
            }
            return HashCode.Combine(message.Dialogue.IdUser, message.Dialogue.IdInterlocutor, message.Dialogue.LastMessage.Id);
        }
    }
}
