using System;
using Windows.Security.Credentials;

namespace WykopSDK.Storage
{
    public class VaultStorage
    {
        private const string CREDENTIALS = "Credentials";

        public void SaveCredentials(string username, string password)
        {
            var vault = new PasswordVault();
            var credential = new PasswordCredential(CREDENTIALS, username, password);
            vault.Add(credential);
        }

        public void RemoveCredentials()
        {
            var vault = new PasswordVault();
            try
            {
                // Removes the credential from the password vault.
                var credentials = vault.FindAllByResource(CREDENTIALS);
                foreach(var cred in credentials)
                    vault.Remove(cred);
            }
            catch (Exception)
            {
                // If no credentials have been stored with the given RESOURCE_NAME, an exception
                // is thrown.
            }

        }
    }
}
