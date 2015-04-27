using System;

namespace WykopAPI
{
    public class MessageEventArgs : EventArgs
    {
        private readonly string _message;

        public string Message
        {
            get { return this._message; }
        }

        public MessageEventArgs(string _m)
        {
            this._message = _m;
        }
    }
}
