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
public class tracka : MonoBehaviour, ICloudRecoEventHandler
{
    private CloudRecoBehaviour mCloudRecoBehaviour;
    private bool mIsScanning = false;
    private string mTargetMetadata = "";
    private bool reproducing = false;
    public Button m_PlayButton;
    private String id ="";
    // Use this for initialization
    public ImageTargetBehaviour ImageTargetTemplate, instance;
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
            var tracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
            tracker.TargetFinder.ClearTrackables(false);
        }
    }

    // Here we handle a cloud target recognition event
    public void OnNewSearchResult(TargetFinder.TargetSearchResult targetSearchResult)
    {
        //TargetFinder.TargetSearchResult cloudRecoSearchResult = (TargetFinder.TargetSearchResult)targetSearchResult;
        // do something with the target metadata
        //mTargetMetadata = cloudRecoSearchResult.MetaData;
        // stop the target finder (i.e. stop scanning the cloud)
        //mCloudRecoBehaviour.CloudRecoEnabled = false;
        // Build augmentation based on target

        ImageTargetBehaviour cloneImageTargetBehaviour = Instantiate(ImageTargetTemplate);

        if (cloneImageTargetBehaviour)
        {
            // enable the new result with the same ImageTargetBehaviour:
            ObjectTracker tracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
            instance = (ImageTargetBehaviour)tracker.TargetFinder.EnableTracking(targetSearchResult, cloneImageTargetBehaviour.gameObject);
            this.id = targetSearchResult.UniqueTargetId;
            //fillData(new JSONObject(""));
            StartCoroutine(GetFilmData(this.id));
        }
    }

    private void fillData(JSONObject json)
    {
        TextMeshPro title = instance.transform.Find("Canvas/RightPanel/Title").GetComponent<TextMeshPro>();
        title.text = json.GetField("name").str;
        
        TextMeshPro description = instance.transform.Find("Canvas/RightPanel/Description").GetComponent<TextMeshPro>();
        description.text = "Sinopsis: " + json.GetField("description").str;

        TextMeshPro puntuacion = instance.transform.Find("Canvas/Puntuacion").GetComponent<TextMeshPro>();
        puntuacion.text = json.GetField("valoration") + "/10";
        puntuacion.color = Color.yellow;
    }

    IEnumerator GetFilmData(String id)
    {
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
        comunication("plan", this.id );
    }
    public void likeClick()
    {
        comunication("like", this.id );
    }
    public void shareClick()
    {
        comunication("share", this.id );
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