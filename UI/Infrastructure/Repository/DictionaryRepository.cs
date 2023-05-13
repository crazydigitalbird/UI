using Core.Models.Sheets;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using UI.Models;

namespace UI.Infrastructure.Repository
{
    public class DictionaryChatRepository : IDictionaryRepository<SheetDialogKey, NewMessage>
    {
        private ConcurrentDictionary<SheetDialogKey, NewMessage> _dictionaryActive = new ConcurrentDictionary<SheetDialogKey, NewMessage>();

        private ConcurrentDictionary<SheetDialogKey, NewMessage> _dictionaryOnline = new ConcurrentDictionary<SheetDialogKey, NewMessage>();

        public ConcurrentDictionary<SheetDialogKey, NewMessage> Active
        {
            get => _dictionaryOnline;
            set
            {
                _dictionaryOnline = value;
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

    public struct SheetDialogKey
    {
        public int SheetId;
        public int IdInterlocutor;

        public SheetDialogKey(int sheetId, int idInterlocutor)
        {
            this.SheetId = sheetId;
            this.IdInterlocutor = idInterlocutor;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SheetId, IdInterlocutor);
        }

        public override bool Equals(object obj)
        {
            return obj is SheetDialogKey other && (SheetId == other.SheetId && IdInterlocutor == other.IdInterlocutor);
        }
    }

    public class NewMessage
    {
        public SheetInfo SheetInfo { get; set; }
        public Dialogue Dialogue { get; set; }
    }
}
