using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;

namespace PotatoBot.Services
{
    public class LanguageService
    {
        private static string _activeLanguagePath => Path.Combine(Directory.GetCurrentDirectory(), "Language", "active.json");

        private readonly Random _random = new();
        private readonly dynamic _language;

        public LanguageService()
        {
            if(!File.Exists(_activeLanguagePath))
            {
                throw new FileNotFoundException($"No language file found! ({_activeLanguagePath})");
            }

            var json = File.ReadAllText(_activeLanguagePath);
            _language = JsonConvert.DeserializeObject<dynamic>(json)!;
            if(_language == null)
            {
                throw new InvalidDataException("Invalid language file");
            }
        }

        internal string GetTranslation(params string[] path)
        {
            if(path == null || path.Length == 0)
            {
                throw new ArgumentNullException(nameof(path));
            }

            var currentDict = _language[path[0]];
            foreach(var key in path.Skip(1))
            {
                try
                {
                    currentDict = currentDict[key];
                }
                catch(RuntimeBinderException)
                {
                    return "INVALID TRANSLATION REQUESTED. MAYOR FUCKUP BY DEVELOPER";
                }
            }

            if(currentDict is JValue)
            {
                return (string)currentDict;
            }

            if(currentDict is not JArray list)
            {
                return "INVALID TRANSLATION REQUESTED. MAYOR FUCKUP BY DEVELOPER";
            }
            return (string)list[_random.Next(list.Count)]!;
        }
    }
}
