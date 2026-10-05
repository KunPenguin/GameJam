//这是一个掌控刷怪的脚本
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemycreate : MonoBehaviour
{
    public GameObject enemy;
    public float spawnRate = 2;
    private float timer = 0;
    private float a,b;//time的载体
    [Header("波次")]
    public float n = 5;//波次
    [Header("每波数量为time/spawnRate")]
    public float time = 10;//数量为time/spawnRate
    [Header("波次间隔时间")]
    public float middletime =5;//波次间隔时间
    // Start is called before the first frame update
    void Start()
    {
        a = time;
        b = middletime;
        Instantiate(enemy, transform.position, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        if (n >= 1)
        {
            if (timer < spawnRate)
            {
                if (time >= 0)
                {
                    timer = timer + Time.deltaTime;
                }
                time = time - Time.deltaTime;
                if (time <= 0)
                {
                     middletime = middletime - Time.deltaTime;
                    if (middletime <= 0)
                    {
                        middletime = b;
                        n--;
                        time = a;
                    }
                }
            }
            else
            {
                shengcheng();
                timer = 0;
            }
        }
     }
        void shengcheng()
        {
            Instantiate(enemy, transform.position, Quaternion.identity);
        }

 }

