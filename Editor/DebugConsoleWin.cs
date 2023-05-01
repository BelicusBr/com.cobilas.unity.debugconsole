using System.IO;
using UnityEditor;
using UnityEngine;
using Cobilas.Collections;
using Cobilas.Unity.Utility;
using System.Collections.Generic;
using Cobilas.Unity.Utility.Console;

namespace Cobilas.Unity.Editor.UtilityConsole {
    public class DebugConsoleWin : EditorWindow {
        [MenuItem("Window/Debug Console")]
        private static void DoIt() {
            DebugConsoleWin debug = GetWindow<DebugConsoleWin>();
            debug.titleContent = new GUIContent("Debug Console");

            debug.OnEnable();
            debug.Show();
        }

        private bool showInfo;
        private bool showWarn;
        private GUIStyle label;
        private bool showError;
        private string contains;
        private GUIStyle toolbar;
        private GUIStyle foldout;
        private int selectedIndex;
        private Vector2 scrollView;
        private Texture2D iconInfo;
        private Texture2D iconWarn;
        private Texture2D iconError;
        private string[] nameModules;
        private DebugLogger[] current;

        private static string ConfigFile => UnityPath.Combine(DebugConsole.DebugConsoleFolder, "Config.txt");

        private void OnEnable() {
            LoadConfig();
            label = (GUIStyle)null;
            toolbar = (GUIStyle)null;
            foldout = (GUIStyle)null;
            iconInfo = EditorGUIUtility.IconContent("console.infoicon").image as Texture2D;
            iconWarn = EditorGUIUtility.IconContent("console.warnicon").image as Texture2D;
            iconError = EditorGUIUtility.IconContent("console.erroricon").image as Texture2D;
            Dictionary<string, DebugLogger[]> keys = DebugConsole.Logs;
            nameModules = new string[keys.Count];
            current = (DebugLogger[])null;
            int count = 0;
            foreach (var item in keys)
                nameModules[count++] = item.Key;
        }

        private void OnDestroy()
            => UnloadConfig();

        private void LoadConfig() {
            if (File.Exists(ConfigFile)) {
                using (StreamReader stream = new StreamReader(ConfigFile)) {
                    byte count = 0;
                    while (!stream.EndOfStream) {
                        switch (count) {
                            case 0: showInfo = bool.Parse(stream.ReadLine()); break;
                            case 1: showWarn = bool.Parse(stream.ReadLine()); break;
                            case 2: showError = bool.Parse(stream.ReadLine()); break;
                        }
                        ++count;
                        if (count > 2) break;
                    }
                }
            } else {
                showError = showInfo = showWarn = true;
                UnloadConfig();
            }
        }

        private void UnloadConfig() {
            if (!Directory.Exists(DebugConsole.DebugConsoleFolder))
                Directory.CreateDirectory(DebugConsole.DebugConsoleFolder);
            using (StreamWriter writer = new StreamWriter(File.Create(ConfigFile))) {
                writer.WriteLine(showInfo);
                writer.WriteLine(showWarn);
                writer.WriteLine(showError);
            }
        }

