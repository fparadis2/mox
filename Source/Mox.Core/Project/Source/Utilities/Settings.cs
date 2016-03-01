using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Mox
{
    public interface ISettingsBackend
    {
        bool TryRead(Type type, out object value);
        void Save(Type type, object value);
    }

    public static class Settings
    {
        #region Variables

        private static readonly Dictionary<Type, object> ms_settings = new Dictionary<Type, object>();

        #endregion

        #region Methods

        public static T Get<T>()
            where T : class, new()
        {
            Type type = typeof (T);

            object value;
            if (!ms_settings.TryGetValue(type, out value))
            {
                if (!Backend.TryRead(type, out value))
                {
                    value = new T();
                }

                ms_settings.Add(type, value);
            }
            return (T) value;
        }

        public static void Save<T>()
        {
            Type type = typeof (T);

            object value;
            if (ms_settings.TryGetValue(type, out value))
            {
                Backend.Save(type, value);
            }
        }

        #endregion

        #region Back end

        private static ISettingsBackend ms_backend = new DefaultBackend();

        public static ISettingsBackend Backend
        {
            get { return ms_backend; }
            set
            {
                if (ms_settings.Count != 0)
                    throw new InvalidOperationException("Cannot change the backend once settings have been loaded");

                ms_backend = value;
            }
        }

        public static void UseFileBackend()
        {
            Backend = new FileBackend();
        }

        #endregion

        #region Inner Types

        private class DefaultBackend : ISettingsBackend
        {
            public bool TryRead(Type type, out object value)
            {
                value = null;
                return false;
            }

            public void Save(Type type, object value)
            {
            }
        }

        private class FileBackend : ISettingsBackend
        {
            private static string SettingsDirectory
            {
                get
                {
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mox", ".settings");
                }
            }

            public FileBackend()
            {
                Directory.CreateDirectory(SettingsDirectory);
            }

            public bool TryRead(Type type, out object value)
            {
                string filename = GetFilename(type);
                if (!File.Exists(filename))
                {
                    value = null;
                    return false;
                }

                try
                {
                    JsonSerializer serializer = JsonSerializer.CreateDefault();
                    using (var reader = File.OpenText(filename))
                    using (JsonTextReader jsonTextReader = new JsonTextReader(reader))
                    {
                        value = serializer.Deserialize(jsonTextReader, type);
                        return true;
                    }
                }
                catch (IOException)
                {
                    value = null;
                    return false;
                }
            }

            public void Save(Type type, object value)
            {
                string filename = GetFilename(type);

                try
                {
                    JsonSerializer serializer = JsonSerializer.CreateDefault();

                    using (var writer = File.CreateText(filename))
                    using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
                    {
                        serializer.Serialize(jsonWriter, value);
                    }
                }
                catch (IOException)
                {
                }
            }

            private static string GetFilename(Type type)
            {
                return Path.Combine(SettingsDirectory, type.Name + ".json");
            }
        }

        #endregion
    }
}
