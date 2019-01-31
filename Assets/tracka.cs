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

            fillData(new JSONObject(""));
            //StartCoroutine(GetFilmData(targetSearchResult.UniqueTargetId));
        }
    }

    private void fillData(JSONObject json)
    {
        TextMeshPro text = instance.transform.Find("Title").GetComponent<TextMeshPro>();
        text.text = instance.TrackableName;

        VideoPlayer videoPlayer = instance.transform.Find("Trailer").GetComponent<VideoPlayer>();

        Button playButton = instance.transform.Find("Canvas/PlayButton").GetComponent<Button>();
        playButton.onClick.AddListener(() =>
        {
            videoPlayer.Play();
        });

        Button stopButton = instance.transform.Find("Canvas/StopButton").GetComponent<Button>();
        stopButton.onClick.AddListener(() =>
        {
            videoPlayer.Stop();
        });
    }

    IEnumerator GetFilmData(String id)
    {
        WWWForm form = new WWWForm();
        form.AddField("uuid", id);

        using (UnityWebRequest www = UnityWebRequest.Post("https://filmar.herokuapp.com/", form))
        {
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                fillData(new JSONObject(www.downloadHandler.text));
                Debug.Log("Received: " + www.downloadHandler.text);
            }
        }
    }

    void OnGUI()
    {

    }
}