# Plain Password Manager
A simple password manager for storing passwords encrypted.

## Introduction
This Application lets you save your passwords in a save way. All data you enter will be AES-256 (Advanced Encryption Standard) encrypted which at the moment has no known practical attack (https://en.wikipedia.org/wiki/Advanced_Encryption_Standard#Known_attacks). This Application does not communicate with any webservices so that the user at all times controls were his most important data is being stored.

## Usage
On the main screen you can add new password entries with the 'add' button or by pressing 'CTRL+A'. Once a new entry was added you can enter you information and save it by either clicking on 'save' or by pressing 'CTRL+S'. To enter a password click on the 'change' button next to it. By default a password won't be displayed, you have to check the 'show' checkbox to make it visible. The application tries to immediately remove the password from the main memory once it is hidden to protect it from malware.

## Sorting and search
You can sort your list by expanding the sorting menu and choose a property you want to sort your password data by. You can also expand the search menu to search for a specific credential entry.

## Primary Paths
When you open the 'settings' window you will see a 'primary file' field. The primary file is the password storage file which keeps your data in encrypted form. Only the Plain Password Manager is able to decrypt this file given you entered the correct master password at the start of the application. If you click on 'set file' you can choose a new location for the data to be stored. The old file will be deleted once you close the settings window and all data will be moved to the new file.

## Secondary Paths
When you open the 'settings' window you will see a list with all 'secondary paths'. This list will be empty if you have started the application for the first time. You can add new paths to this list. No data will be loaded from the files you specify in this list but every time you save your data it will also be saved to all files you have specified here. They serve as a kind of backup in case you loose your main file so keep them at a seperate hard drive if possible.

## Import
When you open the 'settings' window you will see a button called 'Import...' at the top right. This button can be used if you want to import an existing password storage file rather than using the current primary file. CAUTION: Once you import a password storage file you will be asked to enter the password of this new file. All data of the old file will be lost! The application will make a backup copy of the old file which you can import if you want to recover the data at some point.

## Change Master Password
You can at any time change the master password of your password storage. The application will then decrypt and re-encrypt all your data with the new password.
