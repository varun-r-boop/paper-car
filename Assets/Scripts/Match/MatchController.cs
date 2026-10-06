using UnityEngine;
using UnityEngine.Events;

namespace PaperCar
{
    public enum MatchPhase
    {
        Aiming,
        Coasting,
        MatchOver
    }

    public class MatchController : MonoBehaviour
    {
        [SerializeField] CarController player1;
        [SerializeField] CarController player2;
        [SerializeField] AimIndicator player1Aim;
        [SerializeField] AimIndicator player2Aim;
        [SerializeField] ArenaBounds arena;
        [SerializeField] MonoBehaviour thrustInputBehaviour;
        [SerializeField] float settleSpeed = 0.35f;
        [SerializeField] float minCoastTime = 0.35f;
        [SerializeField] float maxCoastTime = 6f;
        [SerializeField] MicrophoneBlowDetector microphone;

        IThrustInput _thrust;
        int _activePlayer = 1;
        float _coastElapsed;
        int _winner;
        int _loser;

        public MatchPhase Phase { get; private set; } = MatchPhase.Aiming;
        public int ActivePlayer => _activePlayer;
        public int Winner => _winner;
        public int Loser => _loser;
        public UnityEvent OnMatchChanged = new UnityEvent();

        public string StateLabel
        {
            get
            {
                switch (Phase)
                {
                    case MatchPhase.Aiming:
                        return $"P{_activePlayer} aiming — blow to thrust (A/D to aim)";
                    case MatchPhase.Coasting:
                        return "coasting";
                    case MatchPhase.MatchOver:
                        return $"P{_winner} wins / P{_loser} loses";
                    default:
                        return Phase.ToString();
                }
            }
        }

        public void Configure(
            CarController p1,
            CarController p2,
            AimIndicator aim1,
            AimIndicator aim2,
            ArenaBounds bounds,
            IThrustInput thrust,
            MicrophoneBlowDetector mic = null)
        {
            player1 = p1;
            player2 = p2;
            player1Aim = aim1;
            player2Aim = aim2;
            arena = bounds;
            _thrust = thrust;
            thrustInputBehaviour = thrust as MonoBehaviour;
            microphone = mic;
        }

        void Awake()
        {
            if (_thrust == null && thrustInputBehaviour is IThrustInput input)
                _thrust = input;
        }

        void Start()
        {
            BeginMatch();
        }

        void Update()
        {
            if (Phase == MatchPhase.Aiming)
                TickAiming();
            else if (Phase == MatchPhase.Coasting)
                TickCoasting();
        }

        public void Restart()
        {
            player1.ResetForMatch();
            player2.ResetForMatch();
            BeginMatch();
        }

        void BeginMatch()
        {
            _winner = 0;
            _loser = 0;
            _activePlayer = 1;
            SetPhase(MatchPhase.Aiming);
            ApplyAimVisibility();
            Notify();
        }

        void TickAiming()
        {
            if (_thrust == null)
                return;

            if (_thrust.TryGetThrust(out float intensity))
            {
                CarController car = ActiveCar();
                AimIndicator aim = ActiveAim();
                car.ApplyThrust(aim.CurrentDirection, intensity);
                _coastElapsed = 0f;
                SetPhase(MatchPhase.Coasting);
                ApplyAimVisibility();
                Notify();
            }
        }

        void TickCoasting()
        {
            _coastElapsed += Time.deltaTime;
            bool settled = player1.IsSettled(settleSpeed) && player2.IsSettled(settleSpeed);
            bool canResolve = _coastElapsed >= minCoastTime && (settled || _coastElapsed >= maxCoastTime);

            if (!canResolve)
                return;

            if (TryResolveKnockout())
                return;

            _activePlayer = _activePlayer == 1 ? 2 : 1;
            SetPhase(MatchPhase.Aiming);
            ApplyAimVisibility();
            Notify();
        }

        bool TryResolveKnockout()
        {
            bool p1In = arena.Contains(player1.transform.position);
            bool p2In = arena.Contains(player2.transform.position);

            if (p1In && p2In)
                return false;

            if (!p1In && !p2In)
            {
                // Simultaneous ring-out: the player who just blew loses (they left too).
                _loser = _activePlayer;
                _winner = _activePlayer == 1 ? 2 : 1;
            }
            else if (!p1In)
            {
                _loser = 1;
                _winner = 2;
            }
            else
            {
                _loser = 2;
                _winner = 1;
            }

            player1.Freeze();
            player2.Freeze();
            SetPhase(MatchPhase.MatchOver);
            ApplyAimVisibility();
            Notify();
            return true;
        }

        void SetPhase(MatchPhase phase)
        {
            Phase = phase;
            if (microphone != null)
                microphone.Listening = phase == MatchPhase.Aiming;
        }

        CarController ActiveCar() => _activePlayer == 1 ? player1 : player2;
        AimIndicator ActiveAim() => _activePlayer == 1 ? player1Aim : player2Aim;

        void ApplyAimVisibility()
        {
            bool aiming = Phase == MatchPhase.Aiming;
            player1Aim.SetActiveAim(aiming && _activePlayer == 1);
            player2Aim.SetActiveAim(aiming && _activePlayer == 2);
        }

        void Notify()
        {
            OnMatchChanged.Invoke();
        }
    }
}
