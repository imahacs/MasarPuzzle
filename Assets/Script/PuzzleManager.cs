using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField] private Transform gameTransform;
    [SerializeField] private Transform piecePrefab;
    [SerializeField] private TextMeshProUGUI timerText; // Reference to TextMeshPro for timer
    [SerializeField] private float gameTime = 10f; // Set the game time to 10 seconds

    private List<Transform> pieces;
    private int emptyLocation;
    private int size;
    private bool isGameLoaded = false;
    private float remainingTime;
    private bool isGameOver = false;

    private void CreateGamePieces(float gapThickness)
    {
        float width = 1 / (float)size;
        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                Transform piece = Instantiate(piecePrefab, gameTransform);
                pieces.Add(piece);

                piece.localPosition = new Vector3(-1 + (2 * width * col) + width,
                                                  +1 - (2 * width * row) - width,
                                                  0);
                piece.localScale = ((2 * width) - gapThickness) * Vector3.one;
                piece.name = $"{(row * size) + col}";

                if ((row == size - 1) && (col == size - 1))
                {
                    emptyLocation = (size * size) - 1;
                    piece.gameObject.SetActive(false);
                }
                else
                {
                    float gap = gapThickness / 2;
                    Mesh mesh = piece.GetComponent<MeshFilter>().mesh;
                    Vector2[] uv = new Vector2[4];

                    uv[0] = new Vector2((width * col) + gap, 1 - ((width * (row + 1)) - gap));
                    uv[1] = new Vector2((width * (col + 1)) - gap, 1 - ((width * (row + 1)) - gap));
                    uv[2] = new Vector2((width * col) + gap, 1 - ((width * row) + gap));
                    uv[3] = new Vector2((width * (col + 1)) - gap, 1 - ((width * row) + gap));

                    mesh.uv = uv;
                }
            }
        }
    }

    void Start()
    {
        if (!isGameLoaded)
        {
            pieces = new List<Transform>();
            size = 3; 
            CreateGamePieces(0.01f);
            Shuffle(); // Shuffle pieces immediately after creation
            isGameLoaded = true;
            remainingTime = gameTime; // Initialize the timer
        }
    }

    void Update()
    {
        if (!isGameOver)
        {
            // Update the timer
            remainingTime -= Time.deltaTime;
            timerText.text = $"Time: {remainingTime:F1}";

            if (remainingTime <= 0)
            {
                isGameOver = true;
                DisplayLostMessage();
            }

            // Handle touch input
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Ended)
                {
                    Vector2 touchPosition = touch.position;
                    Vector2 worldPosition = Camera.main.ScreenToWorldPoint(touchPosition);

                    MovePieceBasedOnTouch(worldPosition);
                }
            }

            // Check if puzzle is completed
            if (CheckCompletion())
            {
                isGameOver = true;
                DisplayCorrectAndLoadScene();
            }
        }
    }

    private void MovePieceBasedOnTouch(Vector2 touchPosition)
    {
        float minDistance = float.MaxValue;
        int targetIndex = -1;

        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].gameObject.activeSelf)
            {
                float distance = Vector2.Distance(touchPosition, pieces[i].localPosition);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    targetIndex = i;
                }
            }
        }

        if (targetIndex != -1)
        {
            int row = targetIndex / size;
            int col = targetIndex % size;

            if (emptyLocation == targetIndex - 1 && col != 0) MovePiece(1);
            else if (emptyLocation == targetIndex + 1 && col != size - 1) MovePiece(-1);
            else if (emptyLocation == targetIndex - size) MovePiece(size);
            else if (emptyLocation == targetIndex + size) MovePiece(-size);
        }
    }

    private void MovePiece(int offset)
    {
        int targetLocation = emptyLocation + offset;
        if (targetLocation >= 0 && targetLocation < size * size &&
            ((offset == -1 && emptyLocation % size != 0) ||
            (offset == 1 && emptyLocation % size != size - 1) ||
            (offset == -size && emptyLocation / size != 0) ||
            (offset == size && emptyLocation / size != size - 1)))
        {
            (pieces[emptyLocation], pieces[targetLocation]) = (pieces[targetLocation], pieces[emptyLocation]);
            (pieces[emptyLocation].localPosition, pieces[targetLocation].localPosition) = (pieces[targetLocation].localPosition, pieces[emptyLocation].localPosition);
            emptyLocation = targetLocation;
        }
    }

    private bool CheckCompletion()
    {
        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].name != $"{i}")
            {
                return false;
            }
        }
        return true;
    }

    private void Shuffle()
    {
        int count = 0;
        int last = 0;
        while (count < (size * size * size))
        {
            int rnd = Random.Range(0, size * size);

            if (rnd == last) { continue; }
            last = emptyLocation;

            if (SwapIfValid(rnd, -size, size))
            {
                count++;
            }
            else if (SwapIfValid(rnd, +size, size))
            {
                count++;
            }
            else if (SwapIfValid(rnd, -1, 0))
            {
                count++;
            }
            else if (SwapIfValid(rnd, +1, size - 1))
            {
                count++;
            }
        }
    }

    private bool SwapIfValid(int i, int offset, int colCheck)
    {
        if (((i % size) != colCheck) && ((i + offset) == emptyLocation))
        {
            (pieces[i], pieces[i + offset]) = (pieces[i + offset], pieces[i]);

            (pieces[i].localPosition, pieces[i + offset].localPosition) = (pieces[i + offset].localPosition, pieces[i].localPosition);

            emptyLocation = i;
            return true;
        }
        return false;
    }

    private void DisplayCorrectAndLoadScene()
    {
        SceneManager.LoadScene(2);
    }

    private void DisplayLostMessage()
    {
        SceneManager.LoadScene(3);
    }
}
