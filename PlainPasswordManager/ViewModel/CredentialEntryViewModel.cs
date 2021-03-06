﻿/*
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
using PlainPasswordManager.Model;
using PlainPasswordManager.View;
using PlainPasswordManager.ViewModel.Commands;
using System.Security;
using System.Windows.Input;

namespace PlainPasswordManager.ViewModel
{
    /// <summary>
    /// View model representation of a credential entry
    /// </summary>
    public class CredentialEntryViewModel : BaseViewModel
    {
        #region Events
        public delegate void CredentialEntryChangedHandler(CredentialEntryViewModel sender);

        /// <summary>
        /// Fires if something changes in the entry (during editing)
        /// </summary>
        public event CredentialEntryChangedHandler CredentialEntryChanged;
        #endregion

        #region Properties

        public bool VisibleByFilter
        {
            get => _visibleByFilter;
            set
            {
                _visibleByFilter = value;
                NotifyPropertyChanged(nameof(VisibleByFilter));
            }
        }
        private bool _visibleByFilter;

        /// <summary>
        /// Gets or sets the title of the entry
        /// </summary>
        public string Title
        {
            get => _model.Title;
            set
            {
                _model.Title = value;
                NotifyPropertyChanged(nameof(Title));
                CredentialEntryChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Gets or sets the Id of the entry
        /// </summary>
        public int Id
        {
            get => _model.Id;
            set
            {
                _model.Id = value;
                NotifyPropertyChanged(nameof(Id));
                CredentialEntryChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Gets or sets the user name of the entry
        /// </summary>
        public string UserName
        {
            get => _model.UserName;
            set
            {
                _model.UserName = value;
                NotifyPropertyChanged(nameof(UserName));
                CredentialEntryChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Gets or sets the provided name of the entry
        /// </summary>
        public string ProvidedName
        {
            get => _model.ProvidedName;
            set
            {
                _model.ProvidedName = value;
                NotifyPropertyChanged(nameof(ProvidedName));
                CredentialEntryChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Gets or sets mail address of the entry
        /// </summary>
        public string Email
        {
            get => _model.Email;
            set
            {
                _model.Email = value;
                NotifyPropertyChanged(nameof(Email));
                CredentialEntryChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Gets or sets the URL of the entry
        /// </summary>
        public string Url
        {
            get => _model.Url;
            set
            {
                _model.Url = value;
                NotifyPropertyChanged(nameof(Url));
            }
        }

        /// <summary>
        /// Gets or sets the address of the entry
        /// </summary>
        public string AddressData
        {
            get => _model.AddressData;
            set
            {
                _model.AddressData = value;
                NotifyPropertyChanged(nameof(AddressData));
                CredentialEntryChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Gets or sets the password of the entry.
        /// </summary>
        public string Password
        {
            get => _model.Password.Value;
            set
            {
                _model.Password.SetEncryptedPassword(value);
                NotifyPropertyChanged(nameof(Password));
            }
        }

        /// <summary>
        /// Gets or sets if the password is should be displayed in clear text
        /// </summary>
        public bool ShowPassword
        {
            get => !_model.Password.Encrypted;
            set
            {
                _model.Password.Encrypted = !value;
                NotifyPropertyChanged(nameof(ShowPassword));
                NotifyPropertyChanged(nameof(Password));
            }
        }

        /// <summary>
        /// Gets if the password can be shown (false if no password has been added yet)
        /// </summary>
        public bool ShowPasswordEnabled
        {
            get
            {
                string encryptedPassword = _model.Password.GetEncryptedPassword();
                return encryptedPassword != null && encryptedPassword != string.Empty;
            }
        }
        #endregion

        #region Members
        private CredentialEntry _model; // underlying credential entry
        #endregion

        #region Commands
        public ICommand SetEntryPasswordCommand { get; private set; }
        public ICommand RemoveCredentialCommand { get; set; }
        #endregion

        public CredentialEntryViewModel(CredentialEntry entry)
        {
            // initialize the view model
            _model = entry;
            SetEntryPasswordCommand = new SetEntryPasswordCommand(this);
            VisibleByFilter = true;
        }

        #region PublicMethods
        /// <summary>
        /// Convert and return the underlying data model to a serializeable format
        /// </summary>
        /// <returns></returns>
        public SerializeableCredentialEntry ToSerializeable()
        {
            return _model.ToSerializeable();
        }

        /// <summary>
        /// Set master password which is used to decrypt the passwords. (used when master password changes)
        /// </summary>
        /// <param name="masterPassword"></param>
        public void SetMasterPassword(SecureString masterPassword)
        {
            _model.DecryptionPassword = masterPassword;
            _model.Password.SetMasterPassword(masterPassword);
            NotifyPropertyChanged(nameof(ShowPassword));
        }

        /// <summary>
        /// Get encrypted password text.
        /// </summary>
        /// <returns></returns>
        public string GetEncryptedPassword()
        {
            return _model.Password.GetEncryptedPassword();
        }

        /// <summary>
        /// Returns the model of this view model.
        /// </summary>
        /// <returns></returns>
        public CredentialEntry GetModel()
        {
            return _model;
        }

        /// <summary>
        /// Show password input window
        /// </summary>
        public void SetPassword()
        {
            PasswordInputWindow pwWindow = new PasswordInputWindow(PasswordInputMode.NewPassword, _model.DecryptionPassword);
            pwWindow.PasswordFound += PwWindow_PasswordFound;
            pwWindow.ShowDialog();
        }
        #endregion

        #region HelpMethods
        /// <summary>
        /// Callback for the password change window
        /// </summary>
        private void PwWindow_PasswordFound(string encryptedPassword)
        {
            Password = encryptedPassword;
            NotifyPropertyChanged(nameof(ShowPasswordEnabled));
            CredentialEntryChanged?.Invoke(this);
        }
        #endregion
    }
}
