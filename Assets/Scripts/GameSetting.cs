using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSetting : MonoBehaviour
{
    public Canvas menuCanvas; // 主菜单的画布
    public Canvas gameModeCanvas; // 选择游戏模式的画布
    public Canvas playerNumberCanvas; // 选择玩家数量的画布
    public Canvas initCardNumberCanvas; // 选择初始卡牌数量的画布

    [SerializeField] private int playerNumber; // 玩家数量
    [SerializeField] private int initCardNumber; // 初始卡牌数量

    // 进入主页面，仅显示主菜单的画布
    void Start()
    {
        menuCanvas.enabled = true;
        gameModeCanvas.enabled = false;
        playerNumberCanvas.enabled = false;
        initCardNumberCanvas.enabled = false;
        DontDestroyOnLoad(this); // 在场景移动时保留游戏设置
    }

    // 给外部提供的获取玩家数量的接口
    public int getPlayerNumber()
    {
        return playerNumber;
    }

    // 给外部提供的获取初始卡牌数量的接口
    public int getInitCardNumber()
    {
        return initCardNumber;
    }

    // 主菜单画布的按钮功能：开始游戏
    public void StartGame()
    {
        menuCanvas.enabled = false;
        gameModeCanvas.enabled = true;
    }

    // 主菜单画布的按钮功能：退出游戏
    public void ExitGame()
    {
        Application.Quit();
    }

    // 游戏模式选择画布的按钮功能：简单模式
    public void SimpleMode()
    {
        gameModeCanvas.enabled = false;
        playerNumberCanvas.enabled = true;
    }

    // 游戏模式选择画布的按钮功能：返回上一页
    public void ReturnButtonInGameMode()
    {
        gameModeCanvas.enabled = false;
        menuCanvas.enabled = true;
    }

    // 游戏模式选择画布的按钮功能：返回至主菜单（等价于返回上一页）
    public void MenuButtonInGameMode()
    {
        gameModeCanvas.enabled = false;
        menuCanvas.enabled = true;
    }

    // 玩家数量选择画布的按钮功能：选择玩家数量
    public void SetPlayer(int number)
    {
        playerNumber = number;
        playerNumberCanvas.enabled = false;
        initCardNumberCanvas.enabled = true;
    }

    // 玩家数量选择画布的按钮功能：返回上一页
    public void ReturnButtonInPlayerNumber()
    {
        playerNumberCanvas.enabled = false;
        gameModeCanvas.enabled = true;
    }

    // 玩家数量选择画布的按钮功能：返回至主菜单
    public void MenuButtonInPlayerNumber()
    {
        playerNumberCanvas.enabled = false;
        menuCanvas.enabled = true;
    }

    // 初始卡牌数量选择画布的按钮功能：选择初始卡牌数量
    public void SetInitCard(int number)
    {
        initCardNumber = number;
        initCardNumberCanvas.enabled = false;
        SceneManager.LoadScene("Game"); // 进入游戏场景
    }

    // 初始卡牌数量选择画布的按钮功能：返回上一页
    public void ReturnButtonInInitCardNumber()
    {
        initCardNumberCanvas.enabled = false;
        playerNumberCanvas.enabled = true;
    }

    // 初始卡牌数量选择画布的按钮功能：返回至主菜单
    public void MenuButtonInInitCardNumber()
    {
        initCardNumberCanvas.enabled = false;
        menuCanvas.enabled = true;
    }
}
