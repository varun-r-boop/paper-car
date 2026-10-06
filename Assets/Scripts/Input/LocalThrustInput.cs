using UnityEngine;

namespace PaperCar
{
    /// <summary>
    /// Combines the shared microphone with a Space-bar fallback for silent testing.
    /// </summary>
    public class LocalThrustInput : MonoBehaviour, IThrustInput
    {
        [SerializeField] MicrophoneBlowDetector microphone;
        [SerializeField] KeyCode debugBlowKey = KeyCode.Space;
        [SerializeField] float debugBlowIntensity = 0.85f;

        public float Intensity => microphone != null ? microphone.Intensity : 0f;

        public void SetMicrophone(MicrophoneBlowDetector detector)
        {
            microphone = detector;
        }

        public bool TryGetThrust(out float intensity)
        {
            if (Input.GetKeyDown(debugBlowKey))
            {
                intensity = debugBlowIntensity;
                return true;
            }

            if (microphone != null && microphone.TryConsumeBlow(out intensity) && intensity > 0f)
                return true;

            intensity = 0f;
            return false;
        }
    }
}
