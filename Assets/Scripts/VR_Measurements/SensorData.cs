using System;
using UnityEngine;
using System.IO.Ports;
using System.IO;
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

		void Start() {
			try {
				_serialPort = new SerialPort(_port, 9600, Parity.None, 8, StopBits.One) {
																							Handshake = Handshake.None,
																							RtsEnable = true
																						};
				_serialPort.Open();

				Debug.Log("Port " + _port + " is reading");
			} catch (IOException) {
				Debug.LogError("Port: " + _port + " is not available");
			}

			if (_recordAtStart) {
				StartRecording();
			}
		}

		void Update() {
			if (!_serialPort.IsOpen) return;

			if (_recording) {
				// get new data
				ReadPort();
			}
		}

		private void ReadPort() {
			try {
				// write data to file
				_sensorDataWriter.Write(_serialPort.ReadExisting());
			} catch (FormatException) {
				Debug.LogError("Serial port data was in an unexpected format. \n" +
							   "This is normal on startup but shouldn't occur while the program is running.");
			} catch (IOException) {
				Debug.LogError("Connection to the Port was lost!");
				_serialPort.Close();
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
			_recording = true;
			_sensorDataWriter = new LogWriter("SensorData_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), true);
			ReadPort();
		}

		public void StopRecording() {
			_recording = false;
			_sensorDataWriter?.CloseWriter();
		}

		private void OnApplicationQuit() {
			if (_serialPort.IsOpen) {
				_serialPort.Close();
			}
		}
	}
}