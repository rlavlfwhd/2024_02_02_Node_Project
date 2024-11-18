using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTest : MonoBehaviour
{
    public int Hp = 0;

    void Start()
    {
        for(int i = 0; i < 10; i++)
        {
            HealCounter();
        }
    }

    public void HealCounter()
    {
        Hp += 10;
    }
        
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Hp -= 5;
        }


    }
}
