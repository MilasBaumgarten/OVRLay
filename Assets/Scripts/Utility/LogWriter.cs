using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Utility {
    public class LogWriter {
        private readonly string filePath;
        private StreamWriter writer;
        private bool appending;
        private Queue<string> stringQueue;
        private Task writingTask;
        public bool IsRunning { get; private set; }

        public enum FileWriteMode {
            Append = 0,
            Overwrite = 1
        }

        public LogWriter(string fileName, bool append) {
            if (!Directory.Exists(Application.dataPath + "/TrackingFiles")) {
                Directory.CreateDirectory(Application.dataPath + "/TrackingFiles");
            }

            filePath = Application.dataPath + "/TrackingFiles/" + fileName + ".txt";
            writer = new StreamWriter(filePath, append);
            appending = append;
            stringQueue = new Queue<string>();
            writingTask = Task.Run(StartWriting);
        }

        public void ChangeWriteMode(FileWriteMode mode) {
            switch (mode) {
                case FileWriteMode.Append:
                    if (!appending) {
                        appending = true;
                        CloseWriter();
                        writer = new StreamWriter(filePath, appending);
                    }
                    break;
                case FileWriteMode.Overwrite:
                    if (appending) {
                        appending = false;
                        CloseWriter();
                        writer = new StreamWriter(filePath, appending);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        public void Write(string data) {
            stringQueue.Enqueue(data);
        }
        
        /// <summary>
        /// Write the given string into a file
        /// </summary>
        private async Task StartWriting() {
            IsRunning = true;
            while (IsRunning) {
                while (stringQueue.Count > 0) {
                    await writer.WriteAsync(stringQueue.Dequeue());
                }
            }
        }

        private async void StopWriting() {
            if (writingTask.IsCanceled) return;
            IsRunning = false;
            await writingTask;
        }

        public void CloseWriter() {
            StopWriting();
            writer.Flush();
            writer.Close();
        }
    }
}