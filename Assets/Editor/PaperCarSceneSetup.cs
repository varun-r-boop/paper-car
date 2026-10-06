#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PaperCar
{
    public static class PaperCarSceneSetup
    {
        const string ScenePath = "Assets/Scenes/PaperCar.unity";

        [MenuItem("Paper Car/Setup Scene")]
        public static void SetupScene()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            PaperCarWorldBuilder.Build();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            Debug.Log("Paper Car scene saved to " + ScenePath);
        }
    }
}
#endif
