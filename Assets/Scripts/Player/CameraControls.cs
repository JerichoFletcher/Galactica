using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

namespace Galactica.Player {
    public class CameraControls : MonoBehaviour {
        [Header("Camera"), SerializeField] private new Camera camera;
        [SerializeField] private float cameraMoveSpeed, cameraMoveZoomBoost, maxDistanceFromGC;
        [SerializeField] private Vector2 cameraZoomBounds;
        [SerializeField] private float cameraZoomStep;

        [Header("Audio"), SerializeField] private Transform audioListener;
        [SerializeField] private Vector2 audioListenerBounds;
        [SerializeField] private AudioMixer audioMixer;

        private Vector2 cameraMove;
        private float scroll;
        private float zoom;

        public void InputOnMove(InputAction.CallbackContext ctx) {
            cameraMove = ctx.ReadValue<Vector2>();
        }

        public void InputOnZoom(InputAction.CallbackContext ctx) {
            scroll = ctx.ReadValue<float>();
        }

        private void Awake() {
            zoom = Mathf.InverseLerp(cameraZoomBounds.x, cameraZoomBounds.y, camera.orthographicSize);
        }

        private void Update() {
            Vector2 currPos = transform.position;
            float moveZoomBoost = Mathf.Lerp(0f, cameraMoveZoomBoost, zoom);

            Vector2 newPos = currPos + (cameraMoveSpeed + moveZoomBoost) * Time.unscaledDeltaTime * cameraMove;
            newPos = Vector2.ClampMagnitude(newPos, maxDistanceFromGC);
            transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
        }

        private void LateUpdate() {
            // Set camera size
            zoom = Mathf.Clamp01(zoom + cameraZoomStep * -scroll);
            camera.orthographicSize = Mathf.Lerp(cameraZoomBounds.x, cameraZoomBounds.y, zoom);

            // Set audio listener position
            Vector3 audioPos = audioListener.position;
            audioPos.z = Mathf.Lerp(audioListenerBounds.x, audioListenerBounds.y, zoom);
            audioListener.position = audioPos;

            // Mix reverb
            float reverbWet = 10000f * zoom * zoom;
            audioMixer.SetFloat("ReverbDry", -reverbWet);
            audioMixer.SetFloat("ReverbWet", -10000f + reverbWet);
        }
    }
}
