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
using System.Windows.Input;

namespace PlainPasswordManager.ViewModel.Commands
{
    /// <summary>
    /// Open Help Window
    /// </summary>
    public class OpenHelpCommand : BaseCommand, ICommand
    {
        private MainWindowViewModel _vm;
        
        public OpenHelpCommand(MainWindowViewModel vm)
        {
            _vm = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _vm.OpenHelp();
        }
    }
}
