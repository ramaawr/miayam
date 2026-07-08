using UnityEngine;
using UnityEngine.EventSystems;


public class testingsingleton : MonoBehaviour
{


    static testingsingleton instance;
    private static bool applicationIsQuitting = false;
    public EventSystem uiEventSystem;
    public int targetFrameRate = 60;

    /*
    public static testingsingleton Instance
    {
        get
        {
            // Debug.Log($"Get Essentials Called applicationIsQuitting {applicationIsQuitting}");
            if (applicationIsQuitting) return null;
            if (instance == null)
            {
                var search = FindObjectsByType<testingsingleton>(FindObjectsSortMode.None);
                if (search.Length > 0)
                {
                    if (search[0] is testingsingleton) instance = search[0] as testingsingleton;
                }

                var prefab = Resources.Load("testingsingleton") as GameObject;
                var newObject = Instantiate(prefab);
                instance = newObject.GetComponent<testingsingleton>();
            }

            return instance;
        }
    }
    */


    private void Awake()
    {
        if (instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }

        applicationIsQuitting = false;
        instance = this;
        Application.targetFrameRate = targetFrameRate;
        Screen.SetResolution(1920, 1080, true);
        DontDestroyOnLoad(gameObject);
        //gameConfigs.Initialize();


    }

}