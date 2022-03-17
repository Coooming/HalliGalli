using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player 
{
    private int ID; // 玩家编号
    private bool isOut = false; // 标识是否已出局
    private int remainCardNumber; // 剩余卡牌数量
    private int[] card = new int[100]; // 现有卡牌编号，不超过最大卡牌总数6×16=96
    
    public Player(int _ID, int cardHead, int cardNumber)
    {
        ID = _ID;
        remainCardNumber = cardNumber;
        for (int i = 0; i < remainCardNumber; i++) card[i] = cardHead + i; // 初始化已有卡牌编号
        for (int i = remainCardNumber; i < 100; i++) card[i] = -1; // 标记空卡牌
    }

    // 返回该玩家是否已出局的标识
    public bool getIsOut()
    {
        return isOut;
    }

    // 设置该玩家是否已出局的标识
    public void setIsOut(bool _isOut)
    {
        isOut = _isOut;
    }

    // 返回该玩家的剩余卡牌数量
    public int getRemainCardNumber()
    {
        return remainCardNumber;
    }

    // 返回该玩家的下标为index的卡牌编号
    public int getCard(int index)
    {
        // 若下标越界，返回空卡牌标识（-1）
        if (index < 0 || index >= remainCardNumber)
        {
            return -1;
        }
        return card[index];
    }

    // 返回该玩家的牌堆顶卡牌
    public int getTopCard()
    {
        return remainCardNumber > 0 ? card[remainCardNumber - 1] : -1;
    }

    // 从牌堆顶弹出牌
    public void PopCard()
    {
        card[--remainCardNumber] = -1;
    }

    // 在牌堆底随机插入多张卡牌
    public void InsertCards(Queue<int> insertedCards)
    {
        int length = insertedCards.Count; // 插入卡牌的数量
        // 偏移原有卡牌
        for (int i = remainCardNumber - 1; i >= 0; i--)
        {
            card[i + length] = card[i];
        }
        int[] shuffle = new int[length]; // 用于洗牌的数组
        int index = 0;
        // 将要插入的卡牌放入用于洗牌的数组
        foreach (int ID in insertedCards)
        {
            shuffle[index++] = ID;
        }
        // 随机交换若干次shuffle中的两张卡牌的位置，实现洗牌
        int swapTimes = 66;
        while (swapTimes > 0)
        {
            int x = Random.Range(0, length);
            int y = Random.Range(0, length);
            int tmp = shuffle[x];
            shuffle[x] = shuffle[y];
            shuffle[y] = tmp;
            swapTimes--;
        }
        // 插入新卡牌
        for (int i = 0; i < length; i++)
        {
            card[i] = shuffle[i];
        }
        remainCardNumber += length; // 更新剩余卡牌数量
    }

    // 在牌堆底插入一张卡牌
    public void InsertCards(int ID)
    {
        // 偏移原有卡牌
        for (int i = remainCardNumber - 1; i >= 0; i--)
        {
            card[i + 1] = card[i];
        }
        card[0] = ID; // 插入新卡牌
        remainCardNumber++; // 更新剩余卡牌数量
    }
}
