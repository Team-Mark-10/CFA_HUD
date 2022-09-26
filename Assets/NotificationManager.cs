using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 

public class NotificationManager : MonoBehaviour
{
   public static NotificationManager Instance  
   {
       get {
           if(instance != null) //if the code does exist then it will return it 
           {
               return instance; 
           }
           instance = FindObjectOfType <NotificationManager>();  // The first active loaded object that matches the specified type. It returns null if no Object matches the type.

           if(instance != null) // might not be stored but it exists so will get and set 
           {
               return instance; 
           }

           CreateNewInstance(); //if get here it def doesnt exist so will need to create a new one 

           return instance; 
       }
   }
 public static NotificationManager CreateNewInstance()
   {
       NotificationManager notificationManagerPrefab = resource.Load<NotificationManager>("NotificationManager");
       instance = Instantiate(notificationManagerPrefab); //the type we are looking for , will filter the assets or resrouces for all the types, specifcially looking for notifcation 
//has a reference so can store and then return
       return instance;
   } 
   //prefab allows for create / store gameObject - is a template 

 
    private static NotificationManager instance; 

    private voide Awake() //opens on start up 
    {
        if(Instance != this)
        {
            Destory(gameObject); //will get rid of duplicate 
        }
    }

    [SerializeField] private TextMeshProUGUI notificationText; //pops up on opening 
    [SerializeField] private float fadeTime; //how long it takes to fade out on the screen 


    private IEnumerator notficationCouroutine; 



    public void SetNotification(string message) 
    {
        if(notficationCouroutine !=null)  
        {
            StopCouroutine(notficationCouroutine);
        }
        notficationCouroutine = FadeOUtNotification(message); 
        StartCouroutine(notficationCouroutine)
    } //if its not null then stop it which will make it null, then this will make a coutoutine to set it too, string will pass the message through 
    

    private IEnumerator FadeOUtNotification(string message){   //building the couroutine, sets the text 
        notificationText.text = message; //notification in game will be set to whatever we want it to say 
        float t = 0; // make it fade out over time 
        while(t < fadeTime)
        {
            t += Time.unscaledDeltaTime; //time is real time - will keep going if game paused etc 
            notificationText.color = new Color( //changes the colour every frame 
            notificationText.color.r, 
            notificationText.color.g,
            notificationText.color.b,
            Mathf.Lerp(1f, 0f, t /fadeTime)); // fading out 
            yield return null; // will allow it to take time over frames otherwise it will just go in one frame 
        }
    }
