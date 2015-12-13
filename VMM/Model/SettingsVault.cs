using System;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Formatters.Binary;
using FirstFloor.ModernUI.Presentation;

namespace VMM.Model
{
    internal static class SettingsVault
    {
        private const string StoragePath = "vault.bin";

        internal static Settings Read()
        {
            try
            {
                using(var storage = IsolatedStorageFile.GetStore(IsolatedStorageScope.Assembly | IsolatedStorageScope.User, null, null))
                {
                    if(storage.FileExists(StoragePath))
                    {
                        using(var stream = storage.OpenFile(StoragePath, FileMode.Open, FileAccess.Read))
                        {
                            var reader = new BinaryFormatter();

                            try
                            {
                                return (Settings)reader.Deserialize(stream);
                            }
                            catch(Exception e)
                            {
                                Trace.WriteLine($"Unable to deserialize vault {stream.Name}. {e}");
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Trace.WriteLine($"Unable to open vault: {e}");
            }


            return new Settings();
        }

        internal static void Write(Settings settings)
        {
            try
            {
                using(var storage = IsolatedStorageFile.GetStore(IsolatedStorageScope.Assembly | IsolatedStorageScope.User, null, null))
                {
                    using(var stream = storage.OpenFile(StoragePath, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        var reader = new BinaryFormatter();

                        try
                        {
                            reader.Serialize(stream, settings);
                        }
                        catch(Exception e)
                        {
                            Trace.WriteLine($"Unable to serialize vault {stream.Name}. {e}");
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Trace.WriteLine($"Unable to open vault: {e}");
            }
        }
    }

    [Serializable]
    internal class Settings
    {
        public string Token { get; set; }
        public long UserId { get; set; }
        public bool ReadOnly { get; set; }

        public Uri Theme { get; set; }
        public string AccentColor { get; set; }
        public FontSize FontSize { get; set; }
    }
}