using System.Collections.Generic;
using UnityEngine;
// ReSharper disable FieldCanBeMadeReadOnly.Local

public class SensorVisualization : MonoBehaviour {
	[SerializeField] private LineRenderer _edaLine = null;
	[SerializeField] private LineRenderer _ecgLine = null;

	private Queue<float> _edaData = new Queue<float>();
	private Queue<float> _ecgData = new Queue<float>();

	[Header("Visualization")]
	[SerializeField] private Vector2 _bounds = new Vector2(6, 3);
	[SerializeField] private float _maxValue = 1000f;
	[SerializeField] private Vector2 _offset = new Vector2(-3, -0.6f);

	void Start() {
		InitializeGraph(_edaLine);
		InitializeGraph(_ecgLine);
	}

	void Update() {
		// keep Graph in sync by always showing all new points
		while (_edaData.Count > 0) {
			UpdateGraph(_edaData.Dequeue(), _edaLine);
		}

		while (_ecgData.Count > 0) {
			UpdateGraph(_ecgData.Dequeue(), _ecgLine);
		}
	}

	private void UpdateGraph(float value, LineRenderer line) {
		// add new point on Graph
		// TODO: change maxValue on the fly
		var newPos = new Vector2(_bounds.x, value * _bounds.y / _maxValue) + _offset;
		Utility.Visualization.ShiftLineRendererPositions(line, newPos);
	}

	// spread the points on the line out to match the bounds(all on y = 0)
	private void InitializeGraph(LineRenderer line) {
		for (var i = 0; i < line.positionCount; i++) {
			line.SetPosition(i, new Vector2((i / (float)line.positionCount) * _bounds.x + _offset.x, 0));
		}
	}

	private void OnDrawGizmosSelected() {
		Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		Gizmos.DrawCube(new Vector3(_bounds.x * 0.5f + _offset.x, _bounds.y * 0.5f + _offset.y, 0.0f), _bounds);
	}
}
