/*
    Copyright 2018 Dominik Werner
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
     http://www.apache.org/licenses/LICENSE-2.0
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System.Security;

namespace PlainPasswordManager.Model
{
    /// <summary>
    /// Credential Entry
    /// </summary>
    public class CredentialEntry
    {
        #region Properties
        public int Id { get; set; }
        public string Title { get; set; }
        public string UserName { get; set; }
        public string ProvidedName { get; set; }
        public string AddressData { get; set; }
        public string Email { get; set; }
        public Password Password { get; private set; }
        public string Url { get; set; }
        #endregion

        #region PublicMembers
        public SecureString DecryptionPassword;
        #endregion

        public CredentialEntry(SecureString decryptionPassword, string encryptedPassword)
        {
            DecryptionPassword = decryptionPassword;
            Password = new Password(decryptionPassword, encryptedPassword);
        }

        /// <summary>
        /// Convert the entry to a serializeable entry (so it can be saved to a binary file)
        /// </summary>
        /// <returns></returns>
        public SerializeableCredentialEntry ToSerializeable()
        {
            SerializeableCredentialEntry entry = new SerializeableCredentialEntry();
            Password.Encrypted = true;
            entry.Id = Id;
            entry.Title = Title;
            entry.UserName = UserName;
            entry.ProvidedName = ProvidedName;
            entry.AddressData = AddressData;
            entry.Email = Email;
            entry.Url = Url;
            entry.Password = Password.GetEncryptedPassword();
            return entry;
        }
    }
}
