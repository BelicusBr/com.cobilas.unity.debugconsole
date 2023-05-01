using System;
using UnityEngine;

namespace Cobilas.Unity.Utility.Console {
    [Serializable]
    public sealed class DebugLogger {
        public bool foldout;
        private readonly LogType type;
        private readonly string msm;
        private readonly string tracking;
        private readonly DateTime time;

        public string MSM => msm;
        public LogType Type => type;
        public DateTime Time => time;
        public string Tracking => tracking;
        public Color LogTypeColor => GetColor();

        public DebugLogger(LogType type, string msm, string tracking) {
            time = DateTime.Now;
            this.type = type;
            this.msm = msm;
            this.tracking = tracking;
        }

        private Color GetColor() {
            switch (type) {
                case LogType.Warning: return Color.yellow;
                case LogType.Error:
                case LogType.Exception: return Color.red;
                default: return Color.white;
            }
        }
    }
}
