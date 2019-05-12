using Vuforia;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

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
    private String idUser = "";
    private String idFilm = "";

    private struct plan_info
    {
        public ArrayList usersUrl;
        public int rate;
    }
    private Dictionary<int, plan_info> top;
    void Start()
    {
        // register this event handler at the cloud reco behaviour
        mCloudRecoBehaviour = GetComponent<CloudRecoBehaviour>();
        GameObject friendsCanvas = this.transform.Find("Canvas").GetComponent<Canvas>().gameObject;
        Utility.showHide(friendsCanvas, false);
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
            // clear all known trackablesgetUserById
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
        //We hide all the canvas at the begin waiting for know what kind of information we have received from the camera
        RectTransform filmPanel = instance.transform.Find("Canvas/FilmPanel").GetComponent<RectTransform>();
        Utility.showHide(filmPanel.gameObject, false);
        RectTransform userPanel = instance.transform.Find("Canvas/UserPanel").GetComponent<RectTransform>();
        Utility.showHide(userPanel.gameObject, false);
        RectTransform friendPanel = instance.transform.Find("Canvas/FriendPanel").GetComponent<RectTransform>();
        Utility.showHide(friendPanel.gameObject, false);
    }

    private void fillUserData() {
        //In this moment we know that is an user but we need to know if is a friend or not
        fillNoFriends();
        friends();
        
    }
    private void fillFriends(string info)
    {
        //we know that is a friend
        //we hide the user panel
        RectTransform userPanel = instance.transform.Find("Canvas/UserPanel").GetComponent<RectTransform>();
        Utility.showHide(userPanel.gameObject, false);
        //we show the friend panel
        GameObject friendsCanvas = this.transform.Find("Canvas").GetComponent<Canvas>().gameObject;
        Utility.showHide(friendsCanvas, true);
        //we load the json with the information that comes from android
        JSONObject json_def = new JSONObject(info);
        //we take and show the information of the recommendation
        if (json_def.IsArray && json_def.list.Count > 0 )
        {
            for (int i = 0; i < json_def.list.Count; i++)
            {
                StartCoroutine(cargaImagen(json_def.list[i].GetField("value2").GetField("imageURL").str, this.transform.Find("Canvas/" + i).GetComponent<UnityEngine.UI.Image>()));
            }
            for (int i = 0; i < json_def.list[0].GetField("value3").list.Count; i++)
            {
                StartCoroutine(cargaImagen(json_def.list[0].GetField("value3").list[i].GetField("imageURL").str, this.transform.Find("Canvas/0/Friend" + (i+1)).GetComponent<UnityEngine.UI.Image>()));
            }
            int rate = int.Parse(json_def.list[0].GetField("value1").GetField("rating").ToString());
            this.transform.Find("Canvas/0/Friend1/Gauge").GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>("Gauge/gauge" + rate * 10);

        }
        //we save the information to change it by pressing the arrows
        top = new Dictionary<int, plan_info>();
        for (int i = 0; i < json_def.list.Count; i++)
        {
            plan_info planAux = new plan_info();
            planAux.usersUrl = new ArrayList();
            for(int j = 0; j < json_def.list[i].GetField("value3").list.Count; j++)
            {
                planAux.usersUrl.Add(json_def.list[i].GetField("value3").list[j].GetField("imageURL").str);
            }
            json_def.list[i].GetField("value1").GetField("rating");
            planAux.rate = int.Parse(json_def.list[i].GetField("value1").GetField("rating").ToString());
            top.Add(i, planAux);
        }
        while(top.Count< 3)
        {
            plan_info planAux = new plan_info();
            planAux.usersUrl = new ArrayList();
            planAux.rate = -1;
            top.Add(top.Count, planAux);
        }   

    }
    IEnumerator cargaImagen(String url, UnityEngine.UI.Image img)
    {
        WWW www = new WWW(url);
        yield return www;
        img.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));
    }

    private void fillNoFriends()
    {
        //If is not a friend we show all the components of the no-friends canvas (user)
        RectTransform userPanel = instance.transform.Find("Canvas/UserPanel").GetComponent<RectTransform>();
        Utility.showHide(userPanel.gameObject, true);

        TextMeshPro userName = instance.transform.Find("Canvas/UserPanel/UserName").GetComponent<TextMeshPro>();
        String name = this.jsonDetectedObject.GetField("name").str;
        Utility.showHide(userName.gameObject, true);
        userName.text = name;
        Button addFriend = instance.transform.Find("Canvas/UserPanel/ButtonAddFriend").GetComponent<Button>();
        Utility.showHide(addFriend.gameObject, true);
    }

    private void fillFilmData()
    {
        //If is a film we show all the components of the film canvas
        RectTransform filmPanel = instance.transform.Find("Canvas/FilmPanel").GetComponent<RectTransform>();
        Utility.showHide(filmPanel.gameObject, true);

        TextMeshPro punctuation = instance.transform.Find("Canvas/FilmPanel/Punctuation").GetComponent<TextMeshPro>();

        //hide button that will be shown when clicking on expand button (+)
        Button infoButton = instance.transform.Find("Canvas/FilmPanel/InfoButton").GetComponent<Button>();
        Utility.showHide(infoButton.gameObject, false);

        Button saveButton = instance.transform.Find("Canvas/FilmPanel/SaveButton").GetComponent<Button>();
        Utility.showHide(saveButton.gameObject, false);

        Button expandButton = instance.transform.Find("Canvas/FilmPanel/ExpandButton").GetComponent<Button>();
        Utility.showHide(expandButton.gameObject, true);

        float valoration = this.jsonDetectedObject.GetField("rating").n;
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
        //We use this method to know if it's a user or a movie
        using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                //We call user and film in android for know if is a film or is an user
                jo.Call("DAOController", "getFilmById", this.idDetected);
                jo.Call("DAOController", "getUserById", this.idDetected);

            }
        }
        
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
        //method that communicates with android to see the information of the movie
        using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                jo.Call("DAOController", "infoFilm", this.idFilm);
            }
        }
    }
    public void getFilmById(String info)
    {
        //This method receive the information from android in case that the object is a film
        this.jsonDetectedObject = new JSONObject(info);
        this.idFilm = jsonDetectedObject.GetField("id").ToString();
        fillFilmData();
    }
    public void getUserById(String info)
    {
        //This method receive the information from android in case that the object is a user
        this.jsonDetectedObject = new JSONObject(info);
        this.idUser = jsonDetectedObject.GetField("id").ToString();
        fillUserData();
    }
    public void youtubeClick()
    {
        comunication("youtube", this.jsonDetectedObject.str);
    }
    public void addFriendClick()
    {
        //method that communicates with android to add a friend
        using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                jo.Call("DAOController", "addFriend", this.idUser);
            }
        }
        
    }
    public void addFriend(String info)
    {
        areFriends(info);
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
    private void changeActivity(string method)
    {
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");

        jo.Call(method);
    }

    private void friends()
    {
        //We use this method when we know that the object is an user and we want to know if is a friend
        using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                jo.Call("DAOController", "areFriends", this.idUser);

            }
        }
    }
    private void getPlans(String info)
    {
        fillFriends(info);
    }
    public void areFriends(String info)
    {
        //This method receive the information from android and says if is a friend or not
        if (string.Equals(info, "true") || (!string.Equals(info, "null") && !string.Equals(info, "false")))
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    jo.Call("DAOController", "getPlans", this.idUser);

                }
            }
            //Son amigos
            Debug.Log("Son amigos");
        }
        else
        {
            fillNoFriends();
            Debug.Log("No son amigos");
        }
    }
    public void friendsLeftArrow()
    {
        /*
         La imagen 3 pasa a la imagen 2
         La imagen 1 pasa a la imagen 3
         La imagen 2 pasa a la imagen 1
         
         */
        
        UnityEngine.UI.Image image1 = this.transform.Find("Canvas/0").GetComponent<UnityEngine.UI.Image>();
        UnityEngine.UI.Image image2 = this.transform.Find("Canvas/1").GetComponent<UnityEngine.UI.Image>();
        UnityEngine.UI.Image image3 = this.transform.Find("Canvas/2").GetComponent<UnityEngine.UI.Image>();
        UnityEngine.UI.Image image2Aux = GameObject.Instantiate(this.transform.Find("Canvas/1").GetComponent<UnityEngine.UI.Image>());
        image2.sprite = image1.sprite;
        image1.sprite = image3.sprite;
        image3.sprite = image2Aux.sprite;
        Destroy(image2Aux);

        resetUserImages();
        for (int i = 0; i < top[2].usersUrl.Count; i++)
        {
            StartCoroutine(cargaImagen((string)top[2].usersUrl[i], this.transform.Find("Canvas/0/Friend" + (i + 1)).GetComponent<UnityEngine.UI.Image>()));
        }
        int puntuacion = (int)top[2].rate;
        if(puntuacion != -1) this.transform.Find("Canvas/0/Friend1/Gauge").GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>("Gauge/gauge" + puntuacion * 10);
        Dictionary<int, plan_info> newDictionary = new Dictionary<int, plan_info>();
        newDictionary.Add(0, top[2]);
        newDictionary.Add(1, top[0]);
        newDictionary.Add(2, top[1]);
        top = newDictionary;
    }
    public void friendsRightArrow()
    {
        /*
         La imagen 2 pasa a la imagen 3
         La imagen 3 pasa a la imagen 1
         La imagen 1 pasa a la imagen 2
         
         */
        UnityEngine.UI.Image image1 = this.transform.Find("Canvas/0").GetComponent<UnityEngine.UI.Image>();
        UnityEngine.UI.Image image2 = this.transform.Find("Canvas/1").GetComponent<UnityEngine.UI.Image>();
        UnityEngine.UI.Image image3 = this.transform.Find("Canvas/2").GetComponent<UnityEngine.UI.Image>();
        UnityEngine.UI.Image image2Aux = GameObject.Instantiate(this.transform.Find("Canvas/1").GetComponent<UnityEngine.UI.Image>());
        image2.sprite = image3.sprite;
        image3.sprite = image1.sprite;
        image1.sprite = image2Aux.sprite;
        Destroy(image2Aux);
        resetUserImages();
        for (int i = 0; i < top[1].usersUrl.Count; i++)
        {
            StartCoroutine(cargaImagen((string)top[1].usersUrl[i], this.transform.Find("Canvas/0/Friend" + (i+1)).GetComponent<UnityEngine.UI.Image>()));
        }
        int puntuacion = (int)top[1].rate;
        if (puntuacion != -1) this.transform.Find("Canvas/0/Friend1/Gauge").GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>("Gauge/gauge" + puntuacion * 10);
        Dictionary<int, plan_info> newDictionary = new Dictionary<int, plan_info>();
        Debug.Log("POSICION 0" + top[0].rate);
        Debug.Log("POSICION 1" + top[1].rate);
        Debug.Log("POSICION 2" + top[2].rate);
        newDictionary.Add(0, top[1]);
        newDictionary.Add(1, top[2]);
        newDictionary.Add(2, top[0]);
        top = newDictionary;
        Debug.Log("POSICION 0" + top[0].rate);
        Debug.Log("POSICION 1" + top[1].rate);
        Debug.Log("POSICION 2" + top[2].rate);
    }
    private void resetUserImages()
    {
        for(int i = 0; i < 6; i++)
        {
            this.transform.Find("Canvas/0/Friend" + (i+1) ).GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>("transparent");
        }
        this.transform.Find("Canvas/0/Friend1/Gauge").GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>("transparent");
        
    }

}
