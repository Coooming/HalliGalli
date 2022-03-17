using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 卡牌的基类（抽象类）
public abstract class Card
{
    protected static Material blankMaterial = Resources.Load<Material>("Materials/Blank"); // 空白材质
    protected static Material cardBackMaterial = Resources.Load<Material>("Materials/CardBack"); // 卡背材质

    protected int ID; // 卡牌编号
    protected int cardSet; // 卡牌套
    protected GameObject instance; // 场景中的GameObject实例
    protected bool isBack = true; // 标识当前是否背面显示

    public Card(int _ID, int _cardSet, GameObject prefab)
    {
        ID = _ID;
        cardSet = _cardSet;
        instance = GameObject.Instantiate(prefab); // 实例化卡牌的prefab
    }

    // 销毁场景中的GameObject实例
    public void DestoryInstance()
    {
        GameObject.Destroy(instance);
    }

    // 移动该卡牌到指定位置，并进行一定偏转（默认不偏转），返回true表示移动和偏转完成，rotationType为0表示不旋转，1表示旋转至正面，2表示旋转至背面
    public bool MoveTo(Vector3 destination, float moveSpeed, int rotationType, float rotationZ = 0f)
    {
        instance.transform.position = Vector3.MoveTowards(instance.transform.position, destination, Time.deltaTime * moveSpeed);
        if (!instance.transform.position.Equals(destination)) return false; // 先等待移动完成
        if (rotationType == 0) return true; // 不需要偏转，则操作完成
        Quaternion rotationGoal = Quaternion.Euler(0f, (rotationType == 1) ? 180f : 0f, rotationZ); // 目标旋转角度
        instance.transform.rotation = Quaternion.RotateTowards(instance.transform.rotation, rotationGoal, Time.deltaTime * 300f);
        // 如果需要旋转至正面，且已转过90度，且当前还是背面显示时，更换贴图
        if (rotationType == 1 && instance.transform.eulerAngles.y >= 90f && isBack)
        {
            Picture();
        }
        // 如果需要旋转至背面，且已转过90度，且当前还是正面显示时，更换贴图
        if (rotationType == 2 && instance.transform.eulerAngles.y <= 90f && !isBack)
        {
            PictureBack();
        }
        return instance.transform.rotation.Equals(rotationGoal);
    }

    // 给卡牌贴图（由子类重载）
    protected abstract void Picture();

    // 还原卡牌背面（由子类重载）
    protected abstract void PictureBack();
}
