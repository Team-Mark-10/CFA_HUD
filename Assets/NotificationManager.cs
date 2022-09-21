using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NotificationManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float fadeTime;

    public static NotificationManager Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }
            instance = FindObjectOfType<NotificationManager>();

            if (instance != null)
            {
                return instance;
            }

            CreateNewInstance();

            return instance;
        }
    }

    private static NotificationManager instance;

    private IEnumerator notficationCouroutine;

    public static NotificationManager CreateNewInstance()
    {
        NotificationManager notificationManagerPrefab = Resources.Load<NotificationManager>("NotificationManager");
        instance = Instantiate(notificationManagerPrefab);

        return instance;
    }

    private void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void SetNewNotification(string message)
    {
        if (notficationCouroutine != null)
        {
            StopCoroutine(notficationCouroutine);
        }
        notficationCouroutine = FadeOutMessage(message);
        StartCoroutine(notficationCouroutine);
    }


    private IEnumerator FadeOutMessage(string message)
    {
        notificationText.text = message;
        float t = 0;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            notificationText.color = new Color(
            notificationText.color.r,
            notificationText.color.g,
            notificationText.color.b,
            Mathf.Lerp(1f, 0f, t / fadeTime));
            yield return null;
        }
    }


}