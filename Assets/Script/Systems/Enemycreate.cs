//这是一个掌控刷怪的脚本
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemycreate : MonoBehaviour
{
    public GameObject closeenemy;
    public GameObject Midcloseenemy;
    public GameObject Highcloseenemy;
    public GameObject farenemy;
    public GameObject Midfarenemy;
    public GameObject Highfarenemy;
    public float spawnRate = 2;
    private float timer = 0;
    private float a,b;//time的载体
    private bool N=false;
    [Header("每波数量为time/spawnRate")]
    public float time;//数量为time/spawnRate
    [Header("波次间隔时间")]
    public float middletime;//波次间隔时间
    [Header("生成近战怪物的概率，要小于等于1哦QAQ")]
    public float P;
    [Header("生成普通近战怪物的概率")]
    public float A;
    [Header("生成mid近战怪物的概率")]
    public float B;
    [Header("生成远程怪物的概率（远程怪物间的概率）")]
    public float C;
    [Header("生成Mid远程怪物的概率（远程怪物间的概率）")]
    public float D;

    // Start is called before the first frame update
    void Start()
    {
        middletime = Random.Range(0.5f, 1.5f);
        time = Random.Range(0.5f, 1.5f);
        a = time;
        b = middletime;
    }

    // Update is called once per frame
    void Update()
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
                        time = a;
                    }
                }
            }
            else
            {
                shengcheng();
                timer = 0;
            middletime = Random.Range(0.5f, 1.5f);
            time = Random.Range(0.5f, 1.5f);
            a = time;
            b = middletime;
        }
        }
    void shengcheng()
    {
        
            float r1 = Random.value;
            float r2 = Random.value;
            float r3 = Random.value;
            float r4 = Random.value;
            if (r1 < P)
            {
                if (r2 < A)
                {
                    Instantiate(closeenemy, transform.position, Quaternion.identity);
                }
                else if (r2 < B)
                {
                    Instantiate(Midcloseenemy, transform.position, Quaternion.identity);
                }
                     else Instantiate(Highcloseenemy, transform.position, Quaternion.identity);
            }
            else if(N==false)
            {
                if (r3 < C)
                {
                    Instantiate(farenemy, transform.position, Quaternion.identity);
                }
                else if (r4 < D)
                {
                    Instantiate(Midfarenemy, transform.position, Quaternion.identity);
                }
                     else Instantiate(Highfarenemy, transform.position, Quaternion.identity);
                N = true;
            }
        
    }
 }

