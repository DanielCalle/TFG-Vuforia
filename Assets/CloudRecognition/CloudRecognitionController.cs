﻿using Vuforia;
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

    private struct plan_info
    {
        public ArrayList usersUrl;
        public ArrayList notas;
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
    private void fillFriends()
    {
        /*RectTransform userPanel = instance.transform.Find("Canvas/UserPanel").GetComponent<RectTransform>();
        Utility.showHide(userPanel.gameObject, false);
        //If is a friend we show all the components of the friends canvas
        RectTransform friendPanel = instance.transform.Find("Canvas/FriendPanel").GetComponent<RectTransform>();
        Utility.showHide(friendPanel.gameObject, true);
        TextMeshPro userName = instance.transform.Find("Canvas/FriendPanel/UserName").GetComponent<TextMeshPro>();
        String name = this.jsonDetectedObject.GetField("name").str;
        userName.text = name;*/
        RectTransform userPanel = instance.transform.Find("Canvas/UserPanel").GetComponent<RectTransform>();
        Utility.showHide(userPanel.gameObject, false);
        GameObject friendsCanvas = this.transform.Find("Canvas").GetComponent<Canvas>().gameObject;
        Utility.showHide(friendsCanvas, true);
        //string str = "{\"usersUrl_Film1\": {\"1\":\"https://drive.google.com/uc?export=download&id=1AZ279LzgZCbfkaasa3eKJzDUi-q0T1CI\",\"2\":\"https://drive.google.com/uc?export=download&id=1Hsoa9hy_RK6ddMdLSIsFXOfiNieS9KRa\",\"3\":\"https://drive.google.com/uc?export=download&id=11_ZRh-t9z1cAOLPaXErg9Uby-r7lxkDL\",\"4\":\"https://drive.google.com/uc?export=download&id=1AWP_iRqBDbbHVe3DugqQH9HrK3g8RgD_\",\"5\":\"https://drive.google.com/uc?export=download&id=1iRUr8ZY-xfgczAw9sWyMclMo_e5JQiIm\",\"6\":\"https://drive.google.com/uc?export=download&id=17qRydHbpDJ444O4ZjFtQVT8Oum7aOeCs\"},\"usersUrl_Film2\": {\"1\":\"https://drive.google.com/uc?export=download&id=1AZ279LzgZCbfkaasa3eKJzDUi-q0T1CI\",\"2\":\"https://drive.google.com/uc?export=download&id=1Hsoa9hy_RK6ddMdLSIsFXOfiNieS9KRa\",\"3\":\"https://drive.google.com/uc?export=download&id=1iRUr8ZY-xfgczAw9sWyMclMo_e5JQiIm\",\"4\":\"https://drive.google.com/uc?export=download&id=1AWP_iRqBDbbHVe3DugqQH9HrK3g8RgD_\",\"5\":\"https://drive.google.com/uc?export=download&id=11_ZRh-t9z1cAOLPaXErg9Uby-r7lxkDL\",\"6\":\"https://drive.google.com/uc?export=download&id=17qRydHbpDJ444O4ZjFtQVT8Oum7aOeCs\"},\"usersUrl_Film3\": {\"1\":\"https://drive.google.com/uc?export=download&id=1AZ279LzgZCbfkaasa3eKJzDUi-q0T1CI\",\"2\":\"https://drive.google.com/uc?export=download&id=1Hsoa9hy_RK6ddMdLSIsFXOfiNieS9KRa\",\"3\":\"https://drive.google.com/uc?export=download&id=11_ZRh-t9z1cAOLPaXErg9Uby-r7lxkDL\",\"4\":\"https://drive.google.com/uc?export=download&id=1AWP_iRqBDbbHVe3DugqQH9HrK3g8RgD_\",\"5\":\"https://drive.google.com/uc?export=download&id=1iRUr8ZY-xfgczAw9sWyMclMo_e5JQiIm\",\"6\":\"https://drive.google.com/uc?export=download&id=17qRydHbpDJ444O4ZjFtQVT8Oum7aOeCs\"},\"films\": {\"1\":\"https://drive.google.com/uc?export=download&id=1JT9Bcl6FimSqvejD3BNvVWmbv0t8mlVE\",\"2\":\"https://drive.google.com/uc?export=download&id=1RBiLM2II3tEXs-ZsdTSWroMu-xCzrbR2\",\"3\":\"https://drive.google.com/uc?export=download&id=1WqyyVvg56pSeCyC0KAtjZLmmekZ0hyi9\"},\"usersRatings_Film1\": {\"1\":8,\"2\":7,\"3\":6,\"4\":5,\"5\":4,\"6\":3},\"usersRatings_Film2\": {\"1\":1,\"2\":1,\"3\":1,\"4\":1,\"5\":1,\"6\":1},\"usersRatings_Film3\": {\"1\":5,\"2\":5,\"3\":5,\"4\":5,\"5\":5,\"6\":5}}";
        string str = "{\"usersUrl_Film1\": {\"1\":\"https://drive.google.com/uc?export=download&id=1u_KQSjgfq6yjEWDgVu26CD3LbOa6LF6I\",\"2\":\"https://drive.google.com/uc?export=download&id=1El8StoIH-90zQS5S3vJ-fm3O0eHXiArC\",\"3\":\"https://drive.google.com/uc?export=download&id=1e_2RqYU-8VNrsjzYTIHJSLe7q7rJzWkQ\",\"4\":\"https://drive.google.com/uc?export=download&id=1txUTitJRfHu1YPftp2db2WIFcwqBEh7n\",\"5\":\"https://drive.google.com/uc?export=download&id=1oKxiawgsZ4HVpMngdLmePqSRYllNJ1bv\",\"6\":\"https://drive.google.com/uc?export=download&id=1BKH2Msrv9LwTLaMZE-VP2FlUqeLUoMtO\"},\"usersUrl_Film2\": {\"1\":\"https://drive.google.com/uc?export=download&id=1u_KQSjgfq6yjEWDgVu26CD3LbOa6LF6I\",\"2\":\"https://drive.google.com/uc?export=download&id=1El8StoIH-90zQS5S3vJ-fm3O0eHXiArC\",\"3\":\"https://drive.google.com/uc?export=download&id=1oKxiawgsZ4HVpMngdLmePqSRYllNJ1bv\",\"4\":\"https://drive.google.com/uc?export=download&id=1txUTitJRfHu1YPftp2db2WIFcwqBEh7n\",\"5\":\"https://drive.google.com/uc?export=download&id=1e_2RqYU-8VNrsjzYTIHJSLe7q7rJzWkQ\",\"6\":\"https://drive.google.com/uc?export=download&id=1BKH2Msrv9LwTLaMZE-VP2FlUqeLUoMtO\"},\"usersUrl_Film3\": {\"1\":\"https://drive.google.com/uc?export=download&id=1u_KQSjgfq6yjEWDgVu26CD3LbOa6LF6I\",\"2\":\"https://drive.google.com/uc?export=download&id=1El8StoIH-90zQS5S3vJ-fm3O0eHXiArC\",\"3\":\"https://drive.google.com/uc?export=download&id=1e_2RqYU-8VNrsjzYTIHJSLe7q7rJzWkQ\",\"4\":\"https://drive.google.com/uc?export=download&id=1txUTitJRfHu1YPftp2db2WIFcwqBEh7n\",\"5\":\"https://drive.google.com/uc?export=download&id=1oKxiawgsZ4HVpMngdLmePqSRYllNJ1bv\",\"6\":\"https://drive.google.com/uc?export=download&id=1BKH2Msrv9LwTLaMZE-VP2FlUqeLUoMtO\"},\"films\": {\"1\":\"https://drive.google.com/uc?export=download&id=1JT9Bcl6FimSqvejD3BNvVWmbv0t8mlVE\",\"2\":\"https://drive.google.com/uc?export=download&id=1RBiLM2II3tEXs-ZsdTSWroMu-xCzrbR2\",\"3\":\"https://drive.google.com/uc?export=download&id=1WqyyVvg56pSeCyC0KAtjZLmmekZ0hyi9\"},\"usersRatings_Film1\": {\"1\":8,\"2\":7,\"3\":6,\"4\":5,\"5\":4,\"6\":3},\"usersRatings_Film2\": {\"1\":1,\"2\":1,\"3\":1,\"4\":1,\"5\":1,\"6\":1},\"usersRatings_Film3\": {\"1\":5,\"2\":5,\"3\":5,\"4\":5,\"5\":5,\"6\":5}}";
        JSONObject json = new JSONObject(str);
        StartCoroutine(cargaImagen(json.GetField("films").GetField("1").str, this.transform.Find("Canvas/FirstPosition").GetComponent<UnityEngine.UI.Image>()));
        StartCoroutine(cargaImagen(json.GetField("films").GetField("2").str, this.transform.Find("Canvas/SecondPosition").GetComponent<UnityEngine.UI.Image>()));
        StartCoroutine(cargaImagen(json.GetField("films").GetField("3").str, this.transform.Find("Canvas/ThirdPosition").GetComponent<UnityEngine.UI.Image>()));
        for (int i = 1; i <= 6; i++)
        {
            StartCoroutine(cargaImagen(json.GetField("usersUrl_Film1").GetField(i.ToString()).str, this.transform.Find("Canvas/FirstPosition/Friend" + i).GetComponent<UnityEngine.UI.Image>()));
            int puntuacion = int.Parse(json.GetField("usersRatings_Film1").GetField(i.ToString()).ToString(), System.Globalization.NumberStyles.Integer);
            this.transform.Find("Canvas/FirstPosition/Friend" + i + "/Gauge").GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>("Gauge/gauge" + puntuacion*10);
        }
        top = new Dictionary<int, plan_info>();
        for (int i = 1; i <= 3; i++)
        {
            plan_info planAux = new plan_info();
            planAux.usersUrl = new ArrayList();
            planAux.notas = new ArrayList();
            for(int j = 1; j <= 6; j++)
            {
                planAux.usersUrl.Add(json.GetField("usersUrl_Film" + (i)).GetField(j.ToString()).str);
                planAux.notas.Add(int.Parse(json.GetField("usersRatings_Film" + (i)).GetField(j.ToString()).ToString(), System.Globalization.NumberStyles.Integer));
            }
            top.Add(i, planAux);
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
        comunication("info", this.jsonDetectedObject.str);
    }
    public void getFilmById(String info)
    {
        //This method receive the information from android in case that the object is a film
        this.jsonDetectedObject = new JSONObject(info);
        fillFilmData();
    }
    public void getUserById(String info)
    {
        //This method receive the information from android in case that the object is a user
        this.jsonDetectedObject = new JSONObject(info);
        this.idUser = jsonDetectedObject.GetField("id").str;
        fillUserData();
    }
    public void youtubeClick()
    {
        comunication("youtube", this.jsonDetectedObject.str);
    }
    public void addFriendClick()
    {
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
        fillFriends();
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
    public void areFriends(String info)
    {
        //This method receive the information from android and says if is a friend or not
        if (string.Equals(info, "true") || (!string.Equals(info, "null") && !string.Equals(info, "false")))
        {
            //Son amigos
            fillFriends();
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
        /*UnityEngine.UI.Image image1 = this.transform.Find("Canvas/FirstPosition").GetComponent<UnityEngine.UI.Image>();
        UnityEngine.UI.Image image2 = this.transform.Find("Canvas/SecondPosition").GetComponent<UnityEngine.UI.Image>();
        UnityEngine.UI.Image image3 = this.transform.Find("Canvas/ThirdPosition").GetComponent<UnityEngine.UI.Image>();
        UnityEngine.UI.Image image2Aux = GameObject.Instantiate(this.transform.Find("Canvas/SecondPosition").GetComponent<UnityEngine.UI.Image>());
        image2.sprite = image1.sprite;
        image1.sprite = image3.sprite;
        image3.sprite = image2Aux.sprite;
        Destroy(image2Aux);*/

        UnityEngine.UI.Image image1 = this.transform.Find("Canvas/FirstPosition").GetComponent<UnityEngine.UI.Image>();
        UnityEngine.UI.Image image2 = this.transform.Find("Canvas/SecondPosition").GetComponent<UnityEngine.UI.Image>();
        UnityEngine.UI.Image image3 = this.transform.Find("Canvas/ThirdPosition").GetComponent<UnityEngine.UI.Image>();
        UnityEngine.UI.Image image2Aux = GameObject.Instantiate(this.transform.Find("Canvas/SecondPosition").GetComponent<UnityEngine.UI.Image>());
        image2.sprite = image1.sprite;
        image1.sprite = image3.sprite;
        image3.sprite = image2Aux.sprite;
        Destroy(image2Aux);

        for (int i = 1; i <= 6; i++)
        {
            StartCoroutine(cargaImagen((string)top[3].usersUrl[i-1], this.transform.Find("Canvas/FirstPosition/Friend" + i).GetComponent<UnityEngine.UI.Image>()));
            int puntuacion = (int) top[3].notas[i-1];
            this.transform.Find("Canvas/FirstPosition/Friend" + i + "/Gauge").GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>("Gauge/gauge" + puntuacion * 10);
        }
        Dictionary<int, plan_info> newDictionary = new Dictionary<int, plan_info>();
        newDictionary.Add(1, top[3]);
        newDictionary.Add(2, top[1]);
        newDictionary.Add(3, top[2]);
        top = newDictionary;
    }
    public void friendsRightArrow()
    {
        /*
         La imagen 2 pasa a la imagen 3
         La imagen 3 pasa a la imagen 1
         La imagen 1 pasa a la imagen 2
         
         */
        UnityEngine.UI.Image image1 = this.transform.Find("Canvas/FirstPosition").GetComponent<UnityEngine.UI.Image>();
        UnityEngine.UI.Image image2 = this.transform.Find("Canvas/SecondPosition").GetComponent<UnityEngine.UI.Image>();
        UnityEngine.UI.Image image3 = this.transform.Find("Canvas/ThirdPosition").GetComponent<UnityEngine.UI.Image>();
        UnityEngine.UI.Image image2Aux = GameObject.Instantiate(this.transform.Find("Canvas/SecondPosition").GetComponent<UnityEngine.UI.Image>());
        image2.sprite = image3.sprite;
        image3.sprite = image1.sprite;
        image1.sprite = image2Aux.sprite;
        Destroy(image2Aux);

        for (int i = 1; i <= 6; i++)
        {
            StartCoroutine(cargaImagen((string)top[2].usersUrl[i - 1], this.transform.Find("Canvas/FirstPosition/Friend" + i).GetComponent<UnityEngine.UI.Image>()));
            int puntuacion = (int)top[2].notas[i - 1];
            this.transform.Find("Canvas/FirstPosition/Friend" + i + "/Gauge").GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>("Gauge/gauge" + puntuacion * 10);
        }
        Dictionary<int, plan_info> newDictionary = new Dictionary<int, plan_info>();
        newDictionary.Add(1, top[2]);
        newDictionary.Add(2, top[3]);
        newDictionary.Add(3, top[1]);
        top = newDictionary;
    }

}
