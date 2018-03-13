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
using PlainPasswordManager.View;
using PlainPasswordManager.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace PlainPasswordManager.ViewModel
{
    /// <summary>
    /// View Model of the main window
    /// </summary>
    public class MainWindowViewModel : BaseViewModel
    {
        #region Events
        public delegate void OnShutdownHandler();

        /// <summary>
        /// Fires if the application is shutting down
        /// </summary>
        public event OnShutdownHandler OnShutdown;
        #endregion

        #region Constants
        private const string windowTitle = "Plain Password Manager";    // window title
        private const int expandedGridHeight = 28;
        private const int collapsedGridHeight = 0;
        #endregion

        #region Properties
        /// <summary>
        /// Credential entries which are listed in the main window
        /// </summary>
        public ObservableCollection<CredentialEntryViewModel> CredentialEntries { get; private set; }

        /// <summary>
        /// View of credential entries which can be used to sort and search without chaning the underlying list.
        /// </summary>
        private ICollectionView _credentialEntriesView;

        /// <summary>
        /// Gets or sets if change was applied to data.
        /// </summary>
        public bool WasEdited
        {
            get { return _wasEdited; }
            set
            {
                _wasEdited = value;
                NotifyPropertyChanged("WasEdited");
                NotifyPropertyChanged("WindowTitle");
            }
        }
        private bool _wasEdited;

        /// <summary>
        /// Gets the current window title. (* appended if change was applied)
        /// </summary>
        public string WindowTitle
        {
            get
            {
                if (_wasEdited) return windowTitle + " (*)";
                else return windowTitle;
            }
        }

        /// <summary>
        /// Gets or sets if the filter bar should be displayed.
        /// </summary>
        public bool ShowFilters
        {
            get { return _showFilters; }
            set
            {
                _showFilters = value;
                NotifyPropertyChanged("ShowFilters");
                NotifyPropertyChanged("FilterRowHeight");
                if (_showFilters)
                {
                    _showSorting = false;
                    NotifyPropertyChanged("ShowSorting");
                    NotifyPropertyChanged("SortingRowHeight");
                }
            }
        }
        private bool _showFilters;

        /// <summary>
        /// Gets the grid height for the filter row
        /// </summary>
        public GridLength FilterRowHeight
        {
            get
            {
                return _showFilters ? new GridLength(expandedGridHeight) : new GridLength(collapsedGridHeight);
            }
        }

        /// <summary>
        /// Gets or sets if the soring bar should be displayed.
        /// </summary>
        public bool ShowSorting
        {
            get { return _showSorting; }
            set
            {
                _showSorting = value;
                NotifyPropertyChanged("ShowSorting");
                NotifyPropertyChanged("SortingRowHeight");
                if (_showSorting)
                {
                    _showFilters = false;
                    NotifyPropertyChanged("ShowFilters");
                    NotifyPropertyChanged("FilterRowHeight");
                }
            }
        }
        private bool _showSorting;

        /// <summary>
        /// Gets the grid height for the sorting row
        /// </summary>
        public GridLength SortingRowHeight
        {
            get
            {
                return _showSorting ? new GridLength(expandedGridHeight) : new GridLength(collapsedGridHeight);
            }
        }

        /// <summary>
        /// Gets or sets the search text to filter the list content.
        /// </summary>
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                NotifyPropertyChanged("SearchText");

                string lowerCase = _searchText.ToLower();

                // set filters
                _credentialEntriesView.Filter = delegate (object item)
                {
                    CredentialEntryViewModel entry = (CredentialEntryViewModel)item;
                    return (entry.Title != null && entry.Title.ToLower().Contains(lowerCase))
                        || (entry.UserName != null && entry.UserName.ToLower().Contains(lowerCase))
                        || (entry.AddressData != null && entry.AddressData.ToLower().Contains(lowerCase))
                        || (entry.Email != null && entry.Email.ToLower().Contains(lowerCase))
                        || (entry.ProvidedName != null && entry.ProvidedName.ToLower().Contains(lowerCase))
                        || (entry.Url != null && entry.Url.ToLower().Contains(lowerCase));
                };
            }
        }
        private string _searchText;

        public SortingMode SortedBy
        {
            get { return _sortedBy; }
            set
            {
                _sortedBy = value;
                NotifyPropertyChanged("SortedBy");

                using (_credentialEntriesView.DeferRefresh())
                {
                    _credentialEntriesView.SortDescriptions.Clear();
                    _credentialEntriesView.SortDescriptions.Add(new SortDescription(_sortedBy.ToString(), _sortDescending ? ListSortDirection.Descending : ListSortDirection.Ascending));
                }
            }
        }
        private SortingMode _sortedBy;

        /// <summary>
        /// Gets or sets if the list is sorted ascending or descending.
        /// </summary>
        public bool SortDescending
        {
            get { return _sortDescending; }
            set
            {
                _sortDescending = value;
                NotifyPropertyChanged("SortDescending");
                SortedBy = _sortedBy;
            }
        }
        private bool _sortDescending;
        #endregion

        #region Commands
        public ICommand AddEntryCommand { get; private set; }
        public ICommand OpenSettingsCommand { get; private set; }
        public ICommand RemoveCredentialCommand { get; private set; }
        public ICommand ChangeMasterPasswordCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand ResetSearchCommand { get; private set; }
        public ICommand OpenHelpCommand { get; private set; }
        public ICommand OpenAboutCommand { get; private set; }
        public object ApplicationDeployment { get; private set; }
        #endregion

        #region Members
        private SecureString _masterPassword;
        private bool _changeMasterPassword;
        private bool _wrongPassword;                    // keep track of the password entry status (is set by the opening thread)
        private string _openErrorMessage;               // error message which was encountered by the opeing thread (more stable to share it this way than to use a dispatcher to display the message)
        private List<CredentialEntry> _loadedEntries;   // credential entries loaded from the primary password storage file
        private HelpWindow _helpWindow;
        private bool _wasOpenedSuccessfully;
        #endregion

        public MainWindowViewModel()
        {
            // initialize default values
            _masterPassword = null;
            _changeMasterPassword = false;
            _wasOpenedSuccessfully = false;
            CredentialEntries = new ObservableCollection<CredentialEntryViewModel>();
            CredentialEntries.CollectionChanged += CredentialEntries_CollectionChanged;
            _credentialEntriesView = CollectionViewSource.GetDefaultView(CredentialEntries);
            SortedBy = SortingMode.Id;
            SortDescending = false;

            // initialize main window commands
            AddEntryCommand = new AddEntryCommand(this);
            OpenSettingsCommand = new OpenSettingsCommand(this);
            RemoveCredentialCommand = new RemoveCredentialCommand(this);
            ChangeMasterPasswordCommand = new ChangeMasterPasswordCommand(this);
            SaveCommand = new SaveCommand(this);
            ResetSearchCommand = new ResetSearchCommand(this);
            OpenHelpCommand = new OpenHelpCommand(this);
            OpenAboutCommand = new OpenAboutCommand(this);
        }

        #region PublicMethods
        /// <summary>
        /// Initialize application
        /// </summary>
        public void Init()
        {
            CheckPasswordFile();
        }

        /// <summary>
        /// Add credential entry in main view
        /// </summary>
        public void AddEntry()
        {
            CredentialEntry entry = new CredentialEntry(_masterPassword, string.Empty);
            entry.Id = CredentialEntries.Count;
            CredentialEntries.Add(new CredentialEntryViewModel(entry));
        }

        /// <summary>
        /// Open settings window
        /// </summary>
        public void OpenSettings()
        {
            SettingsWindow settingsWindow = new SettingsWindow(this);
            settingsWindow.ShowDialog();
            FileDirector.Instance.SaveSettings();
            SaveAsync();
        }

        /// <summary>
        /// Open Help Window
        /// </summary>
        public void OpenHelp()
        {
            _helpWindow = new HelpWindow();
            _helpWindow.Show();
        }

        /// <summary>
        /// Display about information
        /// </summary>
        public void OpenAbout()
        {
            string message = string.Format("Plain Password Manager (by Dominik Werner 2018), {0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            MessageBox.Show(message, "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Open window to change master password
        /// </summary>
        public void ChangeMasterPassword()
        {
            _changeMasterPassword = true;
            ShowPasswordWindow(true);
        }

        /// <summary>
        /// Ask if current entry should be removed
        /// </summary>
        /// <param name="parameter"></param>
        public void RemoveCredentialEntry(object parameter)
        {
            CredentialEntryViewModel entry = (CredentialEntryViewModel)parameter;
            MessageBoxResult result = MessageBox.Show(string.Format("Are you sure you want to remove '{0}'", entry.Title), "Remove Item", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if(result == MessageBoxResult.Yes) CredentialEntries.Remove(entry);
        }

        /// <summary>
        /// Used on closing to check if the user wants to save unsaved changes.
        /// </summary>
        public void HandleApplicationClosing()
        {
            if (_wasEdited && _wasOpenedSuccessfully)
            {
                MessageBoxResult result = MessageBox.Show("There are unsaved changes. Save changes before closing?", "Save Changes", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if(result == MessageBoxResult.Yes)
                {
                    SaveAsync();
                }
            }

            if (_helpWindow != null) _helpWindow.Close();
        }

        /// <summary>
        /// Save data asynchronously
        /// </summary>
        public void SaveAsync()
        {
            Task task = Task.Run(() => Save());
            ProgressWindow window = new ProgressWindow(task, "Saving...");
            window = null;
        }

        /// <summary>
        /// Save data to encrypted file
        /// </summary>
        public void Save()
        {
            List<string> fails = new List<string>();    // keep track of failed saves
            List<string> success = new List<string>();  // keep track of succeeded saves

            // convert to model
            List<CredentialEntry> credentialEntriesModel = new List<CredentialEntry>();
            foreach (CredentialEntryViewModel entry in CredentialEntries) credentialEntriesModel.Add(entry.GetModel());

            try
            {
                FileDirector.Instance.SaveEncrypted(FileDirector.SaveFilePath, credentialEntriesModel, _masterPassword); // save to storage file which should always remain next to the executable
                success.Add(FileDirector.SaveFilePath);
            }
            catch { fails.Add(FileDirector.SaveFilePath); }

            if (FileDirector.SaveFilePath != FileDirector.Instance.PrimarySavePath) // check if the primary save file differs from the storage file
            {
                try
                {
                    FileDirector.Instance.SaveEncrypted(FileDirector.Instance.PrimarySavePath, credentialEntriesModel, _masterPassword); // primary save
                    success.Add(FileDirector.Instance.PrimarySavePath);
                }
                catch { fails.Add(FileDirector.SaveFilePath); }
            }

            foreach (string secondaryPath in FileDirector.Instance.SecondarySavePaths)  // secondary saves
            {
                try
                {
                    FileDirector.Instance.SaveEncrypted(secondaryPath, credentialEntriesModel, _masterPassword);
                    success.Add(secondaryPath);
                }
                catch { fails.Add(FileDirector.SaveFilePath); }
            }

            if (fails.Count > 0)    // display report of fails during the save process
            {
                string failList = "Files were save failed:\r\n";
                foreach (string fail in fails) failList += " ->" + fail + "\r\n";
                string successList = "Files were save succeeded:\r\n";
                foreach (string successPath in success) successList += " ->" + successPath + "\r\n";
                string message = "There were some errors during the save process:\r\n" + failList + (success.Count > 0 ? successList : string.Empty);
                MessageBox.Show(message, "Errors while saving", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            if (success.Contains(FileDirector.Instance.PrimarySavePath)) WasEdited = false;
        }
        #endregion

        #region HelperMethods
        /// <summary>
        /// Check password file and load data
        /// </summary>
        private void CheckPasswordFile()
        {
            _masterPassword = null;

            bool fileFound = File.Exists(FileDirector.SettingsFilePath) || File.Exists(FileDirector.SaveFilePath);    // check if file exists (does not exist if application is started for the first time)

            ShowPasswordWindow(!fileFound); // ask user for master password (or to enter a new master password)

            if (fileFound)  // if a file was found try to decrypt the contents and load the data
            {
                CredentialEntries.Clear();
                _wrongPassword = true;
                while (_wrongPassword)   // loop unitl password was correctly entered or user quits
                {
                    try
                    {
                        if (_masterPassword == null) return;    // quit in case the user canceled password input

                        // Load configuration
                        if (File.Exists(FileDirector.SettingsFilePath)) FileDirector.Instance.LoadSettings();
                        else FileDirector.Instance.SaveSettings();  // create initial config

                        _loadedEntries = null;
                        Task task = Task.Run(() => LoadDataAsync());
                        ProgressWindow window = new ProgressWindow(task, "Loading...");
                        window = null;

                        if (_wrongPassword)  // if the password was wrong remove it and ask again
                        {
                            MessageBox.Show(_openErrorMessage, "Error while opening", MessageBoxButton.OK, MessageBoxImage.Error);
                            _masterPassword = null;
                            ShowPasswordWindow(false);
                        }
                        else if (_loadedEntries != null)
                        {
                            foreach (CredentialEntry entry in _loadedEntries)
                            {
                                CredentialEntries.Add(new CredentialEntryViewModel(entry));    // populate main view with loaded data
                            }
                            SaveAsync();     // synch secondary saves
                        }
                        else
                        {
                            throw new Exception("Unable to load data despite correct password.");
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Error while opening", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }
            else if (_masterPassword != null) SaveAsync();    // create initial save file
        }

        /// <summary>
        /// Used to load data asynchronously
        /// </summary>
        private void LoadDataAsync()
        {
            try
            {
                if (_masterPassword == null) return;    // quit in case the user canceled password input

                _loadedEntries = FileDirector.Instance.OpenEncrypted(FileDirector.Instance.PrimarySavePath, _masterPassword);    // load the data from the primary save file
                _wrongPassword = false;  // if everything up until this point was executed the password was correct
                _wasOpenedSuccessfully = true;
            }
            catch (Exception e)
            {
                _openErrorMessage = e.Message;
            }
        }

        /// <summary>
        /// Show password window
        /// </summary>
        /// <param name="firstTime"></param>
        private void ShowPasswordWindow(bool firstTime)
        {
            PasswordInputWindow pwWindow = new PasswordInputWindow(firstTime ? PasswordInputMode.NewMasterPassword : PasswordInputMode.Authorize, null);
            pwWindow.Closing += PwWindow_Closing;
            pwWindow.MasterPasswordFound += PwWindow_MasterPasswordFound;
            pwWindow.ShowDialog();
            pwWindow.MasterPasswordFound -= PwWindow_MasterPasswordFound;
            pwWindow.Closing -= PwWindow_Closing;
        }

        /// <summary>
        /// This method is needed in case the user cancles the input of the password and wants to shut down the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PwWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_masterPassword == null) OnShutdown?.Invoke();
        }

        /// <summary>
        /// Called once a master password has been entered in the password window (after it has been closed)
        /// </summary>
        /// <param name="password"></param>
        private void PwWindow_MasterPasswordFound(SecureString password)
        {
            if(_changeMasterPassword)   // if the master password has been changed
            {
                Task task = Task.Run(() => MigrateMasterPassword(password));
                ProgressWindow window = new ProgressWindow(task, "Changing Master Password...");
                window = null;
            }

            _masterPassword = password; // replace the master-password
            if (_changeMasterPassword)
            {
                _changeMasterPassword = false;
                Save();
            }
        }

        /// <summary>
        /// Re-Encrypt passwords with the new master password
        /// </summary>
        /// <param name="newMasterPassword"></param>
        private void MigrateMasterPassword(SecureString newMasterPassword)
        {
            // migrate passwords
            foreach (CredentialEntryViewModel entry in CredentialEntries)
            {
                // decrypt all passwords and re-encrypt them with the new master-password
                string oldEncryptedPassword = entry.GetEncryptedPassword();
                string newEncryptedPassword = AESEncryption.EncryptWithPassword(AESEncryption.DecryptWithPassword(oldEncryptedPassword, _masterPassword), newMasterPassword);
                entry.SetMasterPassword(newMasterPassword);
                entry.Password = newEncryptedPassword;              
            }
        }

        /// <summary>
        /// Used to relay commands to the view models of the list items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CredentialEntries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (CredentialEntryViewModel item in e.NewItems)
                {
                    item.CredentialEntryChanged += Item_CredentialEntryChanged; // wire this method so that changes in an item are being detected
                    item.RemoveCredentialCommand = RemoveCredentialCommand; // Relay command to remove entry to the view model
                }
            }
            else if(e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (CredentialEntryViewModel item in e.OldItems) item.CredentialEntryChanged -= Item_CredentialEntryChanged;   // remove callbacks (tidy up)
            }
            WasEdited = true;
        }

        /// <summary>
        /// Callback to indicate changes in list box items
        /// </summary>
        /// <param name="sender"></param>
        private void Item_CredentialEntryChanged(CredentialEntryViewModel sender)
        {
            WasEdited = true;
        }
        #endregion
    }

    public enum SortingMode
    {
        Id,
        Title,
        UserName,
        Url,
        ProvidedName,
        Address,
        Email
    }
}
