using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using GlobalSettings;
public class battleacademy2 : Academy
{
    // Start is called before the first frame update
    float produceFoodsTimer = 0;
    public GameObject p1, p2;
    public List<Ball> balls;
    public List<Food> foods;
    public GameObject plane;
    public List<Food> deadFoodList;
    public void ProduceRandomFoods(float produceInterval, Vector3 planeScale, float quantity)
    {
        produceFoodsTimer += Time.deltaTime;
        if (foods.Count <= 50)
        {
            quantity = 50;
        }
        else
        {
            quantity = 0;
        }
        if (produceFoodsTimer > produceInterval)
        {
            

            for (int i = 0; i < quantity; i++)
            {
                // 随机取一个颜色
                Color randomColor = new Color(Random.value, Random.value, Random.value);

                // 随机生成大小
                float randomSideLength = Random.Range(FoodData.MinSideLength, FoodData.MaxSideLength);

                // 随机生成地点(不能超过平面范围)
                float randomPositionX = Random.Range(-planeScale.x / 2 + randomSideLength / 2, planeScale.x / 2 - randomSideLength / 2);
                float randomPositionZ = Random.Range(-planeScale.z / 2 + randomSideLength / 2, planeScale.z / 2 - randomSideLength / 2);
                Vector3 randomPosition = new Vector3(randomPositionX, 0, randomPositionZ);

                // 生成一个随机的Food并将其加入foods队列中
                Food newFood = new Food(randomColor, randomSideLength, randomPosition);
                foods.Add(newFood);
            }

            // 将计时器归零
            produceFoodsTimer = 0;
        }
    }
    public override void InitializeAcademy()
    {
        base.InitializeAcademy();
        balls = new List<Ball>();
        p1.GetComponent<battleagent2>().player = new Ball(Color.cyan, BallData.DefaultRadius, null, Vector3.zero, true);
        p2.GetComponent<battleagent2>().player = new Ball(Color.cyan, BallData.DefaultRadius, null, Vector3.zero, true);

        foods = new List<Food>();
        deadFoodList = new List<Food>();
    }
    public override void AcademyReset()
    {
        Debug.Log("aca reset");
        
        foreach(var tmp in foods)
        {
            tmp.BeEaten();
        }
        foreach(var tmp in deadFoodList)
        {
            tmp.BeEaten();
        }
        foods = new List<Food>();
        deadFoodList = new List<Food>();
        Vector3 planeScale = new Vector3(PlaneData.Width, 0, PlaneData.Height);
        ProduceRandomFoods(FoodData.ProduceInterval, planeScale, FoodData.DefaultProduceQuantity);
        balls.Clear();
        balls.Add(p1.GetComponent<battleagent2>().player);
        balls.Add(p2.GetComponent<battleagent2>().player);
    }

    public override void AcademyStep()
    {
        Vector3 planeScale = new Vector3(PlaneData.Width, 0, PlaneData.Height);
        ProduceRandomFoods(FoodData.ProduceInterval, planeScale, FoodData.DefaultProduceQuantity);
        foreach (var tmp in deadFoodList)
        {
            tmp.BeEaten();
        }
        deadFoodList = new List<Food>();


    }
}
