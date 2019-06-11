using System;
using UnityEngine;
using System.IO.Ports;
using System.IO;
using UnityEngine.Events;
using UnityEngine.UI;
using Utility;
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace VR_Measurements {
	public class SensorData : MonoBehaviour {
		[Header("Port Settings")]
		[SerializeField] private string _port = "COM3";
		private SerialPort _serialPort;
		private string _serialData = "";

		[Header("Recording Settings")]
		[SerializeField] private bool _recordAtStart = false;

		private LogWriter _sensorDataWriter;
		private bool _recording = false;
		[SerializeField] private Text _debugConsole;

		[SerializeField] private UnityEvent onSensorError;

		private bool Init() {
			try {
				_serialPort = new SerialPort(_port, 9600, Parity.None, 8, StopBits.One) {
																							Handshake = Handshake.None,
																							RtsEnable = true
																						};
				_serialPort.Open();

				_debugConsole.text = ">> Port " + _port + " is reading";
				return true;
			} catch (IOException) {
				onSensorError?.Invoke();
				_debugConsole.text = ">> Port: " + _port + " is not available";
				return false;
			}
		}

		private void Start() {
			if (_recordAtStart) {
				StartRecording();
			}
		}

		void Update() {
			if (_recording) {
				if (!_serialPort.IsOpen) return;
				// get new data
				ReadPort();
			}
		}

		public void SetPort(Text input) {
			_port = input.text;
		}

		private void ReadPort() {
			try {
				// write data to file
				_sensorDataWriter.Write(_serialPort.ReadExisting());
			} catch (FormatException) {
				Debug.LogError("Serial port data was in an unexpected format. \n" +
							   "This is normal on startup but shouldn't occur while the program is running.");
			} catch (IOException) {
				_debugConsole.text = ">> Connection to the Port was lost!";
				onSensorError?.Invoke();
				_serialPort.Close();
				StopRecording();
			} catch (TimeoutException) { 
				// silence the timeout
			} catch (Exception e) {
				Debug.LogError(e.GetBaseException());
			}
		}

		public void ToggleRecording() {
			if (_recording) {
				StopRecording();
			} else {
				StartRecording();
			}
		}

		public void StartRecording() {
			_recording = Init();
			if (_recording) {
				_sensorDataWriter = new LogWriter("SensorData_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), true);
				ReadPort();
			}
		}

		public void StopRecording() {
			_recording = false;
			_sensorDataWriter?.CloseWriter();

			OnApplicationQuit();
		}

		private void OnApplicationQuit() {
			if (_serialPort.IsOpen) {
				_serialPort.Close();
			}
		}
	}
}