using Vuforia;
using UnityEngine;
using TMPro;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Text;
using System;
public class CloudRecognitionController : MonoBehaviour, ICloudRecoEventHandler
{
    private CloudRecoBehaviour mCloudRecoBehaviour;
    private bool mIsScanning = false;
    // Last id for the image recognized
    private String id = "";
    // Original copy
    public ImageTargetBehaviour imageTargetTemplate;
    // Instances for th original
    private ImageTargetBehaviour instance;
    void Start()
    {
        // register this event handler at the cloud reco behaviour
        mCloudRecoBehaviour = GetComponent<CloudRecoBehaviour>();

        if (mCloudRecoBehaviour)
        {
            mCloudRecoBehaviour.RegisterEventHandler(this);
        }
    }

    public void OnInitialized(TargetFinder TargetFinder)
    {
        Debug.Log("Cloud Reco initialized");
    }
    public void OnInitError(TargetFinder.InitState initError)
    {
        Debug.Log("Cloud Reco init error " + initError.ToString());
    }
    public void OnUpdateError(TargetFinder.UpdateState updateError)
    {
        Debug.Log("Cloud Reco update error " + updateError.ToString());
    }

    public void OnStateChanged(bool scanning)
    {
        mIsScanning = scanning;
        if (scanning)
        {
            // clear all known trackables
            ObjectTracker tracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
            tracker.TargetFinder.ClearTrackables(false);
        }
    }

    // Here we handle a cloud target recognition event
    public void OnNewSearchResult(TargetFinder.TargetSearchResult targetSearchResult)
    {
        // We do clone an instance for each image recognized
        ImageTargetBehaviour cloneImageTargetBehaviour = Instantiate(imageTargetTemplate);

        if (cloneImageTargetBehaviour)
        {
            // enable the new result with the same ImageTargetBehaviour:
            ObjectTracker tracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
            
            instance = (ImageTargetBehaviour)tracker.TargetFinder.EnableTracking(targetSearchResult, cloneImageTargetBehaviour.gameObject);

            setControllers();

            this.id = targetSearchResult.UniqueTargetId;
            StartCoroutine(GetFilmData(this.id));
        }
    }

    private void setControllers()
    {
        Canvas leftPanel = instance.transform.Find("Canvas/LeftPanel").GetComponent<Canvas>();
        Canvas rightPanel = instance.transform.Find("Canvas/RightPanel").GetComponent<Canvas>();

        Utility.showHide(leftPanel.gameObject, false);
        Utility.showHide(rightPanel.gameObject, false);

        Button leftArrow = instance.transform.Find("Canvas/LeftArrowButton").GetComponent<Button>();
        Button rightArrow = instance.transform.Find("Canvas/RightArrowButton").GetComponent<Button>();

        leftArrow.onClick.AddListener(() =>
        {
            Utility.showHide(leftPanel.gameObject);
        });
        rightArrow.onClick.AddListener(() =>
        {
            Utility.showHide(rightPanel.gameObject);
        });

        TextMeshPro title = instance.transform.Find("Canvas/RightPanel/Title").GetComponent<TextMeshPro>();
        Utility.showHide(title.gameObject, false);

        TextMeshPro description = instance.transform.Find("Canvas/RightPanel/Description").GetComponent<TextMeshPro>();
        Utility.showHide(description.gameObject, false);

        TextMeshPro punctuation = instance.transform.Find("Canvas/Punctuation").GetComponent<TextMeshPro>();
        Utility.showHide(punctuation.gameObject, false);
    }

    private void fillData(JSONObject json)
    {
        TextMeshPro title = instance.transform.Find("Canvas/RightPanel/Title").GetComponent<TextMeshPro>();
        Utility.showHide(title.gameObject, true);
        title.text = json.GetField("name").str;

        TextMeshPro description = instance.transform.Find("Canvas/RightPanel/Description").GetComponent<TextMeshPro>();
        Utility.showHide(description.gameObject, true);
        description.text = "Sinopsis: " + json.GetField("description").str;

        TextMeshPro punctuation = instance.transform.Find("Canvas/Punctuation").GetComponent<TextMeshPro>();
        Utility.showHide(punctuation.gameObject, true);

        float valoration = json.GetField("valoration").n;
        punctuation.text = valoration + "/10";

        punctuation.color = Color.red;
        if (valoration >= 5)
        {
            punctuation.color = Color.yellow;
        }
        if (valoration >= 8)
        {
            punctuation.color = Color.green;
        }
        if (valoration >= 10)
        {
            punctuation.color = Color.white;
        }

    }

    private IEnumerator GetFilmData(String id)
    {
        // Rest GET to get data about the film
        using (UnityWebRequest www = UnityWebRequest.Get("http://tfg-spring.herokuapp.com/film/" + id))
        {
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Received: " + www.downloadHandler.text);
                fillData(new JSONObject(www.downloadHandler.text));
            }
        }
    }
    void OnGUI()
    {

    }
    public void planClick()
    {
        comunication("plan", this.id);
    }
    public void likeClick()
    {
        comunication("like", this.id);
    }
    public void shareClick()
    {
        comunication("share", this.id);
    }
    private void comunication(String method, String _id)
    {
        using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                jo.Call(method, _id);
            }
        }
    }
}