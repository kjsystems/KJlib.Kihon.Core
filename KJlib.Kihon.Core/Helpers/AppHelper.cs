using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using KJlib.Kihon.Core.Extensions;

namespace KJlib.Kihon.Core.Helpers
{
    public class AppHelper
    {
        static string GetHistoryPath(string methodName)
        {
            var appName = Assembly.GetEntryAssembly()?.GetName().Name;
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            return appDataFolder.Combine(appName)
                .CreateDirIfNotExist()
                .Combine($"{methodName}-history.json");
        }

        /// <summary>
        /// ファイルがあれば履歴を読み込み
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static T ReadHistory<T>(string methodName)
        {
            var settingPath = AppHelper.GetHistoryPath(methodName);
            return settingPath.ExistsFile() ? JsonSerializer.Deserialize<T>(System.IO.File.ReadAllText(settingPath, Encoding.UTF8)) : default(T);
        }

        /// <summary>
        /// 履歴を書き込み
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodName"></param>
        /// <param name="value"></param>
        public static void WriteHistory<T>(string methodName, T value)
        {
            var jsonText = JsonSerializer.Serialize<T>(value, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            });
            File.WriteAllText(AppHelper.GetHistoryPath(methodName), jsonText, Encoding.UTF8);
        }
    }
}
