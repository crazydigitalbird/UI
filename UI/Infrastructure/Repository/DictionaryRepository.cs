﻿using UI.Models;
using System.Collections.Concurrent;

namespace UI.Infrastructure.Repository
{
    public class DictionaryChatRepository : IDictionaryRepository<SheetDialogKey, NewMessage>
    {
        private Dictionary<SheetDialogKey, NewMessage> _dictionaryActive = new Dictionary<SheetDialogKey, NewMessage>();

        private ConcurrentDictionary<int, int> _dictionaryOnline = new ConcurrentDictionary<int, int>();

        public Dictionary<SheetDialogKey, NewMessage> Active
        {
            get => _dictionaryActive;
            set
            {
                _dictionaryActive = value;
            }
        }

        public ConcurrentDictionary<int, int> Online
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
        public bool IsDeleted { get; set; }

        public bool Equals(NewMessage x, NewMessage y)
        {
            return x.Dialogue.IdUser == y.Dialogue?.IdUser && x.Dialogue.IdInterlocutor == y.Dialogue.IdInterlocutor && x.Dialogue.LastMessage.Id == y.Dialogue.LastMessage.Id;
        }

        public int GetHashCode(NewMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(NewMessage));
            }
            return HashCode.Combine(message.Dialogue.IdUser, message.Dialogue.IdInterlocutor, message.Dialogue.LastMessage.Id);
        }
    }
}
