using UnityEngine;
using UnityEngine.UI;

namespace PaperCar
{
    public class MatchUI : MonoBehaviour
    {
        [SerializeField] MatchController match;
        [SerializeField] Text player1Label;
        [SerializeField] Text player2Label;
        [SerializeField] Text stateLabel;
        [SerializeField] Text resultLabel;
        [SerializeField] Button restartButton;

        public void Configure(
            MatchController matchController,
            Text p1,
            Text p2,
            Text state,
            Text result,
            Button restart)
        {
            match = matchController;
            player1Label = p1;
            player2Label = p2;
            stateLabel = state;
            resultLabel = result;
            restartButton = restart;
            Bind();
        }

        void Start()
        {
            Bind();
            Refresh();
        }

        void Bind()
        {
            if (match != null)
            {
                match.OnMatchChanged.RemoveListener(Refresh);
                match.OnMatchChanged.AddListener(Refresh);
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(OnRestart);
                restartButton.onClick.AddListener(OnRestart);
            }
        }

        void OnRestart()
        {
            match.Restart();
        }

        void Refresh()
        {
            if (match == null)
                return;

            if (player1Label != null)
                player1Label.text = match.ActivePlayer == 1 && match.Phase != MatchPhase.MatchOver
                    ? "Player 1  ●"
                    : "Player 1";

            if (player2Label != null)
                player2Label.text = match.ActivePlayer == 2 && match.Phase != MatchPhase.MatchOver
                    ? "Player 2  ●"
                    : "Player 2";

            if (stateLabel != null)
                stateLabel.text = match.StateLabel;

            bool over = match.Phase == MatchPhase.MatchOver;
            if (resultLabel != null)
            {
                resultLabel.gameObject.SetActive(over);
                if (over)
                    resultLabel.text = $"Player {match.Winner} wins\nPlayer {match.Loser} loses";
            }

            if (restartButton != null)
                restartButton.gameObject.SetActive(over);
        }
    }
}
