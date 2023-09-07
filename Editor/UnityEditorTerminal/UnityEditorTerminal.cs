using System.IO;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;

namespace Cobilas.Unity.Editor.UtilityConsole.Terminal {
    public class UnityEditorTerminal : EditorWindow {

        [MenuItem("Window/UE-Terminal")]
        private static void Init() {
            UnityEditorTerminal window = GetWindow<UnityEditorTerminal>();
            window.WorkingDirectory = Path.GetDirectoryName(Application.dataPath);
            window.Show();
        }

        private TextEditor textEditor = new TextEditor();
        private string saida;
        private string saida2;
        [SerializeField] private bool setWorkingDirectory;
        [SerializeField] private string WorkingDirectory;

        private void OnEnable() {
            saida2 = (string)(saida = ">").Clone();
        }

        private void OnGUI() {
            EditorGUILayout.BeginHorizontal();
            if (setWorkingDirectory = GUILayout.Toggle(setWorkingDirectory, "Set", GUI.skin.button, GUILayout.Width(50f))) {
                WorkingDirectory = EditorGUILayout.TextField("WorkingDirectory", WorkingDirectory);
                if (string.IsNullOrEmpty(WorkingDirectory))
                    WorkingDirectory = Path.GetDirectoryName(Application.dataPath);
            } else EditorGUILayout.LabelField($"WorkingDirectory: {WorkingDirectory}", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            Rect rect = EditorGUILayout.GetControlRect(true, GUILayout.ExpandHeight(true));
            Event @event = Event.current;

            if (@event.type == EventType.KeyDown)
                if (@event.keyCode == KeyCode.Return) {
                    CallCMD(saida2.Replace(saida, string.Empty));
                    @event.Use();
                }

            saida2 = DrawTextArea(rect, saida2, saida, @event, GUIUtility.GetControlID(FocusType.Keyboard));
        }

        private void CallCMD(string arg) {
            if (string.IsNullOrEmpty(arg))
                return;
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo("cmd.exe", $"/c {arg}");
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput =
                process.StartInfo.RedirectStandardError =
                process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WorkingDirectory = WorkingDirectory;

            process.Start();
            saida2 = (string)(saida = process.StandardOutput.ReadToEnd() + 
                process.StandardError.ReadToEnd() + ">").Clone();
            process.WaitForExit(); // Aguarda o término do processo

            process.Close();
        }

        private string DrawTextArea(Rect rect, string text, string defaulttext, Event @event, int ID) {
            textEditor.text = text;
            textEditor.SaveBackup();
            textEditor.controlID = ID;
            textEditor.position = rect;
            textEditor.style = GUI.skin.textArea;
            textEditor.multiline = true;
            textEditor.isPasswordField = false;
            textEditor.DetectFocusChange();

            bool isHover = rect.Contains(@event.mousePosition);

            switch (@event.type)
            {
                case EventType.MouseUp:
                    if (!isHover)
                    {
                        //isFocused = false;
                        GUIUtility.keyboardControl = 0;
                        textEditor.OnLostFocus();
                    }
                    if (GUIUtility.hotControl == ID)
                    {
                        textEditor.MouseDragSelectsWholeWords(false);
                        GUIUtility.hotControl = 0;
                        @event.Use();
                    }
                    break;
                case EventType.MouseDown:
                    if (!isHover) break;
                    GUIUtility.hotControl =
                        GUIUtility.keyboardControl = ID;
                    if (GUIUtility.keyboardControl == ID)
                    {
                        //isFocused = true;
                        textEditor.OnFocus();
                    }
                    textEditor.MoveCursorToPosition(@event.mousePosition);
                    if (@event.clickCount == 2 && GUI.skin.settings.doubleClickSelectsWord)
                    {
                        textEditor.SelectCurrentWord();
                        textEditor.DblClickSnap(TextEditor.DblClickSnapping.WORDS);
                        textEditor.MouseDragSelectsWholeWords(true);
                    }
                    if (@event.clickCount == 3 && GUI.skin.settings.tripleClickSelectsLine)
                    {
                        textEditor.SelectCurrentParagraph();
                        textEditor.MouseDragSelectsWholeWords(true);
                        textEditor.DblClickSnap(TextEditor.DblClickSnapping.PARAGRAPHS);
                    }
                    @event.Use();
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == ID)
                    {
                        if (@event.shift) textEditor.MoveCursorToPosition(@event.mousePosition);
                        else textEditor.SelectToPosition(@event.mousePosition);
                        @event.Use();
                    }
                    break;
                case EventType.ValidateCommand:
                case EventType.ExecuteCommand:
                    if (GUIUtility.keyboardControl == ID)
                    {
                        if (@event.commandName == "Copy")
                        {
                            textEditor.Copy();
                            @event.Use();
                        }
                        else if (@event.commandName == "Paste")
                        {
                            textEditor.Paste();
                            @event.Use();
                        }
                    }
                    break;
                case EventType.KeyDown:
                    if (GUIUtility.keyboardControl != ID) break;
                    if (textEditor.HandleKeyEvent(@event))
                    {
                        @event.Use();
                        break;
                    }
                    char character = @event.character;
                    if (@event.keyCode == KeyCode.Tab || character == '\t')
                        break;
                    if (character == '\n' && !@event.alt)
                        break;
                    Font font = textEditor.style.font;
                    font = !font ? GUI.skin.font : font;

                    if (font.HasCharacter(character) || character == '\n')
                    {
                        textEditor.Insert(character);
                        break;
                    }
                    //if (character == char.MinValue) {
                    //    textEditor.ReplaceSelection("");
                    //    flag = true;
                    //    @event.Use();
                    //    break;
                    //}

                    switch (@event.keyCode)
                    {
                        case KeyCode.UpArrow:
                            if (@event.shift) textEditor.SelectUp();
                            else textEditor.MoveUp();
                            break;
                        case KeyCode.DownArrow:
                            if (@event.shift) textEditor.SelectDown();
                            else textEditor.MoveDown();
                            break;
                        case KeyCode.LeftArrow:
                            if (@event.shift) textEditor.SelectLeft();
                            else textEditor.MoveLeft();
                            break;
                        case KeyCode.RightArrow:
                            if (@event.shift) textEditor.SelectRight();
                            else textEditor.MoveRight();
                            break;
                        case KeyCode.Home:
                            if (@event.shift) textEditor.SelectGraphicalLineStart();
                            else textEditor.MoveGraphicalLineStart();
                            break;
                        case KeyCode.End:
                            if (@event.shift) textEditor.SelectGraphicalLineEnd();
                            else textEditor.MoveGraphicalLineEnd();
                            break;
                        case KeyCode.PageUp:
                            if (@event.shift) textEditor.SelectTextStart();
                            else textEditor.MoveTextStart();
                            break;
                        case KeyCode.PageDown:
                            if (@event.shift) textEditor.SelectTextEnd();
                            else textEditor.MoveTextEnd();
                            break;
                    }
                    @event.Use();
                    break;
                case EventType.Repaint:
                    if (GUIUtility.keyboardControl != ID)
                        textEditor.style.Draw(rect, EditorGUIUtility.TrTextContent(textEditor.text), ID);
                    else textEditor.DrawCursor(textEditor.text);
                    break;
            }
            if (textEditor.text.Length <= defaulttext.Length) {
                textEditor.cursorIndex =
                    textEditor.selectIndex =
                    defaulttext.Length;
                return defaulttext;
            }
            if (textEditor.cursorIndex < defaulttext.Length)
                textEditor.cursorIndex = defaulttext.Length;
            if (textEditor.selectIndex < defaulttext.Length)
                textEditor.selectIndex = defaulttext.Length;

            textEditor.UpdateScrollOffsetIfNeeded(@event);
            return textEditor.text;
        }
    }
}
