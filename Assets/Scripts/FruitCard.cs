using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitCard : Card
{
    // 贴图的位置，见“卡牌种类及位置分布.xlsx”
    private static int[][] picturePos = new int[5][]
    {
        new int[] {5},
        new int[] {2, 8},
        new int[] {3, 5, 7},
        new int[] {1, 3, 7, 9},
        new int[] {1, 3, 5, 7, 9}
    };

    private Material[] fruitMaterial = new Material[4]; // 4种水果的材质
    private int fruitNumber; // 这张卡牌上的水果数量
    private int[] fruitKind; // 第i个水果的水果类型（0~3代表A~D即红黄蓝绿4种水果），见“卡牌种类及位置分布.xlsx”
    
    public FruitCard(int _ID, int _cardSet, GameObject prefab) : base(_ID, _cardSet, prefab)
    {
        // 根据卡牌套加载材质，见“卡牌种类及位置分布.xlsx”
        switch (cardSet)
        {
            case 0:
                fruitMaterial[0] = Resources.Load<Material>("Materials/Fruit/0A_Peach");
                fruitMaterial[1] = Resources.Load<Material>("Materials/Fruit/0B_Banana");
                fruitMaterial[2] = Resources.Load<Material>("Materials/Fruit/0C_Grape");
                fruitMaterial[3] = Resources.Load<Material>("Materials/Fruit/0D_Kiwi");
                break;
            case 1:
                fruitMaterial[0] = Resources.Load<Material>("Materials/Fruit/1A_Strawberry");
                fruitMaterial[1] = Resources.Load<Material>("Materials/Fruit/1B_Lemon");
                fruitMaterial[2] = Resources.Load<Material>("Materials/Fruit/1C_Blueberry");
                fruitMaterial[3] = Resources.Load<Material>("Materials/Fruit/1D_Watermelon");
                break;
            case 2:
                fruitMaterial[0] = Resources.Load<Material>("Materials/Fruit/2A_Apple");
                fruitMaterial[1] = Resources.Load<Material>("Materials/Fruit/2B_Mango");
                fruitMaterial[2] = Resources.Load<Material>("Materials/Fruit/2C_Mangosteen");
                fruitMaterial[3] = Resources.Load<Material>("Materials/Fruit/2D_Carambola");
                break;
        }

        fruitNumber = Random.Range(1, 6); // 随机设定这张卡牌上的水果数量
        fruitKind = new int[fruitNumber];
        int cardKind = Random.Range(0, 2); // 随机设定这张水果牌的类型，0为随机类，1为全同类
        if (cardKind == 0)
        {
            for (int i = 0; i < fruitNumber; i++) fruitKind[i] = Random.Range(0, 4);
        }
        else
        {
            int fruit = Random.Range(0, 4); // 随机一种水果类型
            for (int i = 0; i < fruitNumber; i++) fruitKind[i] = fruit;
        }
    }
    
    // 返回这张牌上的水果数量
    public int getFruitNumber()
    {
        return fruitNumber;
    }

    // 返回下标为index的水果种类
    public int getFruitKind(int index)
    {
        return fruitKind[index];
    }

    // 给水果牌贴图
    protected override void Picture()
    {
        isBack = false;
        // 先贴上空白材质
        for (int i = 0; i < 9; i++)
        {
            instance.transform.GetChild(i).GetComponent<Renderer>().material = blankMaterial;
        }
        // 在对应位置贴上对应水果材质
        for (int i = 0; i < fruitNumber; i++)
        {
            instance.transform.GetChild(picturePos[fruitNumber - 1][i] - 1).GetComponent<Renderer>().material = fruitMaterial[fruitKind[i]];
        }
    }
    
    // 还原水果牌背面
    protected override void PictureBack() 
    {
        isBack = true;
        // 贴上卡背材质
        for (int i = 0; i < 9; i++)
        {
            instance.transform.GetChild(i).GetComponent<Renderer>().material = cardBackMaterial;
        }
    }
}
