using System;
using System.IO;
using System.Text;
using UnityEngine;
using System.Reflection;
using Cobilas.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using Cobilas.Unity.Utility;
using System.Runtime.Serialization.Formatters.Binary;
#endif

namespace Cobilas.Unity.UtilityConsole {
    public static class DebugConsole {

        private static Dictionary<string, DebugLogger[]> logs = new Dictionary<string, DebugLogger[]>();
        private static string targetModule;

        public static Dictionary<string, DebugLogger[]> Logs => logs;
#if UNITY_EDITOR
        public static string DebugConsoleFolder => UnityPath.Combine(UnityPath.PersistentDataPath, "DebugConsole");
        public static string DebugConsoleFile => UnityPath.Combine(DebugConsoleFolder, "DebugConsole.log");

        private static float timer;
        [InitializeOnLoadMethod]
        private static void Init() {
            string filePath = DebugConsoleFile;
            if (!Directory.Exists(DebugConsoleFolder))
                Directory.CreateDirectory(DebugConsoleFolder);
            if (File.Exists(filePath)) {
                try {
                    FileStream temp;
                    logs = (Dictionary<string, DebugLogger[]>)new BinaryFormatter().Deserialize(temp = File.OpenRead(filePath));
                    temp.Dispose();
                } catch { }
            } else File.Create(filePath).Dispose();

            EditorApplication.update += () => {
                timer += Time.unscaledDeltaTime;
                if (((int)timer % 50) == 0) {
                    timer = 0;
                    filePath = DebugConsoleFile;
                    FileStream temp;
                    new BinaryFormatter().Serialize(temp = File.Create(filePath), logs);
                    temp.Dispose();
                }
            };
        }
#endif

        /// <summary><seealso cref="Debug"/>.Log(message);</summary>
        public static void Log(object message)
            => Debug.Log(message);

        public static void Log(IFormatProvider provider, string format, params object[] msm)
            => Log(string.Format(provider, format, msm));

        public static void Log(string format, params object[] msm)
            => Log(string.Format(format, msm));

        /// <summary><seealso cref="Debug"/>.LogError(message);</summary>
        public static void LogError(object message)
            => Debug.LogError(message);

        /// <summary><seealso cref="Debug"/>.LogErrorFormat(format, msm);</summary>
        public static void LogError(string format, params object[] msm)
            => Debug.LogErrorFormat(format, msm);

        /// <summary><seealso cref="Debug"/>.LogException(message);</summary>
        public static void LogException(Exception message)
            => Debug.LogException(message);

        /// <summary><seealso cref="Debug"/>.LogWarning(message);</summary>
        public static void LogWarning(object message)
            => Debug.LogWarning(message);

        /// <summary><seealso cref="Debug"/>.LogWarningFormat(format, msm);</summary>
        public static void LogWarning(string format, params object[] msm)
            => Debug.LogWarningFormat(format, msm);

        public static void ClearDebugLog() {
#if UNITY_EDITOR
            try {
                Assembly assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
                Type type = assembly.GetType("UnityEditor.LogEntries");
                MethodInfo method = type.GetMethod("Clear");
                _ = method.Invoke((object)null, (object[])null);
            } catch (Exception E) {
                Debug.LogError($"O metodo não e compativel com a versão:{Application.unityVersion} da unity.\n" +
                    $"{E}");
            }
#endif        
        }

        public static void PrintToFile()
            => PrintToFile(UnityPath.Combine(UnityPath.StreamingAssetsPath, "logger"));

        public static void PrintToFile(string folder) {
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            foreach (var item in logs)
                using (StreamWriter writer = File.CreateText(Path.Combine(folder, string.Format("{0}.txt", item.Key))))
                    for (int I = 0; I < ArrayManipulation.ArrayLength(item.Value); I++) {
                        DebugLogger logger = item.Value[I];
                        writer.WriteLine("[{0}]", logger.Time);
                        writer.WriteLine("LogType:{0}", logger.Type.ToString());
                        writer.WriteLine("[Message]\r\n{0}", logger.MSM.TrimEnd());
                        writer.WriteLine("[Tracking]\r\n{0}", logger.Tracking.TrimEnd());
                        writer.WriteLine("//{0}", string.Empty.PadRight(40, '='));
                    }
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        public static void SetModule(string name)
            => targetModule = name;

        public static void ConsoleLog(object message)
            => SetLog(LogType.Log, message.ToString());

        public static void ConsoleLog(string format, params object[] args)
            => SetLog(LogType.Log, string.Format(format, args));

        public static void ConsoleLogException(Exception message)
            => SetLog(LogType.Exception, message.ToString());

        public static void ConsoleLogError(object message)
            => SetLog(LogType.Error, message.ToString());

        public static void ConsoleLogError(string format, params object[] args)
            => SetLog(LogType.Error, string.Format(format, args));

        public static void ConsoleLogWarning(object message)
            => SetLog(LogType.Warning, message.ToString());

        public static void ConsoleLogWarning(string format, params object[] args)
            => SetLog(LogType.Warning, string.Format(format, args));

        public static void ClearModule() {
            targetModule = string.IsNullOrEmpty(targetModule) ? "Generic" : targetModule;
            logs[targetModule] = new DebugLogger[0];
        }

        public static void ClearAllModules()
            => logs.Clear();

        private static void SetLog(LogType type, string msm) {
            KeyValuePair<string, DebugLogger[]> temp = GetLogger();
            logs[targetModule] = ArrayManipulation.Add(new DebugLogger(
                type, msm,
                MethodTrackingList(3)
                ), temp.Value);
        }

        private static KeyValuePair<string, DebugLogger[]> GetLogger() {
            targetModule = string.IsNullOrEmpty(targetModule) ? "Generic" : targetModule;
            if (!logs.ContainsKey(targetModule))
                logs.Add(targetModule, new DebugLogger[0]);
            return new KeyValuePair<string, DebugLogger[]>(targetModule, logs[targetModule]);
        }

        private static string MethodTrackingList(int startIndex) {
            System.Diagnostics.StackFrame[] frames = TrackMethod();
            StringBuilder builder = new StringBuilder();
            for (int I = startIndex; I < ArrayManipulation.ArrayLength(frames); I++) {
                builder.AppendFormat("File name: {0} (C:{1} L:{2}) Method: {3}\r\n",
                    frames[I].GetFileName(), frames[I].GetFileLineNumber(),
                    frames[I].GetFileLineNumber(), frames[I].GetMethod());
            }
            return builder.ToString();
        }

        private static System.Diagnostics.StackFrame[] TrackMethod()
            => new System.Diagnostics.StackTrace(true).GetFrames();
    }
}
