using UnityEngine;

namespace PaperCar
{
    /// <summary>
    /// If the scene is empty, assemble the prototype when Play starts.
    /// </summary>
    public static class PaperCarBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
#if UNITY_2023_1_OR_NEWER
            bool missing = Object.FindFirstObjectByType<MatchController>() == null;
#else
            bool missing = Object.FindObjectOfType<MatchController>() == null;
#endif
            if (missing)
                PaperCarWorldBuilder.Build();
        }
    }
}
