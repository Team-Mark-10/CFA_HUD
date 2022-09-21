using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 

public class NotificationManager : MonoBehaviour
{
   public static NotificationManager Instance 
   {
       get {
           if(instance != null)
           {
               return instance; 
           }
           instance = FindObjectOfType <NotificationManager>(); 

           if(instance != null)
           {
               return instance; 
           }

           CreateNewInstance();

           return instance; 
       }
   }
 public static NotificationManager CreateNewInstance()
   {
       NotificationManager notificationManagerPrefab = resource.Load<NotificationManager>("NotificationManager");
       instance = Instantiate(notificationManagerPrefab);

       return instance;
   }

 
    private static NotificationManager instance; 

    private voide Awake()
    {
        if(Instance != this)
        {
            Destory(gameObject);
        }
    }

    [SerializeField] private TextMeshProUGUI notificationText; 
    [SerializeField] private float fadeTime; 


    private IEnumerator notficationCouroutine; 



    public void SetNotification(string message)
    {
        if(notficationCouroutine !=null)
        {
            StopCouroutine(notficationCouroutine);
        }
        notficationCouroutine = FadeOUtNotification(message); 
        StartCouroutine(notficationCouroutine)
    }
    

    private IEnumerator FadeOUtNotification(string message){
        notificationText.text = message; 
        float t = 0; 
        while(t < fadeTime)
        {
            t += Time.unscaledDeltaTime; 
            notificationText.color = new Color(
            notificationText.color.r, 
            notificationText.color.g,
            notificationText.color.b,
            Mathf.Lerp(1f, 0f, t /fadeTime));
            yield return null; 
        }
    }
    
   
}
