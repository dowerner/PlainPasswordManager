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

using PlainPasswordManager.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Xml.Serialization;

namespace PlainPasswordManager.Model
{
    /// <summary>
    /// Used for saving and loading files.
    /// </summary>
    public class FileDirector
    {
        /// <summary>
        /// The default save file location.
        /// </summary>
        public const string SaveFilePath = "PasswordStorage.pass";
        public const string SettingsFilePath = "PlainPasswordManager.xml";

        /// <summary>
        /// Gets or sets the location where the primary save file is stored.
        /// </summary>
        public string PrimarySavePath { get; set; }

        /// <summary>
        /// A list were all secondary save files (backups) are stored.
        /// </summary>
        public List<string> SecondarySavePaths { get; private set; }

        /// <summary>
        /// Save application settings for xml file
        /// </summary>
        public void SaveSettings()
        {
            PasswordMangerSettings settings = new PasswordMangerSettings() { PrimaryPath = PrimarySavePath, SecondaryPaths = SecondarySavePaths };
            XmlSerializer serializer = new XmlSerializer(typeof(PasswordMangerSettings));

            if (File.Exists(SettingsFilePath)) File.Copy(SettingsFilePath, SettingsFilePath + ".backup", true);

            try
            {
                StreamWriter writer = new StreamWriter(SettingsFilePath);
                serializer.Serialize(writer, settings);
                writer.Close();
            }
            catch
            {
                if (File.Exists(SettingsFilePath)) File.Delete(SettingsFilePath);   // rollback if something failed
                if (File.Exists(SettingsFilePath + ".backup")) File.Move(SettingsFilePath + ".backup", SettingsFilePath);
                throw new Exception("Unable to save File.");
            }
            finally
            {
                if (File.Exists(SettingsFilePath + ".backup")) File.Delete(SettingsFilePath + ".backup");   // remove backup file
            }
        }

        /// <summary>
        /// Load settings from xml
        /// </summary>
        public void LoadSettings()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(PasswordMangerSettings));

            try
            {
                StreamReader reader = new StreamReader(SettingsFilePath);
                PasswordMangerSettings settings = (PasswordMangerSettings)serializer.Deserialize(reader);

                PrimarySavePath = settings.PrimaryPath;
                SecondarySavePaths.Clear();
                foreach(string path in settings.SecondaryPaths) SecondarySavePaths.Add(path);

                reader.Close();
            }
            catch
            {
                throw new Exception("Unable to load settings. Configuration file corrupted.");
            }
        }

        /// <summary>
        /// Save data to encrypted binary file.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <param name="encryptionPassword"></param>
        public void SaveEncrypted(string path, List<CredentialEntry> data, SecureString encryptionPassword)
        {
            SaveData saveData = new SaveData();   // prepare data for serialization
            saveData.CredentialEntries = new List<SerializeableCredentialEntry>();
            foreach (CredentialEntry entry in data) saveData.CredentialEntries.Add(entry.ToSerializeable());   // convert entries

            BinaryFormatter serializer = new BinaryFormatter();

            if (File.Exists(path)) File.Copy(path, path + ".backup", true);

            try
            {
                // Create memory stream
                MemoryStream memoryStream = new MemoryStream();

                // Create a new StreamWriter
                FileStream writer = File.OpenWrite(path);

                // Serialize the file
                serializer.Serialize(memoryStream, saveData);
                memoryStream.Seek(0, SeekOrigin.Begin);
                byte[] bytes = new byte[memoryStream.Length];
                memoryStream.Read(bytes, 0, (int)memoryStream.Length);
                byte[] encryptedBytes = AESEncryption.EncryptWithPassword(bytes, encryptionPassword);

                writer.Write(encryptedBytes, 0, encryptedBytes.Length);

                // Close the writer
                writer.Close();
            }
            catch
            {
                if (File.Exists(path)) File.Delete(path);   // rollback if something failed
                if (File.Exists(path + ".backup")) File.Move(path + ".backup", path);
                throw new Exception("Unable to save File.");
            }
            finally
            {
                if (File.Exists(path + ".backup")) File.Delete(path + ".backup");   // remove backup file
            }
        }

        /// <summary>
        /// Load binary encrypted data from a given file.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="decryptionPassword"></param>
        /// <returns></returns>
        public List<CredentialEntry> OpenEncrypted(string path, SecureString decryptionPassword)
        {
            // create binary formatter
            BinaryFormatter formatter = new BinaryFormatter();

            // Deserialize the file
            SaveData deserialized;
            
            byte[] encryptedBytes = File.ReadAllBytes(path);
            byte[] decryptionBytes = AESEncryption.DecryptWithPassword(encryptedBytes, decryptionPassword);

            if(decryptionBytes == null) throw new Exception("Wrong Password.");

            try
            {
                MemoryStream memoryStream = new MemoryStream(decryptionBytes, 0, decryptionBytes.Length);
                deserialized = (SaveData)formatter.Deserialize(memoryStream);
            }
            catch
            {
                throw new Exception("Unable to open file.");
            }

            List<CredentialEntry> collection = new List<CredentialEntry>();   // fill in loaded data
            foreach (SerializeableCredentialEntry deserializedEntry in deserialized.CredentialEntries)
            {
                CredentialEntry entry = new CredentialEntry(decryptionPassword, deserializedEntry.Password);
                entry.Title = deserializedEntry.Title;
                entry.Id = deserializedEntry.Id;
                entry.UserName = deserializedEntry.UserName;
                entry.ProvidedName = deserializedEntry.ProvidedName;
                entry.AddressData = deserializedEntry.AddressData;
                entry.Url = deserializedEntry.Url;
                entry.Email = deserializedEntry.Email;

                collection.Add(entry);
            }            

            // Return the object
            return collection;
        }

        #region Singleton
        private FileDirector()
        {
            PrimarySavePath = SaveFilePath;
            SecondarySavePaths = new List<string>();
        }

        public static FileDirector Instance
        {
            get
            {
                if (_instance == null) _instance = new FileDirector();
                return _instance;
            }
        }
        private static FileDirector _instance;
        #endregion
    }
}
