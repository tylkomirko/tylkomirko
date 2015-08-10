using WykopSDK.Storage;

namespace WykopSDK
{
    public static class WykopSDK
    {
        private static LocalStorage _localStorage = null;
        public static LocalStorage LocalStorage
        {
            get { return _localStorage ?? (_localStorage = new LocalStorage()); }
        }

        private static VaultStorage _vaultStorage = null;
        public static VaultStorage VaultStorage
        {
            get { return _vaultStorage ?? (_vaultStorage = new VaultStorage()); }
        }
    }
}
