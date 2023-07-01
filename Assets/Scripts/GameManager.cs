using UnityEngine;
using UnityEngine.InputSystem;

namespace Galactica {
    public class GameManager : MonoBehaviour {
        [SerializeField] private float timescaleLow, timescaleMid, timescaleHigh;

        private Timescale _Timescale {
            get {
                return _tscl;
            }
            set {
                _tscl = value;
                switch(value) {
                    case Timescale.Low:
                        Time.timeScale = timescaleLow;
                        Debug.Log($"Game speed: x{timescaleLow}");
                        break;
                    case Timescale.Mid:
                        Time.timeScale = timescaleMid;
                        Debug.Log($"Game speed: x{timescaleMid}");
                        break;
                    case Timescale.High:
                        Time.timeScale = timescaleHigh;
                        Debug.Log($"Game speed: x{timescaleHigh}");
                        break;
                }
            }
        }
        private Timescale _tscl;

        private bool _Paused {
            get {
                return _paused;
            }
            set {
                _paused = value;
                if(value) {
                    Time.timeScale = 0f;
                    Debug.Log($"Game paused");
                } else {
                    _Timescale = _Timescale;
                }
            }
        }
        private bool _paused;

        public void InputOnGameSpeedPause(InputAction.CallbackContext ctx) {
            if(ctx.performed) {
                _Paused = !_Paused;
            }
        }

        public void InputOnGameSpeedLow(InputAction.CallbackContext ctx) {
            if(ctx.performed) {
                _Timescale = Timescale.Low;
            }
        }

        public void InputOnGameSpeedMid(InputAction.CallbackContext ctx) {
            if(ctx.performed) {
                _Timescale = Timescale.Mid;
            }
        }

        public void InputOnGameSpeedHigh(InputAction.CallbackContext ctx) {
            if(ctx.performed) {
                _Timescale = Timescale.High;
            }
        }

        private void Start() {
            _Timescale = Timescale.Low;
        }

        private enum Timescale {
            Low, Mid, High
        }
    }
}
