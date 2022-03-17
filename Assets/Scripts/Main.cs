using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    // 根据玩家数量确定的不同编号玩家对应的插槽编号，见“玩家位置分布.xlsx”
    private static int[][] playerCorSlot = new int[5][]
    {
        new int[] {1, 2},
        new int[] {1, 6, 2},
        new int[] {1, 4, 8, 2},
        new int[] {1, 4, 6, 8, 2},
        new int[] {1, 3, 5, 7, 9, 2}
    };

    // 每个玩家的控制按键（出牌键和按铃键），见“游戏大纲.docx”
    private static KeyCode[][] controlKeys = new KeyCode[6][]
    {
        new KeyCode[] {KeyCode.Q, KeyCode.W},
        new KeyCode[] {KeyCode.C, KeyCode.V},
        new KeyCode[] {KeyCode.Y, KeyCode.U},
        new KeyCode[] {KeyCode.Comma, KeyCode.Period},
        new KeyCode[] {KeyCode.LeftBracket, KeyCode.RightBracket},
        new KeyCode[] {KeyCode.Keypad1, KeyCode.Keypad2}
    };

    // 每个玩家的控制按键的字符，用于显示
    private static char[][] controlKeysChar = new char[6][]
    {
        new char[] {'Q', 'W'},
        new char[] {'C', 'V'},
        new char[] {'Y', 'U'},
        new char[] {',', '.'},
        new char[] {'[', ']'},
        new char[] {'1', '2'}
    };

    private const float playCardWaitingTime = 10f; // 等待玩家出牌的时间
    private const float outWaitingTime = 2f; // 出现玩家出局到恢复游戏的等待时间
    private const float coolDownTime = 1f; // 一名玩家发牌后到下一名玩家可以发牌的冷却时间
    private const int refreshCardNumber = 8; // 触发刷新的卡牌数量（桌面最大的卡牌数量）

    [Header("GameObject设置")]
    public GameObject[] playerSlot = new GameObject[9]; // 场景中的玩家插槽
    public GameObject cardPrefab; // 卡牌的预制体
    public AudioClip bellSound; // 按铃的音效
    public Canvas gameOverCanvas; // 游戏胜利画布
    public Text Tips; // 游戏提示

    [Header("游戏设置")]
    [SerializeField] private int playerNumber; // 玩家数量
    [SerializeField] private int presentPlayerNumber; // 在场（未出局）玩家数量
    [SerializeField] private int initCardNumber; // 初始卡牌数量
    [SerializeField] private int cardSet; // 本局游戏的卡牌套（4种水果的种类）
    [SerializeField] private int totalCardNumber; // 总卡牌数量

    private int[] playerSlotNumber; // 第i名玩家的插槽编号
    private Vector3[] cardPos; // 第i名玩家的牌堆位置
    private Card[] card; // 所有卡牌对象
    private Player[] player; // 所有玩家对象
    private Queue<int> cardOnTable; // 桌面上的卡牌编号
    private Vector3[] cardOnTablePos = new Vector3[8]; // 桌面上的卡牌位置
    private bool[] cardOnTablePosUsed = new bool[8]; // 标记桌面上的卡牌位置是否被用
    private int cardOnTablePosChoose; // 当前选择的卡牌位置
    private float cardOnTableRot; // 桌面上卡牌的旋转角度

    [Header("游戏状态")]
    [SerializeField] private int currentPlayer; // 当前玩家
    [SerializeField] private int bellPlayer; // 按铃玩家
    [SerializeField] private int bellResult = -1; // 按铃结果，-1表示未知，1表示成功，0表示失败
    [SerializeField] private bool isPause = false; // 标识游戏是否暂停
    [SerializeField] private bool isStarting = false; // 标识游戏是否处于启动过程
    [SerializeField] private bool isDealingOut = false; // 标识是否正在处理出局
    [SerializeField] private bool isPlayingCard = false; // 标识是否正在出牌
    [SerializeField] private bool isCoolingDown = false; // 标识是否正在冷却
    [SerializeField] private bool isSettling = false; // 标识是否正在结算
    [SerializeField] private bool hasSettled; // 表示是否完成了实际逻辑结算

    [Header("计时器")]
    [SerializeField] private float dealingOutTimer; // 处理出局倒计时
    [SerializeField] private float coolingDownTimer; // 处理冷却倒计时
    [SerializeField] private float waitingTimer; // 等待发牌倒计时
    [SerializeField] private float settlingTimer; // 结算的计时器

    void Start()
    {
        try
        {
            playerNumber = GameObject.Find("GameSetting").GetComponent<GameSetting>().getPlayerNumber(); // 从游戏设置中获取玩家数量
            initCardNumber = GameObject.Find("GameSetting").GetComponent<GameSetting>().getInitCardNumber(); // 从游戏设置中获取初始卡牌数量
        }
        catch (System.Exception) // 未从MainPage场景开始运行
        {
            playerNumber = 6;
            initCardNumber = 16;
        }
        presentPlayerNumber = playerNumber; // 初始化在场玩家数量
        cardSet = Random.Range(0, 3); // 随机卡牌套（4种水果的种类）
        InitCanvas(); // 初始化画布
        CreateCards(); // 创建卡牌
        CreatePlayers(); // 创建玩家
        InitScene(); // 初始化场景
        InitGame(); // 初始化游戏设置
        isStarting = true;
    }

    // 游戏主循环
    void Update()
    {
        if (isPause) return; // 游戏未暂停才进行主循环

        // 等待游戏启动
        if (isStarting)
        {
            bool isFinish = true; // 标识是否发牌结束
            // 将卡牌移到对应玩家的牌堆位置
            for (int i = 0; i < playerNumber; i++)
            {
                int finishCard = 0;
                for (int j = 0; j < initCardNumber; j++)
                {
                    isFinish = card[player[i].getCard(j)].MoveTo(cardPos[i], 215f, 0);
                    if (!isFinish) break;
                    finishCard++;
                }
                playerSlot[playerSlotNumber[i]].transform.GetChild(1).GetComponent<Text>().text = finishCard.ToString();
                if (!isFinish) return;
            }
            HideTips(); // 隐藏提示
            ShowClock(); // 显示时钟
            waitingTimer = playCardWaitingTime;
            isStarting = false;
            return;
        }

        // 等待出局
        if (isDealingOut)
        {
            dealingOutTimer -= Time.deltaTime;
            if (dealingOutTimer <= 0f)
            {
                HideTips();
                // 寻找下一个存活玩家
                while (player[currentPlayer].getIsOut())
                {
                    currentPlayer = (currentPlayer + 1) % playerNumber;
                }
                ShowClock();
                waitingTimer = playCardWaitingTime;
                isDealingOut = false;
            }
            return;
        }

        // 等待出牌过程
        if (isPlayingCard)
        {
            // 随机选择未被使用的桌面卡牌位置，并随机偏转角度
            while (cardOnTablePosUsed[cardOnTablePosChoose])
            {
                cardOnTablePosChoose = Random.Range(0, 8);
                cardOnTableRot = Random.Range(-20f, 20f);
            }
            isPlayingCard = !card[player[currentPlayer].getTopCard()].MoveTo(cardOnTablePos[cardOnTablePosChoose], 50f, 1, cardOnTableRot);
            if (!isPlayingCard) // 出牌移动完成
            {
                cardOnTablePosUsed[cardOnTablePosChoose] = true;
                cardOnTable.Enqueue(player[currentPlayer].getTopCard());
                player[currentPlayer].PopCard();
                HideClock(); // 隐藏当前玩家的时钟
                ChangeCardNumber(); // 改变当前玩家的卡牌数量
                currentPlayer = (currentPlayer + 1) % playerNumber; // 切换当前玩家
                coolingDownTimer = coolDownTime;
                isCoolingDown = true; // 进入冷却
            }
            return;
        }

        // 等待冷却
        if (isCoolingDown)
        {
            coolingDownTimer -= Time.deltaTime;
            if (coolingDownTimer <= 0f)
            {
                ShowClock();
                isCoolingDown = false;
            }
            return;
        }

        // 等待结算
        if (isSettling)
        {
            if (bellResult == -1)
            {
                bellResult = Settlement(); // 获取局面结果
                settlingTimer = 0f;
                hasSettled = false;
                ChangeTips(bellResult, bellPlayer);
                ShowTips();
                HideClock(); // 隐藏当前玩家的时钟
            }
            settlingTimer += Time.deltaTime;
            
            // 根据结算结果在场景中移动卡牌
            if (settlingTimer > 1f && !hasSettled)
            {
                if (bellResult == 1)
                {
                    foreach (int index in cardOnTable)
                    {
                        card[index].MoveTo(cardPos[bellPlayer], 50f, 2);
                    }
                }
                else
                {
                    int otherPlayer = bellPlayer;
                    int need = Mathf.Min(presentPlayerNumber - 1, player[bellPlayer].getRemainCardNumber()); // 需要的罚牌数量
                    int index = player[bellPlayer].getRemainCardNumber() - 1; // 下一张罚牌的下标
                    for (int i = 1; i <= need; i++)
                    {
                        otherPlayer = (otherPlayer + 1) % playerNumber;
                        while (player[otherPlayer].getIsOut())
                        {
                            otherPlayer = (otherPlayer + 1) % playerNumber;
                        }
                        card[player[bellPlayer].getCard(index)].MoveTo(cardPos[otherPlayer], 50f, 0);
                        index--;
                    }
                }
            }

            // 根据结算结果执行奖励或罚牌逻辑，确保只执行一次
            if (settlingTimer > 2.5f && !hasSettled)
            {
                if (bellResult == 1)
                {
                    player[bellPlayer].InsertCards(cardOnTable);
                    cardOnTable.Clear();
                }
                else
                {
                    int otherPlayer = bellPlayer;
                    int need = Mathf.Min(presentPlayerNumber - 1, player[bellPlayer].getRemainCardNumber()); // 需要的罚牌数量
                    for (int i = 1; i <= need; i++)
                    {
                        otherPlayer = (otherPlayer + 1) % playerNumber;
                        while (player[otherPlayer].getIsOut())
                        {
                            otherPlayer = (otherPlayer + 1) % playerNumber;
                        }
                        player[otherPlayer].InsertCards(player[bellPlayer].getTopCard());
                        player[bellPlayer].PopCard();
                    }
                }
                ChangeCardNumber();
                hasSettled = true;
            }

            if (settlingTimer > 3.5f)
            {
                bellResult = -1;
                HideTips();
                currentPlayer = bellPlayer;
                ShowClock();
                waitingTimer = playCardWaitingTime;
                isSettling = false;
            }
        }

        waitingTimer -= Time.deltaTime;
        ChangeClockTime();
        // 超时时自动出牌
        if (waitingTimer <= 0f) PlayCard();
    }
    
    // 处理游戏局面和按键输入的循环
    void OnGUI()
    {
        if (isPause || isStarting || isDealingOut || isPlayingCard || isSettling) return;

        // 保持当前玩家为在场玩家
        while (player[currentPlayer].getIsOut())
        {
            currentPlayer = (currentPlayer + 1) % playerNumber;
        }

        // 当游戏仅存一名玩家时，结束游戏
        if (presentPlayerNumber == 1)
        {
            ChangeTips(-2, currentPlayer);
            gameOverCanvas.enabled = true; // 显示游戏结束画布
            isPause = true; // 暂停游戏
            return;
        }

        // 若当前玩家没牌，则进行出局操作
        if (player[currentPlayer].getRemainCardNumber() <= 0)
        {
            player[currentPlayer].setIsOut(true);
            presentPlayerNumber--;
            ChangeTips(-1, currentPlayer);
            ShowTips();
            HideClock();
            dealingOutTimer = outWaitingTime;
            isDealingOut = true; // 进入出局处理
            return;
        }

        // 当前不在出牌和结算过程中，且有按键按下
        if (Input.anyKeyDown)
        {
            Event currentEvent = Event.current; // 获取正在处理的事件
            if (!currentEvent.isKey) return; // 过滤非按键事件
            KeyCode currentKey = currentEvent.keyCode; // 读取当前按键

            // 判断是否当前玩家的出牌键
            if (currentKey == controlKeys[currentPlayer][0])
            {
                if (!isCoolingDown) PlayCard(); // 若没有处于冷却中，进行出牌操作
                return;
            }

            // 否则，寻找是哪名玩家的按铃键
            for (int i = 0; i < playerNumber; i++) 
            {
                if (player[i].getIsOut()) return; // 确保玩家未出局
                if (currentKey != controlKeys[i][1]) continue;
                AudioManager.instance.AudioPlay(bellSound); // 播放按铃音效
                bellPlayer = i; // 标记按铃的玩家
                bellResult = -1; // 标记目前按铃结果为未知
                Debug.Log("P" + (i + 1) + " presses the bell.");
                isSettling = true; // 进入结算
                return;
            }
        }
    }

    private void InitCanvas()
    {
        gameOverCanvas.enabled = false; // 禁用游戏结束画布
        for (int i = 0; i < 9; i++)
        {
            playerSlot[i].SetActive(false); // 禁用所有玩家插槽
        }
        playerSlotNumber = new int[playerNumber];
        cardPos = new Vector3[playerNumber];
        for (int i = 0; i < playerNumber; i++)
        {
            playerSlotNumber[i] = playerCorSlot[playerNumber - 2][i] - 1; // 初始化第i名玩家的插槽编号
            GameObject player = playerSlot[playerSlotNumber[i]]; // 获取Canvas中的玩家插槽对象
            player.SetActive(true);
            cardPos[i] = player.transform.GetChild(0).position; // 获取CardForLocation的位置坐标
            player.transform.GetChild(0).gameObject.SetActive(false); // 禁用CardForLocation
            player.transform.GetChild(1).GetComponent<Text>().text = "0"; // 设置CardNumber
            player.transform.GetChild(2).gameObject.SetActive(false); // 禁用Clock
            Sprite sprite = Resources.Load<Sprite>("Pictures/ProfilePicture/Snoopy" + (i + 1)); // 获取玩家头像资源
            player.transform.GetChild(3).GetChild(0).GetComponent<Image>().overrideSprite = sprite; // 设置玩家头像
            player.transform.GetChild(4).GetComponent<Text>().text = "P" + (i + 1); // 设置玩家编号
            player.transform.GetChild(5).GetComponent<Text>().text = controlKeysChar[i][0].ToString(); // 设置出牌键
            player.transform.GetChild(6).GetComponent<Text>().text = controlKeysChar[i][1].ToString(); // 设置按铃键
        }
    }

    private void CreateCards()
    {
        totalCardNumber = playerNumber * initCardNumber;
        card = new Card[totalCardNumber];
        for (int i = 0; i < totalCardNumber; i++)
        {
            card[i] = new FruitCard(i, cardSet, cardPrefab);
        }
    }

    private void CreatePlayers()
    {
        player = new Player[playerNumber];
        for (int i = 0; i < playerNumber; i++)
        {
            player[i] = new Player(i, i * initCardNumber, initCardNumber);
        }
    }

    private void InitScene()
    {
        cardOnTable = new Queue<int>(); // 初始化桌面上的卡牌
        // 获取桌面卡牌的位置坐标并禁用
        for (int i = 0; i < 8; i++)
        {
            GameObject cardOnTable = GameObject.Find("GameScene").transform.GetChild(i + 2).gameObject;
            cardOnTablePos[i] = cardOnTable.transform.position;
            cardOnTable.SetActive(false);
        }
    }

    private void InitGame()
    {
        currentPlayer = Random.Range(0, playerNumber); // 随机初始玩家
        Debug.Log("The first player is P" + (currentPlayer + 1) + ".");
        cardOnTablePosChoose = Random.Range(0, 8);
        cardOnTableRot = Random.Range(-20f, 20f);
        waitingTimer = playCardWaitingTime;
    }

    private void ShowTips()
    {
        Tips.enabled = true;
    }

    private void HideTips()
    {
        Tips.enabled = false;
    }

    // 改变提示的内容，type为提示类型，-2表示胜利提示，-1表示出局提示，1表示按铃成果提示，0表示按铃失败提示
    private void ChangeTips(int type, int playerID)
    {
        string playerName = "P" + (playerID + 1);
        switch (type)
        {
            case -2:
                Tips.text = "恭喜" + playerName + "获胜！";
                Tips.color = Color.yellow;
                break;
            case -1:
                Tips.text = playerName + "出局";
                Tips.color = Color.yellow;
                break;
            case 1:
                Tips.text = playerName + "按铃正确，获得桌面所有牌";
                Tips.color = Color.green;
                break;
            case 0:
                Tips.text = playerName + "按铃错误，罚牌给其他玩家";
                Tips.color = Color.red;
                break;
        }
    }

    private void ShowClock()
    {
        ChangeClockTime();
        GameObject player = playerSlot[playerSlotNumber[currentPlayer]]; // 获取Canvas中的玩家插槽对象
        player.transform.GetChild(2).gameObject.SetActive(true); // 启用Clock
    }

    private void HideClock()
    {
        GameObject player = playerSlot[playerSlotNumber[currentPlayer]]; // 获取Canvas中的玩家插槽对象
        player.transform.GetChild(2).gameObject.SetActive(false); // 禁用Clock
    }

    private void ChangeClockTime()
    {
        GameObject player = playerSlot[playerSlotNumber[currentPlayer]]; // 获取Canvas中的玩家插槽对象
        player.transform.GetChild(2).GetChild(0).GetComponent<Text>().text = Mathf.Floor(waitingTimer).ToString();
    }

    private void ChangeCardNumber()
    {
        for (int i = 0; i < playerNumber; i++)
        {
            playerSlot[playerSlotNumber[i]].transform.GetChild(1).GetComponent<Text>().text = player[i].getRemainCardNumber().ToString();
        }
    }

    // 发牌
    private void PlayCard()
    {
        if (cardOnTable.Count >= refreshCardNumber)  // 检查是否需要执行刷新操作
        {
            // 销毁当前桌面上的所有卡牌
            foreach (int ID in cardOnTable)
            {
                card[ID].DestoryInstance();
            }
            cardOnTable.Clear(); // 清空桌面上的卡牌
            // 清空桌面位置使用状态
            for (int i = 0; i < 8; i++)
            {
                cardOnTablePosUsed[i] = false;
            }
            cardOnTablePosChoose = Random.Range(0, 8);
            cardOnTableRot = Random.Range(-20f, 20f);
            Debug.Log("The table has been refreshed.");
        }
        HideClock();
        Debug.Log("P" + (currentPlayer + 1) + " plays the card.");
        waitingTimer = playCardWaitingTime; // 重置等待时间
        isPlayingCard = true;
    }
    
    // 结算，返回1表示按铃正确，返回0表示按铃错误
    private int Settlement()
    {
        int[] fruitCount = new int[4] {0, 0, 0, 0};
        foreach (int ID in cardOnTable)
        {
            FruitCard fruitCard = (FruitCard)card[ID];
            for (int i = 0, I = fruitCard.getFruitNumber(); i < I; i++)
            {
                fruitCount[fruitCard.getFruitKind(i)]++;
            }
        }
        for (int i = 0; i < 4; i++)
        {
            if (fruitCount[i] >= 5) return 1;
        }
        return 0;
    }
    
    // 返回主菜单
    public void ReturnMenu()
    {
        Destroy(GameObject.Find("GameSetting"));
        SceneManager.LoadScene("MainPage");
    }

    // 退出游戏
    public void QuitGame()
    {
        Application.Quit();
    }
}
