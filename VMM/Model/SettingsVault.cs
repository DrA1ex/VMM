using System;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Formatters.Binary;

namespace VMM.Model
{
    internal static class SettingsVault
    {
        private const string StoragePath = "vault.bin";

        internal static Settings Read()
        {
            try
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetStore(IsolatedStorageScope.Assembly | IsolatedStorageScope.User, null, null))
                {
                    if (storage.FileExists(StoragePath))
                    {
                        using (IsolatedStorageFileStream stream = storage.OpenFile(StoragePath, FileMode.Open, FileAccess.Read))
                        {
                            var reader = new BinaryFormatter();

                            try
                            {
                                return (Settings)reader.Deserialize(stream);
                            }
                            catch (Exception e)
                            {
                                Trace.WriteLine(String.Format("Unable to deserialize vault {0}. {1}", stream.Name, e));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(String.Format("Unable to open vault: {0}", e));
            }


            return new Settings();
        }

        internal static void Write(Settings settings)
        {
            try
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetStore(IsolatedStorageScope.Assembly | IsolatedStorageScope.User, null, null))
                {
                    using (IsolatedStorageFileStream stream = storage.OpenFile(StoragePath, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        var reader = new BinaryFormatter();

                        try
                        {
                            reader.Serialize(stream, settings);
                        }
                        catch (Exception e)
                        {
                            Trace.WriteLine(String.Format("Unable to serialize vault {0}. {1}", stream.Name, e));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(String.Format("Unable to open vault: {0}", e));
            }
        }
    }

    [Serializable]
    internal class Settings
    {
        public string Token { get; set; }
        public long UserId { get; set; }
        public bool ReadOnly { get; set; }
    }
}