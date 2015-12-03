using UnityEngine;
using System.Collections;

public class SnakeGame : MonoBehaviour {

    public static int MAP_WIDTH = 15;
    public static int MAP_HEIGHT = 9;
    public static int MAP_WIDTH_HALF = MAP_WIDTH / 2;
    public static int MAP_HEIGHT_HALF = MAP_HEIGHT / 2;

    public static int STATUS_MAIN_MENU = 1;
    public static int STATUS_PLAY = 2;
    public static int STATUS_PAUSED = 3;
    public static int STATUS_DEAD_PROCESS = 4;
    public static int STATUS_DEAD_MENU = 5;

    public int BorderX = 8;

    public int BorderY = 5;

    public float Delta = 0.5f;

    public float DeadProcessLength = 1;
    private System.DateTime deadProcessFinishTime;

    private bool initialized = false;

    private int requiredRotation = 0;

    private int bitsCount = 1;

    private Sprite[] foodSprites;

    private int status = STATUS_MAIN_MENU;
    public int Status
    {
        get { return status; }
        set { status = value; }
    }

    private static Vector3[] possibleRotations = new[] {
		new Vector3(),
     	new Vector3(0, 90, 0),
     	new Vector3(0, -90, 0),
	};

    /// <summary>
    /// Карта для сохранения занятых точек карты.
    /// Необходима для нахождения свободных ячеек для еды.
    /// </summary>
    private GameObject[,] map = new GameObject[MAP_WIDTH, MAP_HEIGHT];

    public GameObject Snake;
    public GameObject SnakeBits;
    public GameObject SnakeHead;
    public GameObject SnakeTail;
    public GameObject SnakeBitTemplate;
    public GameObject Food;

	// Use this for initialization
	void Start () {
        GameObject.Find("CanvasMenu").GetComponent<Canvas>().enabled = true;
        Object[] objs = Resources.LoadAll("Food", typeof(Sprite));
        foodSprites = new Sprite[objs.Length];
        for (var i = 0; i < objs.Length; i++)
        {
            foodSprites.SetValue(objs[i], i);
        }
	}

    private bool isInputLeft()
    {
        if (Input.GetButtonDown("Left"))
        {
            return true;
        }
        if (
            Input.touchCount > 0
            && Input.touches[Input.touchCount - 1].phase == TouchPhase.Began
            && Input.touches[Input.touchCount - 1].position.x < Screen.width / 2
        ) {
            return true;
        }
        return false;
    }

    private bool isInputRight()
    {
        if (Input.GetButtonDown("Right"))
        {
            return true;
        }
        if (
            Input.touchCount > 0
            && Input.touches[Input.touchCount - 1].phase == TouchPhase.Began
            && Input.touches[Input.touchCount - 1].position.x > Screen.width / 2
        )
        {
            return true;
        }
        return false;
    }
	
	// Update is called once per frame
	void Update () {
        if (!initialized)
        {
            initialized = true;
            SnakeBitTemplate.transform.position = new Vector3(10, 0, -100);
            ResetGame();
            InvokeRepeating("Move", Delta, Delta);
        }
        else if (status == STATUS_DEAD_PROCESS)
        {
            Snake.transform.localScale = new Vector3(Snake.transform.localScale.x * 0.95f, Snake.transform.localScale.y * 0.95f, Snake.transform.localScale.z * 0.95f);
            Debug.Log(
                System.DateTime.Now.ToShortTimeString() + " - " + deadProcessFinishTime.ToShortTimeString() + System.DateTime.Compare(System.DateTime.Now, deadProcessFinishTime)
            );
            if (System.DateTime.Compare(System.DateTime.Now, deadProcessFinishTime) > 0)
            {
                status = STATUS_MAIN_MENU;
                GameObject.Find("CanvasMenu").GetComponent<Canvas>().enabled = true;
            }
        }
        else if (status == STATUS_PLAY)
        {

            if (isInputLeft())
            {
                requiredRotation = 1;
            }
            if (isInputRight())
            {
                requiredRotation = 2;
            }
        }
	}

