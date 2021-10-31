using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;

namespace PotatoBot.Managers
{
    internal class LanguageManager
    {
        private static string ActiveLanguagePath
        {
            get => Path.Combine(Directory.GetCurrentDirectory(), "Language", "active.json");
        }

        private readonly Random _random = new();
        private readonly dynamic _language;

        internal LanguageManager()
        {
            if (!File.Exists(ActiveLanguagePath))
                throw new FileNotFoundException("No language file found! (active.json)");

            var json = File.ReadAllText(ActiveLanguagePath);
            _language = JsonConvert.DeserializeObject<dynamic>(json);
        }

        internal string GetTranslation(params string[] path)
        {
            if (path == null || path.Length == 0)
                throw new ArgumentNullException(nameof(path));

            var currentDict = _language[path[0]];
            foreach (var key in path.Skip(1))
            {
                try
                {
                    currentDict = currentDict[key];
                }
                catch (RuntimeBinderException)
                {
                    return "INVALID TRANSLATION REQUESTED. MAYOR FUCKUP BY DEVELOPER";
                }
            }

            if (currentDict is JValue)
                return (string)currentDict;

            var list = currentDict as JArray;
            return (string)list[_random.Next(list.Count)];
        }
    }
}
