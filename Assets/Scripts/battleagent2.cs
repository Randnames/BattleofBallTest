using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using GlobalSettings;
public class battleagent2 : Agent
{
    public Ball player;
    public Ball otherplayer;
    public GameObject other;
    //public Plane plane;
    private RayPerception3D rayPer;
    public battleacademy2 aca;
    float[] rayAngles;
    SortedDictionary<float, Food> fooddic;
    public override void InitializeAgent()
    {
        
        
        rayPer = GetComponent<RayPerception3D>();
        fooddic = new SortedDictionary<float, Food>();
        rayAngles = new float[36];
        for(int i = 0; i < 36; i++)
        {
            rayAngles[i] = i * 10;
        }
    }
    public float maxvec = 500.0f;
    public override void CollectObservations()
    {
        //6+5+120=131
        fooddic.Clear();

        AddVectorObs((-player.position.x + otherplayer.position.x)/maxvec);
        AddVectorObs((-player.position.z + otherplayer.position.z)/maxvec);
        AddVectorObs(player.direction.x/player.maxVelocity);
        AddVectorObs(player.direction.z/player.maxVelocity);
        AddVectorObs(player.velocity/player.maxVelocity);
        AddVectorObs((player.radius- otherplayer.radius)>=0?1:-1);
        //Debug.Log(other);
        AddVectorObs(player.position.x/PlaneData.Width);
        AddVectorObs(player.position.z/PlaneData.Height);
        AddVectorObs(otherplayer.direction.x/otherplayer.maxVelocity);
        AddVectorObs(otherplayer.direction.z / otherplayer.maxVelocity);
        AddVectorObs(otherplayer.velocity / otherplayer.maxVelocity);
        //AddVectorObs(otherplayer.radius);
        for(int i = 0; i < aca.foods.Count; i++)
        {
            Food food = aca.foods[i];
            float distance = Vector3.Distance(player.position, food.position);
            if (fooddic.ContainsKey(distance))
            {
                float ttt=Random.value / 20;
                while (fooddic.ContainsKey(distance + ttt))
                {
                    ttt = Random.value / 20;
                }
                distance += ttt;
            }
            fooddic.Add(distance,food);
        }
        int ind = 0;
        //Debug.Log(fooddic.Count);
        foreach(var tmp in fooddic)
        {
            if (ind >= 40) break;
            ind++;
            AddVectorObs((tmp.Value.position.x- player.position.x)/maxvec);
            AddVectorObs((tmp.Value.position.z- player.position.z)/maxvec);
            AddVectorObs(tmp.Value.scale.x/ BallData.MinRadius);
        }
        for (; ind < 40; ind++)
        {
            AddVectorObs(0);
            AddVectorObs(0);
            AddVectorObs(0);
        }
    }
    int steps = 0;

    float movrew(Vector3 position,Vector3 dir,float radius)
    {
        float res=0;
        if (position.x + dir.x < -PlaneData.Width / 2 + radius)
            res += -0.0001f;

        if (position.x + dir.x + radius > PlaneData.Width / 2)
            res += -0.0001f;

        if (position.z + dir.z < -PlaneData.Height / 2 + radius)
            res += -0.0001f;

        if (position.z + dir.z + radius > PlaneData.Height / 2)
            res += -0.0001f;
        return res;
    }
    float tmpx=0, tmpy=0;
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        transform.position = player.position;
        //Debug.Log(vectorAction[0]);

        //continuous space
        Vector3 direction = new Vector3(vectorAction[0], 0, vectorAction[1]);
        if (tmpx * vectorAction[0] < 0 || tmpy * vectorAction[1] < 0)
            AddReward(-0.0002f);
        tmpx = vectorAction[0];
        tmpy = vectorAction[1];
        //discrete space
        //0--don't move, 1--dec, 2--inc
        //int dirX=Mathf.FloorToInt(vectorAction[0]);
        //int dirY=Mathf.FloorToInt(vectorAction[1]);
        //dirX=dirX==0?0:dirX==1?-1:1;
        //dirY=dirY==0?0:dirY==1?-1:1;
        //Vector3 direction = new Vector3(dirX, 0,dirY);

        Vector3 dir = -player.position + otherplayer.position;
		float dirdot = Vector3.Dot(dir.normalized,direction.normalized)/5000;
		float radiusdif = player.radius - otherplayer.radius;
		float rew = radiusdif * dirdot;

        float mover = movrew(player.position, direction, player.radius);

		AddReward(mover-0.0002f);

        if (steps >= 100000)
        {
            steps = 0;
            Done();
            aca.Done();
            return;
        }
        steps++;

        player.MoveTowards(direction);
        player.UpdatePosition();

        for (int k = 0; k < aca.foods.Count; k++)
        {
            Food food = aca.foods[k];
            float distance0 = Vector3.Distance(player.position, food.position);
            if (distance0 < player.radius+food.scale.x/2)
            {
                //Debug.Log("eat");
                AddReward(food.volume/50);
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
                AddReward(1);
                aca.Done();
            }
            else
            {
                SetReward(-1);
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
        otherplayer = other.GetComponent<battleagent2>().player;
        transform.position = player.position;
        steps = 0;
        tmpx = 0;
        tmpy = 0;
    }
}
