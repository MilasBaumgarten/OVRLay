using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Utility;
using Valve.VR;

namespace VR_Measurements {
	public class ReplayData : MonoBehaviour {
		private Coroutine replay;

		[SerializeField] private string fileName = "";
		[SerializeField] private TextAsset textAsset = null;
		[SerializeField] private bool replayAtStart = false;

		[Header("Objects used for visualizing the recorded movement")] [SerializeField]
		private Transform[] visualizationObjects;

		[SerializeField] private GameObject defaultVisualizationPrefab;

		public enum ReadFileMode {
			TextAsset = 0,
			FilePath = 1
		}

		private void Start() {
			if (replayAtStart && textAsset) {
				StartReplayAsync(ReadFileMode.TextAsset);
			}
		}

		// Update is called once per frame
		private void Update() {
			//if (!SteamVR_Actions._default.GrabGrip.GetStateDown(SteamVR_Input_Sources.LeftHand) ||
			//    !SteamVR_Actions._default.GrabGrip.GetStateDown(SteamVR_Input_Sources.RightHand)) return;
			//Debug.Log("Playing");

			//if (replay != null) {
			//    StopReplay();
			//}

			//StartReplayAsync(ReadFileMode.FilePath);
		}

		public async void StartReplayAsync(ReadFileMode readFileMode) {
			if (replay != null) {
				StopReplay();
				return;
			}

			switch (readFileMode) {
				case ReadFileMode.TextAsset:
					replay = StartCoroutine(Replay(await PrepareData(await LogReader.ReadLogAsync(textAsset))));
					break;
				case ReadFileMode.FilePath:
					replay = StartCoroutine(Replay(await PrepareData(await LogReader.ReadLogAsync(fileName))));
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(readFileMode), readFileMode, null);
			}
		}

		private void StopReplay() {
			StopCoroutine(replay);
		}

		private async Task<(float[], (Vector3, Quaternion)[][])> PrepareData((string info, string content) data) {
			var (info, content) = data;
			var numTrackedObjects = info.Split('\t').Length - 1;
			var numVisualizationObjects = visualizationObjects.Length;

			if (numVisualizationObjects != numTrackedObjects) {
				Array.Resize(ref visualizationObjects, numTrackedObjects);

				for (var i = 0; i < numTrackedObjects - numVisualizationObjects; i++) {
					visualizationObjects[i + numVisualizationObjects] =
						Instantiate(defaultVisualizationPrefab).transform;
				}
			}

			var lines = content.Split('\n');
			var numLines = lines.Length;
			var numColumns = lines[0].Split('\t').Length;

			var timeDeltas = new float[numLines];
			var trackingDatas = new (Vector3, Quaternion)[numTrackedObjects][];

			for (var i = 0; i < numTrackedObjects; i++) {
				trackingDatas[i] = new (Vector3, Quaternion)[numLines];
			}


			var lineTasks = new Task[numLines];
			for (var i = 0; i < numLines; i++) {
				var i1 = i;

				lineTasks[i] = Task.Run(async () => {
					var columns = lines[i1].Split('\t');
					timeDeltas[i1] = float.Parse(columns[0]);
					var columnTasks = new Task[numColumns - 1];
					for (var j = 0; j < numTrackedObjects; j++) {
						var j1 = j;
						columnTasks[j] = Task.Run(() => {
							var floats = columns[j1 + 1].Split(' ');
							trackingDatas[j1][i1] = (
								new Vector3(float.Parse(floats[0]), float.Parse(floats[1]), float.Parse(floats[2])),
								new Quaternion(float.Parse(floats[3]), float.Parse(floats[4]), float.Parse(floats[5]),
									float.Parse(floats[6])));
						});
					}

					await Task.WhenAll(columnTasks);
				});
			}

			await Task.WhenAll(lineTasks);
			return (timeDeltas, trackingDatas);
		}

		private IEnumerator Replay((float[], (Vector3, Quaternion)[][]) preparedData) {
			print(3);
			var (timeDeltas, trackingDatas) = preparedData;
			var stopWatch = new System.Diagnostics.Stopwatch();
			stopWatch.Start();
			for (var i = 0; i < timeDeltas.Length - 1; i++) {
				while (timeDeltas[i] > stopWatch.ElapsedMilliseconds) yield return null;

				while (timeDeltas[i + 1] < stopWatch.ElapsedMilliseconds) i++;

				for (var j = 0; j < visualizationObjects.Length; j++)
					visualizationObjects[j]
						.SetPositionAndRotation(trackingDatas[j][i].Item1, trackingDatas[j][i].Item2);
			}
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(ReplayData))]
	public class ReplayDataEditor : Editor {
		public override void OnInspectorGUI() {
			var replayData = target as ReplayData;

			DrawDefaultInspector();

			if (GUILayout.Button("Start Replay")) {
				replayData?.StartReplayAsync(ReplayData.ReadFileMode.TextAsset);
			}
		}
	}
#endif
}