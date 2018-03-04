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
using System.Windows;

namespace PlainPasswordManager.ViewModel
{
    /// <summary>
    /// Password input window view model
    /// </summary>
    public class PasswordInputViewModel : BaseViewModel
    {
        #region Properties
        /// <summary>
        /// Gets or sets the visibility of the second password field (repeat password field)
        /// </summary>
        public Visibility Pw2Visibility
        {
            get { return _pw2Visibility; }
            set
            {
                _pw2Visibility = value;
                NotifyPropertyChanged("Pw2Visibility");
            }
        }
        private Visibility _pw2Visibility;

        /// <summary>
        /// Gets or sets the title for the first password field
        /// </summary>
        public string Pw1Title
        {
            get { return _pw1Title; }
            set
            {
                _pw1Title = value;
                NotifyPropertyChanged("Pw1Title");
            }
        }
        private string _pw1Title;

        /// <summary>
        /// Gets or sets the title for the second password field
        /// </summary>
        public string Pw2Title
        {
            get { return _pw2Title; }
            set
            {
                _pw2Title = value;
                NotifyPropertyChanged("Pw2Title");
            }
        }
        private string _pw2Title;

        /// <summary>
        /// Gets or sets the mode of the password window. (New master password, authenitication via master password, set password of entry)
        /// </summary>
        public PasswordInputMode Mode
        {
            get { return _mode; }
            set
            {
                _mode = value;
                switch (_mode)
                {
                    case PasswordInputMode.Authorize:
                        Pw2Visibility = Visibility.Collapsed;
                        Pw1Title = "Enter Master Password:";
                        break;
                    case PasswordInputMode.NewMasterPassword:
                        Pw2Visibility = Visibility.Visible;
                        Pw1Title = "Enter new Master Password:";
                        Pw2Title = "Re-Enter Master Password:";
                        break;
                    case PasswordInputMode.NewPassword:
                        Pw2Visibility = Visibility.Collapsed;
                        Pw1Title = "Enter Password to save:";
                        break;
                }
            }
        }
        private PasswordInputMode _mode;

        /// <summary>
        /// Gets or sets the encrypted password string
        /// </summary>
        public string EncryptedPassword { get; set; }

        /// <summary>
        /// Gets or sets the master password to encrypt and decrypt passwords
        /// </summary>
        public SecureString MasterPassword { get; set; }
        #endregion

        public PasswordInputViewModel(SecureString encryptionPassword)
        {
            Mode = PasswordInputMode.Authorize;     // set the mode of the password window
            MasterPassword = encryptionPassword;    // store the master password
        }
    }

    public enum PasswordInputMode
    {
        Authorize,
        NewPassword,
        NewMasterPassword
    }
}
