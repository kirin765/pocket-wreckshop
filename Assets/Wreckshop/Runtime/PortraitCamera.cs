using UnityEngine;

namespace PocketWreckshop
{
    [ExecuteAlways, RequireComponent(typeof(Camera))]
    public sealed class PortraitCamera : MonoBehaviour
    {
        Camera view;
        void OnEnable() { view = GetComponent<Camera>(); Apply(); }
        void Update() { Apply(); }
        void Apply()
        {
            if (!view || Screen.height <= 0 || Screen.width <= 0) return;
            // Fit the 9:16 game inside the physical safe area, including Android cutouts.
            Rect safe = Application.isPlaying ? Screen.safeArea : new Rect(0, 0, Screen.width, Screen.height);
            if (safe.width <= 0 || safe.height <= 0) return;
            float target = 9f / 16f, aspect = safe.width / safe.height;
            float w = safe.width, h = safe.height;
            if (aspect > target) w = h * target; else h = w / target;
            view.rect = new Rect((safe.x + (safe.width - w) / 2f) / Screen.width,
                (safe.y + (safe.height - h) / 2f) / Screen.height, w / Screen.width, h / Screen.height);
            view.orthographicSize = 4f;
        }
    }
}
