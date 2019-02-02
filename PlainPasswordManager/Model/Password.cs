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

using System;
using System.Security;

namespace PlainPasswordManager.Model
{
    /// <summary>
    /// Representation of a password. The password is secured and only the encrypted string is accessible.
    /// The decrypted value can be retrieved by setting Encrypted to false (if the master password ist correct)
    /// </summary>
    public class Password
    {
        #region Constants
        private const string _dummyPassword = "*******";
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets if the password is encrypted.
        /// If false the saved password will be accessable as clear text
        /// otherwise it will show a dummy password
        /// </summary>
        public bool Encrypted
        {
            get { return _encrypted; }
            set
            {
                if (_encrypted == value) return;    // don't do anything if the new value would be the same -> saves performance
                _encrypted = value;
                if (_encrypted) // if the password should be encrypted, destroy the clear text password in the memory
                {
                    AESEncryption._destroyPasswordInMemory(ref _passwordValue);
                    _passwordValue = null;
                    GC.Collect();
                }
                else
                {
                    _passwordValue = AESEncryption.DecryptWithPassword(_encryptedPassword, _masterPassword);    // get clear text password
                }
            }
        }
        private bool _encrypted;

        /// <summary>
        /// Gets the value of the password (dummy or cleartext)
        /// </summary>
        public String Value
        {
            get
            {
                if (_encrypted) return _dummyPassword;
                else return _passwordValue;
            }
        }
        private String _passwordValue;
        #endregion

        #region Members
        SecureString _masterPassword;   // master password to encrypt and decrypt the password value
        private string _encryptedPassword;  // encrypted password
        #endregion

        public Password(SecureString masterPassword, string encryptedPasswordData)
        {
            _masterPassword = masterPassword;
            _encryptedPassword = encryptedPasswordData;
            _encrypted = true;
        }

        #region PublicMethods
        /// <summary>
        /// Set master password for decryption
        /// </summary>
        /// <param name="decryptionPassword"></param>
        public void SetMasterPassword(SecureString decryptionPassword)
        {
            _masterPassword = decryptionPassword;
        }

        /// <summary>
        /// Set encrypted password (used if the master password is changed)
        /// </summary>
        /// <param name="encryptedPassword"></param>
        public void SetEncryptedPassword(string encryptedPassword)
        {
            _encryptedPassword = encryptedPassword;
            if(!_encrypted) _passwordValue = AESEncryption.DecryptWithPassword(_encryptedPassword, _masterPassword);
        }

        /// <summary>
        /// Get the encrypted password
        /// </summary>
        /// <returns></returns>
        public string GetEncryptedPassword()
        {
            return _encryptedPassword;
        }
        #endregion
    }
}
