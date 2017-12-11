using UnityEngine;
using UnityEngine.UI;
using BeauRoutine;
using System.Collections;

namespace BeauRoutine.Examples
{
    public class Example4_Script : MonoBehaviour
    {
        #region Inspector

        [Header("Button")]

        [SerializeField]
        private Button m_ButtonObject;

        [SerializeField]
        private RectTransform m_ButtonTransform;

        [SerializeField]
        private RectTransform m_ButtonRandomRegion;

        [Header("UI")]

        [SerializeField]
        private Text m_InstructionsText;

        [SerializeField]
        private Text m_ScoreText;

        [SerializeField]
        private Button m_RestartButton;

        [Header("Settings")]

        [SerializeField]
        private float m_IntervalStart = 5;

        [SerializeField]
        private float m_IntervalFinal = 1;

        [SerializeField]
        private float m_IntervalFinalPoints = 20;

        #endregion

        private int m_Score = 0;
        private float m_CurrentInterval;

        private Routine m_TimerRoutine;
        private Routine m_InputRoutine;

        private void Start()
        {
            Routine.Start(this, GameLoop());
        }

        #region Routines

        // This will run the game in a loop
        private IEnumerator GameLoop()
        {
            while (true)
            {
                yield return PlayRound();
            }
        }

        // This will run a round of the game
        private IEnumerator PlayRound()
        {
            // Execute the intro sequence
            yield return RoundIntro();

            // While we are counting down,
            // wait for the countdown to expire
            while (m_TimerRoutine)
                yield return m_TimerRoutine;

            // Execute the game over sequence
            yield return GameOver();
        }

        // This will run the intro to the round
        private IEnumerator RoundIntro()
        {
            // Reset everything to our starting state
            m_RestartButton.gameObject.SetActive(false);
            UpdateScore(0);
            ResetButtonPosition();
            UpdateInstructions("Click the Button");

            // Begin executing the click routine
            m_InputRoutine.Replace(this, InputLoop());

            // While the player has no score, we aren't really playing the game
            while (m_Score == 0)
                yield return null;
        }

        // This will display game over and wait for the player to restart
        private IEnumerator GameOver()
        {
            UpdateInstructions("Game Over");

            m_RestartButton.gameObject.SetActive(true);
            yield return m_RestartButton.onClick.WaitForInvoke();
        }

        // This will run the timer
        private IEnumerator RoundTimer()
        {
            UpdateTimeLeft(m_CurrentInterval);
            yield return Routine.Timer(m_CurrentInterval, UpdateTimeLeft);

            m_InputRoutine.Stop();
        }

        // This will respond to player inpu
        private IEnumerator InputLoop()
        {
            while (true)
            {
                // Wait for the player to click on the object
                yield return m_ButtonObject.onClick.WaitForInvoke();

                // Increment score and randomize button position
                UpdateScore(m_Score + 1);
                RandomizeButtonPosition();

                // Restart the timer
                m_TimerRoutine.Replace(this, RoundTimer());
            }
        }

        #endregion

        // Updates score counter and interval
        private void UpdateScore(int score)
        {
            m_Score = score;

            float gameProgression = Mathf.Clamp01((float)score / m_IntervalFinalPoints);
            m_CurrentInterval = Mathf.Lerp(m_IntervalStart, m_IntervalFinal, gameProgression);

            m_ScoreText.text = "Score: " + m_Score;
        }

        // Updates the time display counter
        private void UpdateTimeLeft(float timeLeft)
        {
            UpdateInstructions("Click: " + timeLeft.ToString("0.0") + "s");
        }

        // Updates the instructions
        private void UpdateInstructions(string instructions)
        {
            m_InstructionsText.text = instructions;
        }

        // Resets the button to the center of the screen
        private void ResetButtonPosition()
        {
            m_ButtonTransform.anchoredPosition = Vector2.zero;
        }

        // Randomizes the button's position
        private void RandomizeButtonPosition()
        {
            float halfWidth = m_ButtonRandomRegion.sizeDelta.x / 2;
            float halfHeight = m_ButtonRandomRegion.sizeDelta.y / 2;
            float randomX = Random.Range(-halfWidth, halfWidth);
            float randomY = Random.Range(-halfHeight, halfHeight);

            m_ButtonTransform.anchoredPosition = new Vector2(randomX, randomY);
        }
    }
}