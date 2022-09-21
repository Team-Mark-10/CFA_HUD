using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestNotification : MonoBehaviour
{
   private void Start() 
   {
       NotificationManager.Instance.SetNewNotification("Test Test Test");
   }

   private void Update() 
   {
    if(Time.time > 5f)
    {
        NotificationManager.Instance.SetNewNotification("coding is cool");
        Destory(gameObject); 
    }
   }
}