    public void DebugPrintMap()
    {
        var log = "";
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                log += (map[x, y] == null) ? "0" : "1";
            }
            log += "\n";
        }
        Debug.Log(log);
    }

    public GameObject[,] GetMap()
    {
        return map;
    }

    public GameObject GetMapValue(float x, float y)
    {
        return GetMapValue(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
    }

    public GameObject GetMapValue(int x, int y)
    {
        var xi = x + MAP_WIDTH_HALF;
        var yi = y + MAP_HEIGHT_HALF;
//        Debug.Log("GetMap " + xi + ":" + yi + "=" + map[xi, yi]);
        return map[xi, yi];
    }

    public void ResetGame()
    {
        foreach (Transform child in SnakeBits.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        ResetMap();
        Snake.transform.localScale = new Vector3(1, 1, 1);

        Debug.Log(SnakeHead.transform.rotation.ToString());

        SnakeHead.transform.position = new Vector3(2, 0);
        SnakeHead.transform.rotation = new Quaternion(0, -0.7f, -0.7f, 0);
        SnakeTail.transform.position = new Vector3(2, -1);
        SnakeTail.transform.rotation = new Quaternion(0, -0.7f, -0.7f, 0);
        SetMapValue(SnakeHead.transform.position, SnakeHead);
        SetMapValue(SnakeTail.transform.position, SnakeTail);
        SnakeHead.GetComponent<SnakeBit>().NextBit = SnakeTail;
    }

    public GameObject[,] ResetMap()
    {
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                map[x, y] = null;
            }
        }
        return map;
    }

    public void SetMapValue(float x, float y, GameObject val)
    {
        var xi = Mathf.RoundToInt(x) + MAP_WIDTH_HALF;
        var yi = Mathf.RoundToInt(y) + MAP_HEIGHT_HALF;
//        Debug.Log("SetMap " + xi + ":" + yi + "=" + val);
        map[xi, yi] = val;
    }

    public void SetMapValue(Vector3 position, GameObject val)
    {
        SetMapValue(position.x, position.y, val);
    }

    void Move()
    {
        if (status != STATUS_PLAY)
        {
            return;
        }

        if (requiredRotation > 0)
        {
            SnakeHead.transform.Rotate(possibleRotations[requiredRotation]);
            requiredRotation = 0;
        }
        Vector3 lastPosition = SnakeHead.transform.position;
        Quaternion lastRotation = SnakeHead.transform.rotation;

        float newX = SnakeHead.transform.position.x + SnakeHead.transform.forward.x;
        float newY = SnakeHead.transform.position.y + SnakeHead.transform.forward.y;

        float epsilon = 0.001f;

        if (newX < -BorderX || BorderX < newX || (BorderX - Mathf.Abs(newX)) < epsilon)
        {
            gameOver();
            return;
        }
        if (newY < -BorderY || BorderY < newY || (BorderY - Mathf.Abs(newY)) < epsilon)
        {
            gameOver();
            return;
        }

        var objectInTarget = GetMapValue(newX, newY);
        if (objectInTarget && objectInTarget != SnakeTail)
        {
            gameOver();
            return;
        }

        SnakeHead.transform.position += SnakeHead.transform.forward;

        SetMapValue(SnakeHead.transform.position, gameObject);

        bool snakeIncrease = false;
        var foodSpriteRenderer = Food.transform.FindChild("Sprite").GetComponent<SpriteRenderer>();
        var lastFoodSprite = foodSpriteRenderer.sprite;
        if (
            Mathf.Abs(newX - Food.transform.position.x) < epsilon
            && Mathf.Abs(newY - Food.transform.position.y) < epsilon
        ) {
            snakeIncrease = true;
            Food.transform.position = FindNewFoodPosition();
            foodSpriteRenderer.sprite = foodSprites[Random.Range(0, foodSprites.Length)];
        }

        // increase size
        if (snakeIncrease)
        {
            GameObject newBit = (GameObject)Instantiate(SnakeBitTemplate, lastPosition, lastRotation);
            newBit.transform.parent = SnakeBits.transform;
            newBit.GetComponent<SnakeBit>().NextBit = SnakeHead.GetComponent<SnakeBit>().NextBit;
            newBit.transform.FindChild("Sprite").GetComponent<SpriteRenderer>().sprite = lastFoodSprite;
            SnakeHead.GetComponent<SnakeBit>().NextBit = newBit;
            SetMapValue(lastPosition, newBit);
            bitsCount++;
        }
        else
        {
            GameObject bit = SnakeHead.GetComponent<SnakeBit>().NextBit;
            while (bit != null)
            {
                Vector3 tempPosition = bit.transform.position;
                Quaternion tempRotation = bit.transform.rotation;
                bit.transform.position = lastPosition;
                bit.transform.rotation = lastRotation;
                SetMapValue(lastPosition, bit);
                lastPosition = tempPosition;
                lastRotation = tempRotation;
                bit = bit.GetComponent<SnakeBit>().NextBit;
            }
            //var before = GetMapValue(lastPosition.x, lastPosition.y);

            // проверяем на хвост
            if (
                Mathf.Abs(SnakeHead.transform.position.x - lastPosition.x) > epsilon
                || Mathf.Abs(SnakeHead.transform.position.y - lastPosition.y) > epsilon
            ) {
                SetMapValue(lastPosition, null);
            }
            
            //Debug.Log("Free cell " + lastPosition.x + " " + lastPosition.y + ": " + before + " - " + Game.GetMapValue(lastPosition.x, lastPosition.y));
        }
        //DebugPrintMap();
    }

    Vector3 FindNewFoodPosition()
    {
        int x = Random.Range(-BorderX + 1, BorderX - 1);
        int y = Random.Range(-BorderY + 1, BorderY - 1);
        if (GetMapValue(x, y) != null)
        {
            Debug.Log("FindNewFoodPosition extra");
            // эта клетка занята, ищем другую
            var freeCells = new int[SnakeGame.MAP_WIDTH * SnakeGame.MAP_HEIGHT];
            var freeCellsCount = 0;
            for (int mx = 0; mx < map.GetLength(0); mx++)
            {
                for (int my = 0; my < map.GetLength(1); my++)
                {
                    if (map[mx, my] == null)
                    {
                        freeCells[freeCellsCount] = mx * SnakeGame.MAP_WIDTH + my;
                        freeCellsCount++;
                    }
                }
            }
            if (freeCellsCount > 0)
            {
                // Свободные клетки есть
                var cell = Random.Range(0, freeCellsCount - 1);
                x = freeCells[cell] / SnakeGame.MAP_WIDTH - SnakeGame.MAP_WIDTH_HALF;
                y = freeCells[cell] % SnakeGame.MAP_WIDTH - SnakeGame.MAP_HEIGHT_HALF;
                Debug.Log("Cell " + freeCellsCount + " " + cell + ": " + freeCells[cell] + " (" + x + ";" + y + ")");
            }
        }
        return new Vector3(x, y);
    }

    void gameOver()
    {
        deadProcessFinishTime = System.DateTime.Now.AddSeconds(DeadProcessLength);
        Status = STATUS_DEAD_PROCESS;

    }

    int GetBitsCount()
    {
        return bitsCount;
    }

}
