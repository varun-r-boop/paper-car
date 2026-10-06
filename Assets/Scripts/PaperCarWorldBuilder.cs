using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PaperCar
{
    /// <summary>
    /// Builds the playable prototype at runtime (or from the editor menu) so the
    /// scene does not depend on hand-authored YAML.
    /// </summary>
    public static class PaperCarWorldBuilder
    {
        const float FloorHalf = 6f;
        const float KnockoutHalf = 5.0f;
        const float WallThickness = 0.4f;
        const float WallHeight = 0.8f;

        public static MatchController Build()
        {
            var existing = Find<MatchController>();
            if (existing != null)
                return existing;

            SetupCamera();
            SetupLight();

            var root = new GameObject("PaperCar");
            var arenaGo = CreateArena(root.transform);
            var bounds = arenaGo.GetComponent<ArenaBounds>();

            CarController p1 = CreateCar(root.transform, "Player1", new Vector3(-2.2f, 0.32f, 0f), new Color(0.85f, 0.25f, 0.2f));
            CarController p2 = CreateCar(root.transform, "Player2", new Vector3(2.2f, 0.32f, 0f), new Color(0.2f, 0.45f, 0.9f));
            AimIndicator aim1 = p1.GetComponentInChildren<AimIndicator>(true);
            AimIndicator aim2 = p2.GetComponentInChildren<AimIndicator>(true);

            var systems = new GameObject("Systems");
            systems.transform.SetParent(root.transform);
            var mic = systems.AddComponent<MicrophoneBlowDetector>();
            var thrust = systems.AddComponent<LocalThrustInput>();
            thrust.SetMicrophone(mic);

            var match = systems.AddComponent<MatchController>();
            match.Configure(p1, p2, aim1, aim2, bounds, thrust, mic);

            CreateUi(root.transform, match);
            return match;
        }

        static void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                cam = go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
                go.tag = "MainCamera";
            }

            cam.orthographic = true;
            cam.orthographicSize = 8f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.14f, 0.18f);
            cam.transform.SetPositionAndRotation(new Vector3(0f, 18f, 0f), Quaternion.Euler(90f, 0f, 0f));
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 40f;
        }

        static void SetupLight()
        {
            if (Find<Light>() != null)
                return;

            var go = new GameObject("Directional Light");
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        static GameObject CreateArena(Transform parent)
        {
            var arena = new GameObject("Arena");
            arena.transform.SetParent(parent);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(arena.transform);
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(FloorHalf * 2f, 0.2f, FloorHalf * 2f);
            SetColor(floor, new Color(0.82f, 0.78f, 0.7f));

            var bounds = arena.AddComponent<ArenaBounds>();
            bounds.HalfExtents = new Vector2(KnockoutHalf, KnockoutHalf);

            CreateWall(arena.transform, "WallNorth", new Vector3(0f, WallHeight * 0.5f, FloorHalf + WallThickness * 0.5f), new Vector3(FloorHalf * 2f + WallThickness * 2f, WallHeight, WallThickness));
            CreateWall(arena.transform, "WallSouth", new Vector3(0f, WallHeight * 0.5f, -(FloorHalf + WallThickness * 0.5f)), new Vector3(FloorHalf * 2f + WallThickness * 2f, WallHeight, WallThickness));
            CreateWall(arena.transform, "WallEast", new Vector3(FloorHalf + WallThickness * 0.5f, WallHeight * 0.5f, 0f), new Vector3(WallThickness, WallHeight, FloorHalf * 2f));
            CreateWall(arena.transform, "WallWest", new Vector3(-(FloorHalf + WallThickness * 0.5f), WallHeight * 0.5f, 0f), new Vector3(WallThickness, WallHeight, FloorHalf * 2f));

            return arena;
        }

        static void CreateWall(Transform parent, string name, Vector3 position, Vector3 scale)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent);
            wall.transform.position = position;
            wall.transform.localScale = scale;
            SetColor(wall, new Color(0.35f, 0.32f, 0.28f));
        }

        static CarController CreateCar(Transform parent, string name, Vector3 position, Color color)
        {
            var car = new GameObject(name);
            car.transform.SetParent(parent);
            car.transform.position = position;

            var box = car.AddComponent<BoxCollider>();
            box.size = new Vector3(1.1f, 0.4f, 0.7f);
            ApplyPhysicsMaterial(box);

            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Body";
            DestroyCollider(visual);
            visual.transform.SetParent(car.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(1.1f, 0.4f, 0.7f);
            SetColor(visual, color);

            var rb = car.AddComponent<Rigidbody>();
            rb.mass = 1.2f;
            rb.useGravity = true;
#if UNITY_6000_0_OR_NEWER
            rb.linearDamping = 1.6f;
            rb.angularDamping = 2.5f;
#else
            rb.drag = 1.6f;
            rb.angularDrag = 2.5f;
#endif

            var controller = car.AddComponent<CarController>();

            var aimPivot = new GameObject("Aim");
            aimPivot.transform.SetParent(car.transform);
            aimPivot.transform.localPosition = Vector3.zero;
            aimPivot.transform.localRotation = Quaternion.identity;
            var aim = aimPivot.AddComponent<AimIndicator>();

            var arrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arrow.name = "Arrow";
            DestroyCollider(arrow);
            arrow.transform.SetParent(aimPivot.transform);
            arrow.transform.localPosition = new Vector3(0f, 0.45f, 1.15f);
            arrow.transform.localScale = new Vector3(0.12f, 0.08f, 1.5f);
            SetColor(arrow, Color.white);
            aim.SetArrow(arrow.transform);
            aim.SetActiveAim(false);

            return controller;
        }

        static void ApplyPhysicsMaterial(Collider collider)
        {
#if UNITY_6000_0_OR_NEWER
            var phys = new PhysicsMaterial
            {
                name = "PaperCar",
                dynamicFriction = 0.35f,
                staticFriction = 0.4f,
                bounciness = 0.2f,
                frictionCombine = PhysicsMaterialCombine.Average,
                bounceCombine = PhysicsMaterialCombine.Average
            };
#else
            var phys = new PhysicMaterial("PaperCar")
            {
                dynamicFriction = 0.35f,
                staticFriction = 0.4f,
                bounciness = 0.2f
            };
#endif
            collider.material = phys;
        }

        static void CreateUi(Transform parent, MatchController match)
        {
            var canvasGo = new GameObject("Canvas");
            canvasGo.transform.SetParent(parent);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasGo.AddComponent<GraphicRaycaster>();

            if (Find<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.transform.SetParent(parent);
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                        ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            Text p1 = CreateText(canvasGo.transform, "P1", new Vector2(40, -30), new Vector2(400, 60), TextAnchor.UpperLeft, 28);
            Text p2 = CreateText(canvasGo.transform, "P2", new Vector2(-40, -30), new Vector2(400, 60), TextAnchor.UpperRight, 28);
            p2.rectTransform.anchorMin = new Vector2(1f, 1f);
            p2.rectTransform.anchorMax = new Vector2(1f, 1f);
            p2.rectTransform.pivot = new Vector2(1f, 1f);
            Text state = CreateText(canvasGo.transform, "State", new Vector2(0, -30), new Vector2(900, 60), TextAnchor.UpperCenter, 24);
            state.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            state.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            state.rectTransform.pivot = new Vector2(0.5f, 1f);

            Text result = CreateText(canvasGo.transform, "Result", Vector2.zero, new Vector2(800, 160), TextAnchor.MiddleCenter, 40);
            result.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            result.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            result.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            result.gameObject.SetActive(false);

            var buttonGo = new GameObject("Restart");
            buttonGo.transform.SetParent(canvasGo.transform, false);
            var buttonRt = buttonGo.AddComponent<RectTransform>();
            buttonRt.anchorMin = new Vector2(0.5f, 0.35f);
            buttonRt.anchorMax = new Vector2(0.5f, 0.35f);
            buttonRt.pivot = new Vector2(0.5f, 0.5f);
            buttonRt.sizeDelta = new Vector2(220, 56);
            var image = buttonGo.AddComponent<Image>();
            image.color = new Color(0.15f, 0.6f, 0.35f);
            var button = buttonGo.AddComponent<Button>();
            Text buttonText = CreateText(buttonGo.transform, "RestartLabel", Vector2.zero, new Vector2(220, 56), TextAnchor.MiddleCenter, 26);
            buttonText.text = "Restart";
            buttonText.color = Color.white;
            buttonGo.SetActive(false);

            if (font != null)
            {
                p1.font = font;
                p2.font = font;
                state.font = font;
                result.font = font;
                buttonText.font = font;
            }

            p1.text = "Player 1";
            p2.text = "Player 2";
            p1.color = new Color(0.95f, 0.45f, 0.4f);
            p2.color = new Color(0.45f, 0.65f, 1f);
            state.color = Color.white;
            result.color = Color.white;

            var ui = canvasGo.AddComponent<MatchUI>();
            ui.Configure(match, p1, p2, state, result, button);
        }

        static Text CreateText(Transform parent, string name, Vector2 anchoredPos, Vector2 size, TextAnchor align, int fontSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            var text = go.AddComponent<Text>();
            text.alignment = align;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        static T Find<T>() where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType<T>();
#else
            return Object.FindObjectOfType<T>();
#endif
        }

        static void DestroyCollider(GameObject go)
        {
            var col = go.GetComponent<Collider>();
            if (col == null)
                return;
            if (Application.isPlaying)
                Object.Destroy(col);
            else
                Object.DestroyImmediate(col);
        }

        static void SetColor(GameObject go, Color color)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer == null)
                return;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                            ?? Shader.Find("Universal Render Pipeline/Unlit")
                            ?? Shader.Find("Standard")
                            ?? Shader.Find("Unlit/Color")
                            ?? Shader.Find("Sprites/Default");
            if (shader == null)
                return;

            var material = new Material(shader);
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);
            renderer.material = material;
        }
    }
}
