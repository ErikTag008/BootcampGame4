using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using System.Runtime.CompilerServices;


namespace Project
{
    public static class EUtils
    {
        private static Camera _camera;

        public static Camera Cam
        {
            get
            {
                if (_camera == null)
                    _camera = Camera.main;
                return _camera;
            }
        }
        public static void MoveTextThenDestroy(this TMP_Text text, Vector3 relativeMove, float duration = 2f, bool fade = true, Ease ease = Ease.InOutSine)
        {
            if (fade)
            {
                text.transform.DOMove(relativeMove, duration).SetRelative(true).SetEase(ease);
                // Fix: Use canvasGroup.DOFade or CrossFadeAlpha for TMP_Text
                // TMP_Text inherits from Graphic, so use CrossFadeAlpha
                text.CrossFadeAlpha(0f, duration, false);
                DOVirtual.DelayedCall(duration, () => Object.Destroy(text.gameObject));
            }
            else
            {
                text.transform.DOMove(relativeMove, duration).SetRelative(true).SetEase(ease)
                    .OnComplete(() => Object.Destroy(text.gameObject));
            }
        }
        public static void SnapToGrid(this Transform transform, float gridSize) =>
                transform.position = Math.SnapToGrid(transform.position, gridSize);
        public static void SetUIElementWorldPosition(this RectTransform element, Vector3 worldPos, Canvas canvas)
        {
            if (canvas == null || element == null || Cam == null) return;

            Vector2 screenPos = Cam.WorldToScreenPoint(worldPos);

            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                element.position = screenPos;
            else if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, screenPos, Cam, out Vector2 local))
                element.anchoredPosition = local;
        }

        public static void DeleteChildren(this Transform t)
        {
            foreach (Transform c in t)
                Object.Destroy(c.gameObject);
        }

        public static float ToPercent(this float percentage) => percentage * 0.01f;

        public static class Math
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float Lerp(float a, float b, float speed = 3f) => b + (a - b) * Mathf.Exp(-speed * Time.deltaTime);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Vector2 Lerp(Vector2 a, Vector2 b, float speed = 3f) => b + (a - b) * Mathf.Exp(-speed * Time.deltaTime);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Vector3 Lerp(Vector3 a, Vector3 b, float speed = 3f) => b + (a - b) * Mathf.Exp(-speed * Time.deltaTime);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Quaternion Slerp(Quaternion a, Quaternion b, float speed = 3f) =>
                Quaternion.Slerp(a, b, 1f - Mathf.Exp(-speed * Time.deltaTime));
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float LerpRounded(float a, float b, float speed = 3f, float roundingMagnitude = 0.02f) => b - a > roundingMagnitude ? b + (a - b) * Mathf.Exp(-speed * Time.deltaTime) : b;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Vector2 LerpRounded(Vector2 a, Vector2 b, float speed = 3f, float roundingMagnitude = 0.02f) => b.magnitude - a.magnitude > roundingMagnitude ? b + (a - b) * Mathf.Exp(-speed * Time.deltaTime) : b;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Vector3 LerpRounded(Vector3 a, Vector3 b, float speed = 3f, float roundingMagnitude = 0.02f) => b + (a - b) * Mathf.Exp(-speed * Time.deltaTime);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Quaternion SlerpRounded(Quaternion a, Quaternion b, float speed = 3f, float roundingMagnitude = 0.02f) =>
                Quaternion.Angle(a, b) < roundingMagnitude ? b : Quaternion.Slerp(a, b, 1f - Mathf.Exp(-speed * Time.deltaTime));

            public static float Percent(float percentage) => percentage * 0.01f;
            public static float Percent(int percentage) => percentage * 0.01f;

            public static float Remap(float iMin, float iMax, float oMin, float oMax, float v) =>
                Mathf.Lerp(oMin, oMax, Mathf.InverseLerp(iMin, iMax, v));

            public static Vector2 Remap(float iMin, float iMax, Vector2 oMin, Vector2 oMax, float v) =>
                Vector2.Lerp(oMin, oMax, Mathf.InverseLerp(iMin, iMax, v));

            public static Vector3 Remap(float iMin, float iMax, Vector3 oMin, Vector3 oMax, float v) =>
                Vector3.Lerp(oMin, oMax, Mathf.InverseLerp(iMin, iMax, v));

            public static float SnapTo(float value, float snapSize) => Mathf.Round(value / snapSize) * snapSize;

            public static Vector3 SnapToGrid(Vector3 position, float gridSize) =>
                new(
                    Mathf.Round(position.x / gridSize) * gridSize,
                    Mathf.Round(position.y / gridSize) * gridSize,
                    Mathf.Round(position.z / gridSize) * gridSize
                );
            public static float GetAverage(params float[] nums)
            {
                if(nums.Length == 0) return 0;
                float sum = 0;
                foreach( var v in nums)
                {
                    sum += v;
                }
                return sum / nums.Length;
            }

        }

        public static class Random
        {
            public static Vector2 Vector2(float max) => new Vector2(UnityEngine.Random.Range(-max, max), UnityEngine.Random.Range(-max, max));
            public static Vector3 Vector3(float max) => new Vector3(UnityEngine.Random.Range(-max, max), UnityEngine.Random.Range(-max, max), UnityEngine.Random.Range(-max, max));

            public static Color ColorFullAlpha() => new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1);
            public static Color ColorRandomAlpha() => new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
            public static Color ColorVibrant() => Color.HSVToRGB(UnityEngine.Random.value, 1f, 1f);
        }

        public static class UI
        {
            private static readonly List<RaycastResult> _results = new List<RaycastResult>(10);


            public static bool IsOverUI()
            {
                if (EventSystem.current == null)
                    return false;

                Vector2 screenPosition;

                if (Mouse.current != null)
                    screenPosition = Mouse.current.position.ReadValue();
                else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
                    screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                else
                    return false;

                var eventData = new PointerEventData(EventSystem.current)
                {
                    position = screenPosition
                };

                EventSystem.current.RaycastAll(eventData, _results);
                return _results.Count > 0;
            }

            public static Vector2 GetWorldPositionOfCanvasElement(RectTransform element)
            {
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Cam, element.position);
                RectTransformUtility.ScreenPointToWorldPointInRectangle(element, screenPoint, Cam, out var result);
                return result;
            }


        }

        public static class Timer
        {
            public struct Simple
            {
                private float _duration;
                private float _timeRemaining;

                public readonly bool IsRunning => _timeRemaining > 0f;
                public readonly bool IsDone => _timeRemaining <= 0f;
                public readonly float TimeRemaining => Mathf.Max(0f, _timeRemaining);
                public readonly float Progress => 1f - Mathf.Clamp01(_timeRemaining / _duration);

                public Simple(float duration)
                {
                    _duration = duration;
                    _timeRemaining = duration;
                }

                public void Restart(float newDuration = -1f)
                {
                    if (newDuration > 0f) _duration = newDuration;
                    _timeRemaining = _duration;
                }

                public void Stop() => _timeRemaining = 0f;
                public void Tick(float deltaTime) { if (_timeRemaining > 0f) _timeRemaining -= deltaTime; }
            }
        }

        public static class WorldUI
        {
            public static TMP_Text CreateWorldText(string text, Vector3 position, Transform parent = null, int fontSize = 24, Color? color = null)
            {
                GameObject obj = new GameObject("World_Text", typeof(TextMeshPro));
                obj.transform.SetParent(parent, false);
                obj.transform.position = position;

                TextMeshPro tmp = obj.GetComponent<TextMeshPro>();
                tmp.text = text;
                tmp.fontSize = fontSize;
                tmp.color = color ?? Color.white;
                return tmp;
            }


        }

        public static class ObjectUtils
        {
            public static void SafeDestroy(Object obj)
            {
                if (obj != null)
                    Object.Destroy(obj);
            }
        }

        public static class Scene
        {
            public static async UniTask LoadSceneWithLoadingScreen(string targetScene, string loadingScene)
            {
                await SceneManager.LoadSceneAsync(loadingScene, LoadSceneMode.Additive);

                var loadOp = SceneManager.LoadSceneAsync(targetScene);
                loadOp.allowSceneActivation = false;

                await UniTask.WaitUntil(() => loadOp.progress >= 0.9f);

                loadOp.allowSceneActivation = true;
                await loadOp.ToUniTask();
                if (SceneManager.GetSceneByName(loadingScene).isLoaded)
                {
                    await SceneManager.UnloadSceneAsync(loadingScene);
                }
            }
            public static async UniTask LoadSceneWithLoadingScreen(int targetScene, string loadingScene, float loadingMinDuration = 1f)
            {
                await SceneManager.LoadSceneAsync(loadingScene, LoadSceneMode.Additive);

                var loadOp = SceneManager.LoadSceneAsync(targetScene);
                loadOp.allowSceneActivation = false;

                await UniTask.WhenAll(
                    UniTask.WaitUntil(() => loadOp.progress >= 0.9f),
                    UniTask.WaitForSeconds(loadingMinDuration)
                    );

                loadOp.allowSceneActivation = true;
                await loadOp.ToUniTask();
                if (SceneManager.GetSceneByName(loadingScene).isLoaded)
                {
                    await SceneManager.UnloadSceneAsync(loadingScene);
                }
            }
        }

        public static class Logger
        {
            public static string Colorize(string text, string colorHexOrName)
            {
                return $"<color={colorHexOrName}>{text}</color>";
            }
        }
    }
}

