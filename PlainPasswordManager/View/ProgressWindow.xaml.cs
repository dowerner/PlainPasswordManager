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
using System.Threading.Tasks;
using System.Windows;

namespace PlainPasswordManager.View
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        #region Members
        private Task _taskToAwait;
        #endregion

        /// <summary>
        /// Give task to await
        /// </summary>
        /// <param name="task"></param>
        public ProgressWindow(Task task, string text)
        {
            InitializeComponent();
            _taskToAwait = task;
            LoadingText.Text = text;
            Task.Run(() => WaitForTaskToComplete());
            ShowDialog();            
        }

        /// <summary>
        /// Wait for given task to complete and close the window once the task is done
        /// </summary>
        private void WaitForTaskToComplete()
        {
            _taskToAwait.Wait();
            Dispatcher.BeginInvoke((Action)delegate
            {
                Close();
            });            
        }
    }
}
