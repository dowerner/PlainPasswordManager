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
using Microsoft.Win32;
using PlainPasswordManager.Model;
using PlainPasswordManager.ViewModel.Commands;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace PlainPasswordManager.ViewModel
{
    /// <summary>
    /// Settings window view model
    /// </summary>
    public class SettingsViewModel : BaseViewModel
    {
        #region Events
        public delegate void RequestCloseHandler();
        public event RequestCloseHandler RequestClose;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the primary save path
        /// </summary>
        public string PrimaryPath
        {
            get => _primaryPath;
            set
            {
                _primaryPath = value;
                NotifyPropertyChanged(nameof(PrimaryPath));
            }
        }
        private string _primaryPath;

        /// <summary>
        /// List of secondary save paths
        /// </summary>
        public ObservableCollection<PathInformation> SecondaryPaths { get; private set; }
        #endregion

        #region PrivateMembers
        private MainWindowViewModel _mainWindowViewModel;
        #endregion

        #region Commands
        public ICommand SetFilePathCommand { get; private set; }
        public ICommand AddSavePathCommand { get; private set; }
        public ICommand RemovePathCommand { get; private set; }
        public ICommand ImportCommand { get; private set; }
        public ICommand CloseSettingsCommand { get; private set; }
        #endregion

        public SettingsViewModel(MainWindowViewModel mainWindowViewModel)
        {
            // initialize the view model
            PrimaryPath = FileDirector.Instance.PrimarySavePath;
            SecondaryPaths = new ObservableCollection<PathInformation>();
            _mainWindowViewModel = mainWindowViewModel;

            // initialize commands
            SetFilePathCommand = new SetFilePathCommand(this);
            AddSavePathCommand = new AddSavePathCommand(this);
            RemovePathCommand = new RemovePathCommand(this);
            ImportCommand = new ImportCommand(this);
            CloseSettingsCommand = new CloseSettingsCommand(this);

            // add the existing secondary paths to the settings window
            foreach (string secondaryPassword in FileDirector.Instance.SecondarySavePaths)
            {
                SecondaryPaths.Add(new PathInformation() { Path = secondaryPassword, SetFilePathCommand = SetFilePathCommand, RemovePathCommand = RemovePathCommand });
            }
        }

        #region PublicMethods
        /// <summary>
        /// Set the configuration on the file director
        /// </summary>
        public void WriteBack()
        {
            FileDirector.Instance.PrimarySavePath = PrimaryPath;
            FileDirector.Instance.SecondarySavePaths.Clear();
            foreach (PathInformation secondaryPath in SecondaryPaths) FileDirector.Instance.SecondarySavePaths.Add(secondaryPath.Path);
        }

        /// <summary>
        /// Add new path to the secondary path list
        /// </summary>
        public void AddPath()
        {
            PathInformation newEntry = new PathInformation() { SetFilePathCommand = SetFilePathCommand, RemovePathCommand = RemovePathCommand };
            SecondaryPaths.Add(newEntry);
            SetFilePath(newEntry);
        }

        /// <summary>
        /// Ask if the selected path should be removed
        /// </summary>
        /// <param name="parameter"></param>
        public void RemovePath(object parameter)
        {
            PathInformation pathInfo = (PathInformation)parameter;
            MessageBoxResult result = MessageBox.Show("Do you want to remove this storage location?", "Remove Storage Location", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if(result == MessageBoxResult.Yes)
            {
                if (File.Exists(pathInfo.Path)) File.Delete(pathInfo.Path);
                SecondaryPaths.Remove(pathInfo);
            }
        }

        /// <summary>
        /// Show dialog to set a new path
        /// </summary>
        /// <param name="parameter"></param>
        public void SetFilePath(object parameter)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.AddExtension = true;
            saveDialog.FileName = FileDirector.SaveFilePath;
            saveDialog.DefaultExt = ".pass";
            saveDialog.Filter = "Password Files (.pass) | *.pass";
            bool completed = (bool)saveDialog.ShowDialog();

            if (!completed) return;

            if(parameter == null)
            {
                PrimaryPath = saveDialog.FileName;
            }
            else
            {
                ((PathInformation)parameter).Path = saveDialog.FileName;
            }
        }

        public void CloseSettings()
        {
            RequestClose?.Invoke();
        }

        /// <summary>
        /// Ask user if he wants to overwrite the current data with an existing file and import it if confirmed.
        /// </summary>
        public void ImportPasswordFile()
        {
            MessageBoxResult result = MessageBox.Show("If you import an existing password storage file all your current data will be lost! Are you sure you want to import an existing file?", "Import Password Storage", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if(result == MessageBoxResult.Yes)
            {
                try
                {
                    OpenFileDialog openDialog = new OpenFileDialog();
                    openDialog.AddExtension = true;
                    openDialog.DefaultExt = ".pass";
                    openDialog.Filter = "Password Files (.pass) | *.pass";
                    bool completed = (bool)openDialog.ShowDialog();

                    if (!completed) return;

                    DateTime today = DateTime.Today;
                    string date_prefix = string.Format("{0}-{1}-{2}", today.Year.ToString(), today.Month.ToString(), today.Day.ToString());
                    string backup_file = string.Format("{0}_{1}.pass", FileDirector.Instance.PrimarySavePath.Replace(".pass", string.Empty), date_prefix);

                    int number = 0;
                    while (File.Exists(backup_file))
                    {
                        backup_file = string.Format("{0}_{1}_{2}.pass", FileDirector.Instance.PrimarySavePath.Replace(".pass", string.Empty), number.ToString(), date_prefix);
                        number++;
                    }

                    File.Copy(FileDirector.Instance.PrimarySavePath, backup_file);

                    MessageBox.Show(string.Format("The current data will now be deleted and the new data will be imported. A backup of the current data can be found in the file:\r\n{0}", backup_file), "Import Password Storage", MessageBoxButton.OK, MessageBoxImage.Information);

                    FileDirector.Instance.PrimarySavePath = openDialog.FileName;
                    PrimaryPath = FileDirector.Instance.PrimarySavePath;
                    FileDirector.Instance.SaveSettings();
                    _mainWindowViewModel.Init();
                }
                catch(Exception e)
                {
                    MessageBox.Show(string.Format("Unable to complete import: {0}", e.Message), "Import failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                
            }
        }
        #endregion
    }
}
