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
using System.Collections.Generic;

namespace PlainPasswordManager.Model
{
    /// <summary>
    /// Data to be serialized into binary format.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public List<SerializeableCredentialEntry> CredentialEntries { get; set; }
    }
}
