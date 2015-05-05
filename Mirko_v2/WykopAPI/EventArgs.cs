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
