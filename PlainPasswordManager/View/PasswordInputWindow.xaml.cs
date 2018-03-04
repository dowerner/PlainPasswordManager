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
using PlainPasswordManager.Model;
using PlainPasswordManager.ViewModel;
using System.Security;
using System.Windows;
using System.Windows.Controls;

namespace PlainPasswordManager.View
{
    /// <summary>
    /// Interaction logic for PasswordInputWindow.xaml
    /// </summary>
    public partial class PasswordInputWindow : Window
    {
        #region Events
        public delegate void PasswordFoundHandler(string encryptedPassword);

        /// <summary>
        /// Fires a password has been found
        /// </summary>
        public event PasswordFoundHandler PasswordFound;

        public delegate void MasterPasswordFoundHandler(SecureString password);

        /// <summary>
        /// Fires if a master password has been found
        /// </summary>
        public event MasterPasswordFoundHandler MasterPasswordFound;
        #endregion

        #region Members
        private PasswordInputViewModel _vm; // handle to view model
        #endregion

        public PasswordInputWindow(PasswordInputMode mode, SecureString encryptionPassword)
        {
            InitializeComponent();
            KeyDown += PasswordInputWindow_KeyDown;
            _vm = new PasswordInputViewModel(encryptionPassword);
            _vm.Mode = mode;
            DataContext = _vm;
        }

        #region HelpMethods
        /// <summary>
        /// Callback for keypresses
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PasswordInputWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
            {
                Enter();   
            }
            if (e.Key == System.Windows.Input.Key.Escape) Close();
        }

        private void Enter()
        {
            if(pw1.Password.Length == 0)
            {
                MessageBox.Show("Password cannot have length zero.", "Cannot Enter Password", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _vm.EncryptedPassword = AESEncryption.EncryptWithPassword(pw1.Password, _vm.MasterPassword);
            if (_vm.Mode == PasswordInputMode.NewPassword) PasswordFound?.Invoke(_vm.EncryptedPassword);
            else
            {
                if (_vm.Mode == PasswordInputMode.NewMasterPassword && !pw1.SecurePassword.IsEqualTo(pw2.SecurePassword))
                {
                    MessageBox.Show("The passwords do not match.", "Cannot set password", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                MasterPasswordFound?.Invoke(_vm.MasterPassword);
            }
            Close();
        }

        /// <summary>
        /// Callback to update password in viewmodel (passwordbox does not support bindings)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pw_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox pwBox = (PasswordBox)sender;
            if (_vm.Mode != PasswordInputMode.NewPassword) _vm.MasterPassword = pwBox.SecurePassword;
        }

        /// <summary>
        /// Callback for the password enter button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Enter();
        }
        #endregion
    }
}
