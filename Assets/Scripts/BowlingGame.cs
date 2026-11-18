using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Nowy skrypt. Nie wymaga zmian w oryginalnym BowlingPin.cs.
public class BowlingGame : MonoBehaviour
{
    [Header("Pins in scene (empty = find automatically)")]
    [SerializeField] private BowlingPin[] pins;
    [SerializeField] private float minimumFallAngle = 45f;
    [SerializeField] private bool showScoreOnScreen = true;

    private struct PinStart
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 up;
        public Rigidbody body;
    }

    private PinStart[] starts;
    private bool[] countedInCurrentRack;
    private readonly List<int>[] frames = new List<int>[10];
    private int frameIndex;
    private int pinsRemaining = 10;
    private bool finished;
    private string lastMessage = "Ready for the first roll";

    private void Awake()
    {
        for (int i = 0; i < frames.Length; i++)
            frames[i] = new List<int>(3);

        if (pins == null || pins.Length == 0)
            pins = FindObjectsByType<BowlingPin>(FindObjectsSortMode.None);

        starts = new PinStart[pins.Length];
        countedInCurrentRack = new bool[pins.Length];
        for (int i = 0; i < pins.Length; i++)
        {
            if (pins[i] == null) continue;
            starts[i].position = pins[i].transform.position;
            starts[i].rotation = pins[i].transform.rotation;
            starts[i].up = pins[i].transform.up;
            starts[i].body = pins[i].GetComponent<Rigidbody>();
        }
    }

    private void Start()
    {
        if (pins.Length != 10)
            Debug.LogWarning("BowlingGame: Expected 10 pins. Found: " + pins.Length);
    }

    private bool IsFallen(int i)
    {
        return pins[i] != null && pins[i].gameObject.activeSelf &&
               Vector3.Angle(pins[i].transform.up, starts[i].up) >= minimumFallAngle;
    }

    // Called once by BallReturn when a roll is complete.
    public void CompleteRoll()
    {
        if (finished || pins.Length != 10) return;

        // Count only pins newly knocked down in this roll, not previous rolls.
        int knocked = 0;
        for (int i = 0; i < pins.Length; i++)
        {
            if (countedInCurrentRack[i] || !IsFallen(i)) continue;
            countedInCurrentRack[i] = true;
            knocked++;
        }

        knocked = Mathf.Clamp(knocked, 0, pinsRemaining);
        List<int> current = frames[frameIndex];
        current.Add(knocked);
        pinsRemaining -= knocked;
        lastMessage = "Pins knocked down: " + knocked;

        if (frameIndex < 9)
        {
            if (current.Count == 1 && knocked == 10)
            {
                lastMessage = "STRIKE!";
                AdvanceFrame();
            }
            else if (current.Count == 2)
            {
                if (current[0] + current[1] == 10) lastMessage = "SPARE!";
                AdvanceFrame();
            }
            else
            {
                HideFallenPins();
            }
            return;
        }

        // Frame 10: a third roll is allowed only after a strike or spare.
        if (current.Count == 1)
        {
            if (knocked == 10)
            {
                lastMessage = "STRIKE!";
                ResetAllPins();
            }
            else HideFallenPins();
        }
        else if (current.Count == 2)
        {
            bool firstStrike = current[0] == 10;
            bool spare = !firstStrike && current[0] + current[1] == 10;
            if (spare) lastMessage = "SPARE!";
            if (firstStrike || spare)
            {
                // After a first-roll strike, the third roll uses remaining pins
                // unless the second roll was also a strike.
                if (spare || current[1] == 10) ResetAllPins();
                else HideFallenPins();
            }
            else finished = true;
        }
        else finished = true;

        if (finished) lastMessage = "GAME OVER! Final score: " + GetTotalScore();
    }

    private void AdvanceFrame()
    {
        frameIndex++;
        ResetAllPins();
    }

    private void HideFallenPins()
    {
        for (int i = 0; i < pins.Length; i++)
            if (countedInCurrentRack[i] && pins[i] != null)
                pins[i].gameObject.SetActive(false);
    }

    private void ResetAllPins()
    {
        for (int i = 0; i < pins.Length; i++)
        {
            countedInCurrentRack[i] = false;
            if (pins[i] == null) continue;
            GameObject pin = pins[i].gameObject;
            pin.SetActive(true);
            Rigidbody rb = starts[i].body;
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.position = starts[i].position;
                rb.rotation = starts[i].rotation;
                rb.Sleep();
            }
            pins[i].transform.SetPositionAndRotation(starts[i].position, starts[i].rotation);
        }
        pinsRemaining = 10;
    }

    // Total score of frames with fully determined bonuses.
    public int GetTotalScore()
    {
        List<int> rolls = new List<int>();
        foreach (List<int> frame in frames) rolls.AddRange(frame);

        int score = 0;
        int roll = 0;
        for (int f = 0; f < 10 && roll < rolls.Count; f++)
        {
            if (f == 9)
            {
                if (finished)
                    for (int j = roll; j < rolls.Count; j++) score += rolls[j];
                break;
            }
            if (rolls[roll] == 10)
            {
                if (roll + 2 >= rolls.Count) break;
                score += 10 + rolls[roll + 1] + rolls[roll + 2];
                roll++;
            }
            else
            {
                if (roll + 1 >= rolls.Count) break;
                int sum = rolls[roll] + rolls[roll + 1];
                if (sum == 10)
                {
                    if (roll + 2 >= rolls.Count) break;
                    score += 10 + rolls[roll + 2];
                }
                else score += sum;
                roll += 2;
            }
        }
        return score;
    }

    private string ScoreText()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("BOWLING - 10 FRAMES");
        sb.AppendLine(finished ? "Game over" : "Frame " + (frameIndex + 1) + "/10, roll " + (frames[frameIndex].Count + 1));
        sb.AppendLine(lastMessage);
        sb.AppendLine("Confirmed score: " + GetTotalScore());
        sb.AppendLine("------------------------");
        for (int f = 0; f < 10; f++)
        {
            if (frames[f].Count == 0) continue;
            sb.Append("R").Append(f + 1).Append(": ");
            for (int j = 0; j < frames[f].Count; j++)
            {
                int n = frames[f][j];
                if (j != 0) sb.Append("  ");
                if (n == 10 && (j == 0 || (f == 9 && j > 0 && (j == 1 || frames[f][j - 1] == 10)))) sb.Append('X');
                else if (j > 0 && n + frames[f][j - 1] == 10 &&
                         (f < 9 || (j == 1 && frames[f][0] != 10) || (j == 2 && frames[f][0] == 10 && frames[f][1] != 10))) sb.Append('/');
                else sb.Append(n);
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private void OnGUI()
    {
        if (!showScoreOnScreen) return;
        GUI.Box(new Rect(12, 12, 290, 345), "SCORE");
        GUI.Label(new Rect(24, 35, 270, 320), ScoreText());
    }
}
