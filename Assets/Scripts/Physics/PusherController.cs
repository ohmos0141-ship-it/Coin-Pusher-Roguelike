using UnityEngine;
using CoinPusher.Core;

namespace CoinPusher.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    public class PusherController : MonoBehaviour
    {
        [SerializeField] GameConfig config;
        [SerializeField] Vector3 pushDirection = Vector3.forward;

        enum State { MovingOut, PausedOut, MovingBack, PausedBack }

        Rigidbody _rb;
        State _state = State.MovingOut;
        float _travelled;
        float _pauseTimer;

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.isKinematic = true;
        }

        void FixedUpdate()
        {
            Vector3 dir = pushDirection.normalized;
            float distance = config.pusherDistance;
            float speed = config.pusherSpeed;
            float pause = config.pusherPauseDuration;
            float step = speed * Time.fixedDeltaTime;

            switch (_state)
            {
                case State.MovingOut:
                {
                    float move = Mathf.Min(step, distance - _travelled);
                    _rb.MovePosition(_rb.position + dir * move);
                    _travelled += move;
                    if (_travelled >= distance - 0.0001f)
                    {
                        _pauseTimer = 0f;
                        _state = State.PausedOut;
                    }
                    break;
                }
                case State.PausedOut:
                    _pauseTimer += Time.fixedDeltaTime;
                    if (_pauseTimer >= pause) _state = State.MovingBack;
                    break;

                case State.MovingBack:
                {
                    float move = Mathf.Min(step, _travelled);
                    _rb.MovePosition(_rb.position - dir * move);
                    _travelled -= move;
                    if (_travelled <= 0.0001f)
                    {
                        _travelled = 0f;
                        _pauseTimer = 0f;
                        _state = State.PausedBack;
                    }
                    break;
                }
                case State.PausedBack:
                    _pauseTimer += Time.fixedDeltaTime;
                    if (_pauseTimer >= pause) _state = State.MovingOut;
                    break;
            }
        }
    }
}
