using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Utility;

namespace VR_Measurements {
	public class TrackingData : MonoBehaviour {
		private LogWriter trackingDataWriter;
		private bool recording;
		private System.Diagnostics.Stopwatch stopwatch;
		private StringBuilder stringBuilder;
		private long lastRecordedFrameTime;

		[SerializeField] private float fixedReplayFps = 60;
		[SerializeField] private bool recordAtStart = false;
		[SerializeField] private Transform[] trackedObjects;

		[SerializeField] private UnityEvent _onStartRecording;
		[SerializeField] private UnityEvent _onStopRecording;

		private void Start() {
			if (recordAtStart) {
				StartRecording();
			}
		}

		private void Update() {
			if (recording) {
				TrackData();
			}
			// toggle recording
			if (Input.GetKeyDown(KeyCode.Space)) {
				ToggleRecording();
			}
		}

		private void OnApplicationQuit() {
			if (trackingDataWriter == null) return;
			if (trackingDataWriter.IsRunning) {
				StopRecording();
			}
		}

		public void StartRecording() {
			recording = true;
			stopwatch = new System.Diagnostics.Stopwatch();
			stopwatch.Start();
			trackingDataWriter = new LogWriter("TrackingData_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), true);
			stringBuilder = new StringBuilder();
			stringBuilder.Append("timeDeltas");
			foreach (var trackedObject in trackedObjects) {
				stringBuilder.Append("\t" + trackedObject.name);
			}

			lastRecordedFrameTime = stopwatch.ElapsedMilliseconds;
			TrackData();

			_onStartRecording?.Invoke();
		}

		public void StopRecording() {
			recording = false;
			trackingDataWriter?.CloseWriter();

			_onStopRecording?.Invoke();
		}

		public void ToggleRecording() {
			if (recording) {
				StopRecording();
			} else {
				StartRecording();
			}
		}

		private void TrackData() {
			if (!(1 / fixedReplayFps < stopwatch.ElapsedMilliseconds - lastRecordedFrameTime)) return;
			stringBuilder.Append("\n" + (stopwatch.ElapsedMilliseconds));

			foreach (var trackedObject in trackedObjects) {
				stringBuilder.Append("\t" + trackedObject.position.x.ToString("0.000") + " " + trackedObject.position.y.ToString("0.000") + " " +
									 trackedObject.position.z.ToString("0.000") + " " + trackedObject.rotation.x.ToString("0.000") + " " +
									 trackedObject.rotation.y.ToString("0.000") + " " + trackedObject.rotation.z.ToString("0.000") + " " +
									 trackedObject.rotation.w.ToString("0.000"));
			}

			trackingDataWriter.Write(stringBuilder.ToString());
			stringBuilder.Clear();

			lastRecordedFrameTime = stopwatch.ElapsedMilliseconds;
		}
	}

	[CustomEditor(typeof(TrackingData))]
	public class TrackingDataEditor : Editor {
		public override void OnInspectorGUI() {
			var replayData = target as TrackingData;

			DrawDefaultInspector();

			if (GUILayout.Button("Toggle Recording")) {
				replayData?.ToggleRecording();
			}
		}
	}
}