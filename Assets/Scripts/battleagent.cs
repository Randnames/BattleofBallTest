using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using GlobalSettings;
public class battleagent : Agent
{
    public Ball player;
    public Ball otherplayer;
    public GameObject other;
    //public Plane plane;
    private RayPerception3D rayPer;
    public battleacademy aca;
    float[] rayAngles;
    public override void InitializeAgent()
    {
        
        
        rayPer = GetComponent<RayPerception3D>();
        rayAngles = new float[36];
        for(int i = 0; i < 36; i++)
        {
            rayAngles[i] = i * 10;
        }
    }
    public override void CollectObservations()
    {
        //6+6+180=192
        AddVectorObs(player.position.x);
        AddVectorObs(player.position.z);
        AddVectorObs(player.direction.x);
        AddVectorObs(player.direction.z);
        AddVectorObs(player.velocity);
        AddVectorObs(player.radius- otherplayer.radius);
        //Debug.Log(other);
        AddVectorObs(otherplayer.position.x);
        AddVectorObs(otherplayer.position.z);
        AddVectorObs(otherplayer.direction.x);
        AddVectorObs(otherplayer.direction.z);
        AddVectorObs(otherplayer.velocity);
        //AddVectorObs(otherplayer.radius);

        float rayDistance = 500f;
        string[] detectableObjects = { "Food", "Player", "Wall" };
        //36*5=180
        AddVectorObs(rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        transform.position = player.position;
        Vector3 direction = new Vector3(vectorAction[0], 0,vectorAction[1]);
        AddReward(-0.1f);
        player.MoveTowards(direction);
        player.UpdatePosition();

        for (int k = 0; k < aca.foods.Count; k++)
        {
            Food food = aca.foods[k];
            float distance0 = Vector3.Distance(player.position, food.position);
            if (distance0 < player.radius+food.scale.x/2)
            {
                //Debug.Log("eat");
                AddReward(food.volume);
                aca.deadFoodList.Add(food);
                player.Eat(food);
                //food.BeEaten();
            }
        }
        for (int i = aca.deadFoodList.Count-1; i >=0; i--)
        {
            aca.foods.Remove(aca.deadFoodList[i]);
        }
        float radiusCompare = player.radius - otherplayer.radius;
        // 如果两个球大小相等，则不做判断
        if (System.Math.Abs(radiusCompare) < BallData.RadiusTolerance)
            return;
        Ball bigBall = (radiusCompare > 0) ? player : otherplayer;
        Ball smallBall = (bigBall == player) ? otherplayer : player;
        float distance = Vector3.Distance(player.position, otherplayer.position);
        if (distance < bigBall.radius)
        {
            Done();
            //Debug.Log("done");
            if (bigBall == player)
            {
                AddReward(otherplayer.volume);
                aca.Done();
            }
            else
            {
                AddReward(-player.volume);
            }
        }
        
    }

    public override void AgentReset()
    {
        float randomRadius = Random.Range(BallData.MinRadius, BallData.MaxRadius);
        Vector3 planeScale = new Vector3(PlaneData.Width, 0, PlaneData.Height);
        float randomPositionX = Random.Range(-planeScale.x / 2 + randomRadius, planeScale.x / 2 - randomRadius);
        float randomPositionZ = Random.Range(-planeScale.z / 2 + randomRadius, planeScale.z / 2 - randomRadius);
        Vector3 randomPosition = new Vector3(randomPositionX, 0, randomPositionZ);
        player.position = randomPosition;
        player.radius = BallData.DefaultRadius;
        otherplayer = other.GetComponent<battleagent>().player;
        transform.position = player.position;
    }
}
