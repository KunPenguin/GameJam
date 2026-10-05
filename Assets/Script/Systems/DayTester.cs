using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DayTester : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("天数+1，当前：" + GameData.Instance.currentDay);
            SceneManager.LoadScene("DayTransitionScence");
        }
    }
}
