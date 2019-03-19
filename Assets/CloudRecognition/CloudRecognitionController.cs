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
    //private String id = "";
    private JSONObject jsonDetectedObject;
    // Original copy
    public ImageTargetBehaviour imageTargetTemplate;
    // Instances for th original
    private ImageTargetBehaviour instance;
    private Boolean expand_clicked = false;
    private String idDetected;

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

            this.idDetected = targetSearchResult.UniqueTargetId;


            GetData(this.idDetected);

        }
    }

    private void setControllers()
    {
        /*Canvas leftPanel = instance.transform.Find("Canvas/LeftPanel").GetComponent<Canvas>();
        
        Canvas rightPanel = instance.transform.Find("Canvas/RightPanel").GetComponent<Canvas>();

        Utility.showHide(leftPanel.gameObject, false);
        Utility.showHide(rightPanel.gameObject, false);
        */
        /*Button leftArrow = instance.transform.Find("Canvas/LeftArrowButton").GetComponent<Button>();
        Button rightArrow = instance.transform.Find("Canvas/RightArrowButton").GetComponent<Button>();

        leftArrow.onClick.AddListener(() =>
        {
            Utility.showHide(leftPanel.gameObject);
            leftArrow.transform.Rotate(0, 0, 180);
        });
        rightArrow.onClick.AddListener(() =>
        {
            Utility.showHide(rightPanel.gameObject);
            rightArrow.transform.Rotate(0, 0, 180);
        });*/

        /*TextMeshPro title = instance.transform.Find("Canvas/RightPanel/Title").GetComponent<TextMeshPro>();
        Utility.showHide(title.gameObject, false);

        TextMeshPro description = instance.transform.Find("Canvas/RightPanel/Description").GetComponent<TextMeshPro>();
        Utility.showHide(description.gameObject, false);*/
        RectTransform filmPanel = instance.transform.Find("Canvas/FilmPanel").GetComponent<RectTransform>();
        //TextMeshPro punctuation = instance.transform.Find("Canvas/FilmPanel/Image/Punctuation").GetComponent<TextMeshPro>();
        Utility.showHide(filmPanel.gameObject, false);
        RectTransform userPanel = instance.transform.Find("Canvas/UserPanel").GetComponent<RectTransform>();
        Utility.showHide(userPanel.gameObject, false);
    }

    private void fillUserData(JSONObject json) {
        RectTransform userPanel = instance.transform.Find("Canvas/UserPanel").GetComponent<RectTransform>();
        Utility.showHide(userPanel.gameObject, true);

        TextMeshPro userName = instance.transform.Find("Canvas/UserPanel/UserName").GetComponent<TextMeshPro>();
        String name = json.GetField("name").str;
        userName.text = name;
        friends();

        /*if (friends())
        {
            Button addFriend = instance.transform.Find("Canvas/UserPanel/ButtonAddFriend").GetComponent<Button>();
            Utility.showHide(addFriend.gameObject, false);
        }*/
    }
    private void fillNoFriends()
    {
        Button addFriend = instance.transform.Find("Canvas/UserPanel/ButtonAddFriend").GetComponent<Button>();
        Utility.showHide(addFriend.gameObject, false);
    }

    private void fillFilmData(JSONObject json)
    {
      /*VideoPlayer videoPlayer = instance.transform.Find("Canvas/LeftPanel/TrailerUrl").GetComponent<VideoPlayer>();
        if (json.GetField("trailer").type != JSONObject.Type.NULL) {
            videoPlayer.url = json.GetField("trailer").str;
        }

        TextMeshPro title = instance.transform.Find("Canvas/RightPanel/Title").GetComponent<TextMeshPro>();
        Utility.showHide(title.gameObject, true);
        title.text = json.GetField("name").str;

        TextMeshPro description = instance.transform.Find("Canvas/RightPanel/Description").GetComponent<TextMeshPro>();
        Utility.showHide(description.gameObject, true);
        description.text = "Sinopsis: " + json.GetField("description").str;*/

        RectTransform filmPanel = instance.transform.Find("Canvas/FilmPanel").GetComponent<RectTransform>();
        Utility.showHide(filmPanel.gameObject, true);

        //Canvas userCanvas = instance.transform.Find("UserCanvas").GetComponent<Canvas>();
        //Utility.showHide(userCanvas.gameObject.gameObject, true);

        TextMeshPro punctuation = instance.transform.Find("Canvas/FilmPanel/Punctuation").GetComponent<TextMeshPro>();

        //hide button that will be shown when clicking on expand button (+)
        Button infoButton = instance.transform.Find("Canvas/FilmPanel/InfoButton").GetComponent<Button>();
        Utility.showHide(infoButton.gameObject, false);

        Button saveButton = instance.transform.Find("Canvas/FilmPanel/SaveButton").GetComponent<Button>();
        Utility.showHide(saveButton.gameObject, false);

        Button expandButton = instance.transform.Find("Canvas/FilmPanel/ExpandButton").GetComponent<Button>();
        Utility.showHide(expandButton.gameObject, true);

        float valoration = json.GetField("rating").n;
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

    private void GetData(String id)
    {
        // Rest GET to get data about the film
        using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                jo.Call("DAOController", "getFilmById", this.idDetected);
                jo.Call("DAOController", "getUserById", this.idDetected);

            }
        }
        /*comunication("info", this.jsonDetectedObject.str);
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
                this.jsonDetectedObject = new JSONObject(www.downloadHandler.text);
                Debug.Log(jsonDetectedObject.ToString());
                if (jsonDetectedObject.ToString() != "null")
                    fillFilmData(jsonDetectedObject);
            }
        }
        if (this.jsonDetectedObject.ToString() == "null")
        {
            Debug.Log("DETECTED USER!!!");
            using (UnityWebRequest www = UnityWebRequest.Get("http://tfg-spring.herokuapp.com/user/" + id))
            {
                yield return www.SendWebRequest();
                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    Debug.Log("Received: " + www.downloadHandler.text);
                    this.jsonDetectedObject = new JSONObject(www.downloadHandler.text);
                    Debug.Log(jsonDetectedObject.ToString());
                    if (jsonDetectedObject.ToString() != "null")
                        fillUserData(jsonDetectedObject);
                }
            }
        }*/
        
    }
    void OnGUI()
    {

    }
    public void saveClick()
    {
        comunication("save", this.jsonDetectedObject.GetField("uuid").str);
    }
    public void expandClick()
    {

        Button infoButton = instance.transform.Find("Canvas/FilmPanel/InfoButton").GetComponent<Button>();
        Button saveButton = instance.transform.Find("Canvas/FilmPanel/SaveButton").GetComponent<Button>();
        if (!this.expand_clicked)
        {
            //show buttons when clicking first time on expand button (+)
            Utility.showHide(infoButton.gameObject, true);
            Utility.showHide(saveButton.gameObject, true);
            this.expand_clicked = true;
        } else {
            //hide buttons when clicking second time on expand button (+)
            Utility.showHide(infoButton.gameObject, false);
            Utility.showHide(saveButton.gameObject, false);
            this.expand_clicked = false;
        }
    }
   
    public void infoClick()
    {
        comunication("info", this.jsonDetectedObject.str);
    }
    public void recibeInfoFilm(String info)
    {
        Debug.Log("Estoy en unity (film) y he recibido " + info);
        this.jsonDetectedObject = new JSONObject(info);
        fillFilmData(this.jsonDetectedObject);
    }
    public void recibeInfoUser(String info)
    {
        Debug.Log("Estoy en unity (user) y he recibido " + info);
        this.jsonDetectedObject = new JSONObject(info);
        fillUserData(this.jsonDetectedObject);
    }
    public void youtubeClick()
    {
        comunication("youtube", this.jsonDetectedObject.str);
    }
    public void addFriendClick()
    {
        comunication("addFriend","");
    }
    private void comunication(String method, String _id)
    {
        using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                jo.Call(method,this.jsonDetectedObject.ToString());

            }
        }
    }
    public void exitARClick()
    {
        changeActivity("exitAR");
    }
    public void goPlans()
    {
        changeActivity("goPlans");
    }
    public void goSaves()
    {
        changeActivity("goSaves");
    }
    private void changeActivity(string method)
    {
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");

        jo.Call(method);
    }

    private void friends()
    {
        using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                jo.Call("areFriends", this.jsonDetectedObject.str);

            }
        }
    }
    public void recibeInfoFriends(String info)
    {
        if (string.Equals(info, "true"))
        {
            //Son amigos
            Debug.Log("Son amigos");
        }
        else
        {
            fillNoFriends();
            Debug.Log("No son amigos");
        }
    }

}