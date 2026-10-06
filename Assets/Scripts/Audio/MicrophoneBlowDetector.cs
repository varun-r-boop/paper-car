using UnityEngine;

namespace PaperCar
{
    /// <summary>
    /// Loops the default microphone and maps RMS loudness to a 0–1 blow intensity.
    /// Has no knowledge of cars or match state.
    /// </summary>
    public class MicrophoneBlowDetector : MonoBehaviour
    {
        [SerializeField] float noiseGate = 0.04f;
        [SerializeField] float maxRms = 0.25f;
        [SerializeField] int sampleRate = 44100;
        [SerializeField] int clipSeconds = 1;
        [SerializeField] int rmsWindow = 1024;

        AudioClip _clip;
        string _device;
        float[] _samples;
        int _lastPosition;
        bool _wasAboveGate;
        bool _pendingBlow;
        float _pendingIntensity;

        public bool Listening { get; set; } = true;
        public float Intensity { get; private set; }
        public bool IsRecording => _clip != null;

        void Awake()
        {
            _samples = new float[Mathf.Max(256, rmsWindow)];
        }

        void OnEnable()
        {
            StartMic();
        }

        void OnDisable()
        {
            StopMic();
        }

        void Update()
        {
            Intensity = ReadRmsMapped();
            bool above = Intensity > 0f;
            if (Listening && above && !_wasAboveGate)
            {
                _pendingBlow = true;
                _pendingIntensity = Intensity;
            }

            if (!Listening)
                _pendingBlow = false;

            _wasAboveGate = above;
        }

        public bool TryConsumeBlow(out float intensity)
        {
            if (_pendingBlow)
            {
                _pendingBlow = false;
                intensity = _pendingIntensity;
                return true;
            }

            intensity = 0f;
            return false;
        }

        void StartMic()
        {
            StopMic();
            if (Microphone.devices == null || Microphone.devices.Length == 0)
            {
                Debug.LogWarning("Paper Car: no microphone found. Use Space to simulate a blow.");
                return;
            }

            _device = Microphone.devices[0];
            _clip = Microphone.Start(_device, true, clipSeconds, sampleRate);
            _lastPosition = 0;
            _wasAboveGate = false;
        }

        void StopMic()
        {
            if (!string.IsNullOrEmpty(_device) && Microphone.IsRecording(_device))
                Microphone.End(_device);
            _clip = null;
            _device = null;
            Intensity = 0f;
        }

        float ReadRmsMapped()
        {
            if (_clip == null || string.IsNullOrEmpty(_device))
                return 0f;

            int position = Microphone.GetPosition(_device);
            if (position < 0 || position == _lastPosition)
                return Intensity;

            int length = _samples.Length;
            int start = position - length;
            if (start < 0)
                start += _clip.samples;

            _clip.GetData(_samples, start);
            _lastPosition = position;

            double sum = 0d;
            for (int i = 0; i < _samples.Length; i++)
            {
                float s = _samples[i];
                sum += s * s;
            }

            float rms = Mathf.Sqrt((float)(sum / _samples.Length));
            if (rms < noiseGate)
                return 0f;

            return Mathf.Clamp01((rms - noiseGate) / Mathf.Max(0.0001f, maxRms - noiseGate));
        }
    }
}
