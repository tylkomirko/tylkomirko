using System;

namespace WykopAPI
{
    public class MessageEventArgs : EventArgs
    {
        private readonly string _message;
        private readonly int _code;

        public string Message
        {
            get { return this._message; }
        }

        public int Code
        {
            get { return this._code; }
        }

        public MessageEventArgs(string _m, int _c)
        {
            this._message = _m;
            this._code = _c;
        }
    }

    public class NetworkEventArgs : EventArgs
    {
        private readonly bool _isNetworkAvailable;
        private readonly bool _isWIFIAvailable;

        public bool IsNetworkAvailable
        {
            get { return this._isNetworkAvailable; }
        }

        public bool IsWIFIAvailable
        {
            get { return this._isWIFIAvailable; }
        }

        public NetworkEventArgs(bool _net, bool _wifi)
        {
            this._isNetworkAvailable = _net;
            this._isWIFIAvailable = _wifi;
        }
    }
}
