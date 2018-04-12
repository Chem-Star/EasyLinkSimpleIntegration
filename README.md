# Easy-Link API - Simple Integration

This is a small C# program to demonstrate how one might integrate with the ChemStar Easy-Link API.

It is given an 'OrderFolder' to watch. Underneath the root 'OrderFolder' it creates two sub-folders, one for 'Inbound' and one for 'Outbound' orders. Beneath each of those two sub-folders it creates an additional two sub-folders, one for 'NEW' payloads and one for 'UPDATE' payloads. When a json file is dropped into one of the 'NEW'/'UPDATE' folders, the program will try to submit the payload to ChemStar through the API, if the payload is successful it will be moved to a 'Processed' sub-folder, otherwise it will be moved to an 'Errored' sub-folder.

To get started, clone or download the repository.
Then set up your credentials in the App.config file. You should have received all the authorization credentials from your Rinchem contact.
You should also set up and point to a folder that will 'host' the integration. Only the 'root' folder is necessary, all sub-folders will be generated automatically.