        private void OnGUI() {
            if (!ArrayManipulation.EmpytArray(nameModules))
                current = DebugConsole.Logs[nameModules[selectedIndex]];

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            if (ToolBarButton("Refresh", 55))
                OnEnable();
            EditorGUI.BeginDisabledGroup(ArrayManipulation.EmpytArray(nameModules));
            if (EditorGUILayout.DropdownButton(EditorGUIUtility.TrTempContent("Clear"), FocusType.Passive,
                EditorStyles.toolbarDropDown, GUILayout.Width(55f))) {

                GenericMenu generic = new GenericMenu();
                generic.AddItem(new GUIContent("Clear module"), false, ClearModule);
                generic.AddItem(new GUIContent("Clear all module"), false, ClearAllModules);
                generic.ShowAsContext();
            }
            ToolBarPopup();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();
            contains = EditorGUILayout.TextField(contains, EditorStyles.toolbarSearchField);
            EditorGUI.BeginChangeCheck();
            showInfo = DrawToogle(showInfo, LogType.Log);
            showWarn = DrawToogle(showWarn, LogType.Warning);
            showError = DrawToogle(showError, LogType.Error);
            if (EditorGUI.EndChangeCheck())
                UnloadConfig();
            EditorGUILayout.EndHorizontal();
            scrollView = EditorGUILayout.BeginScrollView(scrollView);
            for (int I = 0; I < ArrayManipulation.ArrayLength(current); I++) {
                if (!ShowPrint(current[I]) || !ContainsPrint(current[I])) continue;
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                current[I].foldout = EditorGUILayout.Foldout(current[I].foldout, 
                    FoldoutText(current[I]), CreateFoldout());
                EditorGUILayout.EndHorizontal();
                if (current[I].foldout) {
                    ++EditorGUI.indentLevel;
                    DrawLabel(current[I]);
                    --EditorGUI.indentLevel;
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void ClearModule() {
            DebugConsole.SetModule(nameModules[selectedIndex]);
            DebugConsole.ClearModule();
            ArrayManipulation.ClearArraySafe(ref current);
        }

        private void ClearAllModules() {
            DebugConsole.ClearAllModules();
            ArrayManipulation.ClearArraySafe(ref current);
            ArrayManipulation.ClearArraySafe(ref nameModules);
            nameModules = new string[0];
        }

        private bool ContainsPrint(DebugLogger logger) {
            if (string.IsNullOrEmpty(contains)) return true;
            return logger.MSM.Contains(contains);
        }

        private bool ShowPrint(DebugLogger logger) {
            switch (logger.Type) {
                case LogType.Warning:
                    return showWarn;
                case LogType.Assert:
                case LogType.Log:
                    return showInfo;
                case LogType.Error:
                case LogType.Exception:
                    return showError;
                default: return false;
            }
        }

        private GUIContent FoldoutText(DebugLogger logger) {
            GUIContent temp = EditorGUIUtility.TrTempContent(string.Format("[{0}]{1}",
                ColoredText(logger.LogTypeColor, PickUpALine(logger.Time.ToString())),
                ColoredText(logger.LogTypeColor, PickUpALine(logger.MSM)))
                );
            switch (logger.Type) {
                case LogType.Warning:
                    temp.image = iconWarn;
                    break;
                case LogType.Assert:
                case LogType.Log:
                    temp.image = iconInfo;
                    break;
                case LogType.Error:
                case LogType.Exception:
                    temp.image = iconError;
                    break;
            }
            return temp;
        }

        private bool DrawToogle(bool value, LogType type) {
            GUIContent temp = EditorGUIUtility.TrTempContent("Show");
            switch (type) {
                case LogType.Warning:
                    temp.image = iconWarn;
                    break;
                case LogType.Assert:
                case LogType.Log:
                    temp.image = iconInfo;
                    break;
                case LogType.Error:
                case LogType.Exception:
                    temp.image = iconError;
                    break;
            }
            return GUILayout.Toggle(value, temp, EditorStyles.toolbarButton, GUILayout.Width(65f));
        }

        private GUIContent LabelMSMText(DebugLogger logger) {
            GUIContent temp = EditorGUIUtility.TrTempContent(
                ColoredText(logger.LogTypeColor, logger.MSM.TrimEnd())
                );
            switch (logger.Type) {
                case LogType.Warning:
                    temp.image = iconWarn;
                    break;
                case LogType.Assert:
                case LogType.Log:
                    temp.image = iconInfo;
                    break;
                case LogType.Error:
                case LogType.Exception:
                    temp.image = iconError;
                    break;
            }
            return temp;
        }

        private void DrawLabel(DebugLogger logger) {
            EditorGUILayout.BeginVertical(CreateToolBar(), GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField(LabelMSMText(logger), CreateLabel());
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical(CreateToolBar(), GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField(ColoredText(logger.LogTypeColor, logger.Tracking), CreateLabel());
            EditorGUILayout.EndVertical();
        }

        private string PickUpALine(string msm)
            => !string.IsNullOrEmpty(msm) && msm.Contains("\n") ?
            string.Format("{0}...", msm.Remove(msm.IndexOf('\n'))) : msm;

        private GUIStyle CreateToolBar() {
            if (toolbar != null) return toolbar;
            toolbar = new GUIStyle(EditorStyles.toolbar);
            toolbar.fixedHeight = 0f;
            return toolbar;
        }

        //Método para usar futuramente
        //private string ColoredText(DebugLogger logger)
        //    => ColoredText(logger.LogTypeColor, logger.MSM.TrimEnd());

        private string ColoredText(Color color, string txt)
            => string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGBA(color), txt);

        private GUIStyle CreateFoldout() {
            if (foldout != null) return foldout;
            foldout = new GUIStyle(EditorStyles.foldout);
            foldout.richText = true;
            return foldout;
        }

        private GUIStyle CreateLabel() {
            if (label != null) return label;
            label = new GUIStyle(EditorStyles.label);
            label.wordWrap =
            label.richText = true;
            return label;
        }

        private bool ToolBarButton(string txt, float width)
            => GUILayout.Button(txt, EditorStyles.toolbarButton, GUILayout.Width(width));

        private void ToolBarPopup() {
            EditorGUI.BeginChangeCheck();
            selectedIndex = EditorGUILayout.Popup(selectedIndex, nameModules, EditorStyles.toolbarPopup, GUILayout.Width(130f));
            if (EditorGUI.EndChangeCheck() && !ArrayManipulation.EmpytArray(nameModules))
                current = DebugConsole.Logs[nameModules[selectedIndex]];
        }
    }
}