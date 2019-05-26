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
    private AndroidJavaObject jo;

    private struct plan_info
    {
        public ArrayList usersUrl;
        public ArrayList ratings;
        public int planID;
    }
    private Dictionary<int, plan_info> top;
    void Start()
    {
        // register this event handler at the cloud reco behaviour
        mCloudRecoBehaviour = GetComponent<CloudRecoBehaviour>();
        GameObject friendsCanvas = this.transform.Find("Canvas").GetComponent<Canvas>().gameObject;
        Utility.showHide(friendsCanvas, false);
        fillFriends("");
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
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        this.jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
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
        /*RectTransform userPanel = instance.transform.Find("Canvas/UserPanel").GetComponent<RectTransform>();
        Utility.showHide(userPanel.gameObject, false);*/
        //we show the friend panel
        GameObject friendsCanvas = this.transform.Find("Canvas").GetComponent<Canvas>().gameObject;
        Utility.showHide(friendsCanvas, true);
        //we load the json with the information that comes from android
        //string definitivo = "[{\"value2\":{\"id\":5,\"uuid\":\"b1d63dd282174648a18b207057d9c177\",\"name\":\"El Rey LeÔö£Ôöén\",\"director\":\"Roger Allers\",\"trailerURL\":\"https://www.youtube.com/watch?v=xB5ceAruYrI\",\"infoURL\":null,\"synopsis\":\"Tras la muerte de su padre, Simba vuelve a enfrentar a su malvado tío, Scar, y reclamar el trono de rey.\",\"imageURL\":\"http://filmar-develop.herokuapp.com/images/avengers-endgame.jpg\",\"genre\":\"Animation\",\"duration\":88,\"rating\":8.5,\"country\":\"USA\",\"premiere\":\"2019-02-07T23:00:00.000+0000\"},\"value3\":[{\"id\":2,\"uuid\":\"4c8048623944436699b3456fad3238d2\",\"name\":\"Diego\",\"email\":\"dacuna@ucm.es\",\"password\":\"1234\",\"imageURL\":\"http://filmar-develop.herokuapp.com/images/diego.png\"},{\"id\":3,\"uuid\":\"6cac53efcd2e4caebcda9ea401d0e782\",\"name\":\"Daniel\",\"email\":\"dacalle@ucm.es\",\"password\":\"1234\",\"imageURL\":\"http://filmar-develop.herokuapp.com/images/daniel.png\"},{\"id\":1,\"uuid\":\"596abcff71644f4eb855a2d372941674\",\"name\":\"Zihao\",\"email\":\"zhong@ucm.es\",\"password\":\"1234\",\"imageURL\":\"http://filmar-develop.herokuapp.com/images/zihao.png\"}],\"value0\":{\"id\":64,\"creatorId\":3,\"filmId\":5,\"title\":\"Palomitas y leones\",\"date\":\"2019-06-20\",\"location\":\"Principe Pio\",\"description\":\"Muuuuuy prioritario\uD83D\uDE0A\"},\"value1\":{\"userId\":2,\"filmId\":5,\"rating\":10.0,\"date\":\"2019-05-12T11:06:11.543+0000\"},\"size\":4},{\"value2\":{\"id\":5,\"uuid\":\"b1d63dd282174648a18b207057d9c177\",\"name\":\"El Rey LeÔö£Ôöén\",\"director\":\"Roger Allers\",\"trailerURL\":\"https://www.youtube.com/watch?v=xB5ceAruYrI\",\"infoURL\":null,\"synopsis\":\"Tras la muerte de su padre, Simba vuelve a enfrentar a su malvado tío, Scar, y reclamar el trono de rey.\",\"imageURL\":\"http://filmar-develop.herokuapp.com/images/the-hunger-games.jpg\",\"genre\":\"Animation\",\"duration\":88,\"rating\":8.5,\"country\":\"USA\",\"premiere\":\"2019-02-07T23:00:00.000+0000\"},\"value3\":[{\"id\":2,\"uuid\":\"4c8048623944436699b3456fad3238d2\",\"name\":\"Diego\",\"email\":\"dacuna@ucm.es\",\"password\":\"1234\",\"imageURL\":\"http://filmar-develop.herokuapp.com/images/diego.png\"},{\"id\":3,\"uuid\":\"6cac53efcd2e4caebcda9ea401d0e782\",\"name\":\"Daniel\",\"email\":\"dacalle@ucm.es\",\"password\":\"1234\",\"imageURL\":\"http://filmar-develop.herokuapp.com/images/daniel.png\"}],\"value0\":{\"id\":64,\"creatorId\":3,\"filmId\":5,\"title\":\"Palomitas y leones\",\"date\":\"2019-06-20\",\"location\":\"Principe Pio\",\"description\":\"Muuuuuy prioritario😊\"},\"value1\":{\"userId\":2,\"filmId\":5,\"rating\":10,\"date\":\"2019-05-12T11:06:11.543+0000\"},\"size\":4},{\"value2\":{\"id\":5,\"uuid\":\"b1d63dd282174648a18b207057d9c177\",\"name\":\"El Rey LeÔö£Ôöén\",\"director\":\"Roger Allers\",\"trailerURL\":\"https://www.youtube.com/watch?v=xB5ceAruYrI\",\"infoURL\":null,\"synopsis\":\"Tras la muerte de su padre, Simba vuelve a enfrentar a su malvado tío, Scar, y reclamar el trono de rey.\",\"imageURL\":\"http://filmar-develop.herokuapp.com/images/the-lion-king.jpg\",\"genre\":\"Animation\",\"duration\":88,\"rating\":8.5,\"country\":\"USA\",\"premiere\":\"2019-02-07T23:00:00.000+0000\"},\"value3\":[{\"id\":2,\"uuid\":\"4c8048623944436699b3456fad3238d2\",\"name\":\"Diego\",\"email\":\"dacuna@ucm.es\",\"password\":\"1234\",\"imageURL\":\"http://filmar-develop.herokuapp.com/images/diego.png\"},{\"id\":3,\"uuid\":\"6cac53efcd2e4caebcda9ea401d0e782\",\"name\":\"Daniel\",\"email\":\"dacalle@ucm.es\",\"password\":\"1234\",\"imageURL\":\"http://filmar-develop.herokuapp.com/images/carlos.png\"}],\"value0\":{\"id\":64,\"creatorId\":3,\"filmId\":5,\"title\":\"Palomitas y leones\",\"date\":\"2019-06-20\",\"location\":\"Principe Pio\",\"description\":\"Muuuuuy prioritario😊\"},\"value1\":{\"userId\":2,\"filmId\":5,\"rating\":10,\"date\":\"2019-05-12T11:06:11.543+0000\"},\"size\":4}]";
        string definitivo = "[{\"value2\":{\"id\":14,\"uuid\":\"94c8b578a35043738c1b4aae1631adb0\",\"name\":\"Avengers: Endgame\",\"director\":\"Anthony Russo, Joe Russo\",\"trailerURL\":\"https://www.youtube.com/watch?v=TcMBFSGVi1c\",\"synopsis\":\"After the devastating events of Vengadores: Infinity War (2018), the universe is in ruins. With the help of remaining allies, the Avengers assemble once more in order to undo Thanos' actions and restore order to the universe.\",\"imageURL\":\"http://filmar-develop.herokuapp.com/images/avengers-endgame.jpg\",\"genre\":\"Action, Adventure, Fantasy, Sci-Fi\",\"duration\":181,\"rating\":8.8,\"country\":\"USA\",\"premiere\":\"2019-04-25T02:00:00.000+0000\"},\"value3\":[{\"value0\":{\"id\":4,\"uuid\":\"0ef4e072b24744869bb1c1a238032a2c\",\"name\":\"Carlos\",\"email\":\"cargom11@ucm.es\",\"password\":\"carlos\",\"imageURL\":\"http://filmar-develop.herokuapp.com/images/carlos.png\"},\"value1\":{\"userId\":4,\"filmId\":14,\"rating\":10.0,\"date\":\"2019-05-26T08:25:55.052+0000\"},\"size\":2},{\"value0\":{\"id\":3,\"uuid\":\"6cac53efcd2e4caebcda9ea401d0e782\",\"name\":\"Daniel\",\"email\":\"dacalle@ucm.es\",\"password\":\"daniel\",\"imageURL\":\"http://filmar-develop.herokuapp.com/images/daniel.png\"},\"value1\":null,\"size\":2},{\"value0\":{\"id\":1,\"uuid\":\"596abcff71644f4eb855a2d372941674\",\"name\":\"Zihao\",\"email\":\"zhong@ucm.es\",\"password\":\"zihao\",\"imageURL\":\"http://filmar-develop.herokuapp.com/images/zihao.png\"},\"value1\":{\"userId\":1,\"filmId\":14,\"rating\":8.751913,\"date\":\"2019-05-26T08:25:54.675+0000\"},\"size\":2}],\"value0\":{\"id\":19,\"creatorId\":3,\"filmId\":14,\"title\":\"HOLA\",\"date\":\"2019-05-14\",\"location\":\"Narnia\",\"description\":\"ME ABURRO\"},\"value1\":{\"userId\":4,\"filmId\":14,\"rating\":10.0,\"date\":\"2019-05-26T08:25:55.052+0000\"},\"size\":4}]";
        JSONObject json_def = new JSONObject(definitivo);
        //we take and show the information of the recommendation
        if (json_def.IsArray && json_def.list.Count > 0 )
        {
            for (int i = 0; i < json_def.list.Count; i++)
            {
                StartCoroutine(cargaImagen(json_def.list[i].GetField("value2").GetField("imageURL").str, this.transform.Find("Canvas/" + i).GetComponent<UnityEngine.UI.Image>()));
            }
            for (int i = 0; i < json_def.list[0].GetField("value3").list.Count; i++)
            {
                StartCoroutine(cargaImagen(json_def.list[0].GetField("value3").list[i].GetField("value0").GetField("imageURL").str, this.transform.Find("Canvas/0/Friend" + (i+1)).GetComponent<UnityEngine.UI.Image>()));
                double rate_ = 0.0;
                Debug.Log(json_def.list[0].GetField("value3").list[i].GetField("value1").str);
                if (json_def.list[0].GetField("value3").list[i].GetField("value1") != null)
                {
                    if (json_def.list[0].GetField("value3").list[i].GetField("value1").HasField("rating"))
                    {
                        rate_ = double.Parse(json_def.list[0].GetField("value3").list[i].GetField("value1").GetField("rating").ToString());
                    }
                }
                this.transform.Find("Canvas/0/Friend" + (i + 1) + "/Gauge").GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>("Gauge/gauge" + Math.Round(rate_) * 10);

            }
            /*double rate = double.Parse(json_def.list[0].GetField("value1").GetField("rating").ToString());
            this.transform.Find("Canvas/0/Friend1/Gauge").GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>("Gauge/gauge" + Math.Round(rate) * 10);*/
            

        }
        //we save the information to change it by pressing the arrows
        this.top = new Dictionary<int, plan_info>();
        for (int i = 0; i < json_def.list.Count; i++)
        {
            plan_info planAux = new plan_info();
            planAux.usersUrl = new ArrayList();
            planAux.ratings = new ArrayList();
            for(int j = 0; j < json_def.list[i].GetField("value3").list.Count; j++)
            {
                planAux.usersUrl.Add(json_def.list[i].GetField("value3").list[j].GetField("value0").GetField("imageURL").str);
                double rate_ = 0.0;
                if (json_def.list[i].GetField("value3").list[j].GetField("value1").str != null)
                {
                    if (json_def.list[i].GetField("value3").list[j].GetField("value1").HasField("rating"))
                    {
                        rate_ = double.Parse(json_def.list[i].GetField("value3").list[j].GetField("value1").GetField("rating").ToString());
                    }
                }
                planAux.ratings.Add(rate_);
                
            }
            json_def.list[i].GetField("value1").GetField("rating");
            //planAux.rate = double.Parse(json_def.list[i].GetField("value1").GetField("rating").ToString());
            planAux.planID = int.Parse(json_def.list[i].GetField("value0").GetField("id").ToString());
            this.top.Add(i, planAux);
        }
        while(this.top.Count< 3)
        {
            plan_info planAux = new plan_info();
            planAux.usersUrl = new ArrayList();
            //planAux.rate = -1;
            planAux.ratings = new ArrayList();
            planAux.planID = -1;
            this.top.Add(this.top.Count, planAux);
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
    public void joinPlan()
    {
        //method that communicates with android to add a friend
        using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                int id = this.top[0].planID;
                string id_string = "" + id;
                Debug.Log("El id es");
                Debug.Log(id);
                jo.Call("DAOController", "joinPlan", id_string);
            }
        }
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
        for (int i = 0; i < this.top[2].usersUrl.Count; i++)
        {
            StartCoroutine(cargaImagen((string)this.top[2].usersUrl[i], this.transform.Find("Canvas/0/Friend" + (i + 1)).GetComponent<UnityEngine.UI.Image>()));
        }
        int puntuacion = (int)top[2].ratings[0];
        if(puntuacion != -1) this.transform.Find("Canvas/0/Friend1/Gauge").GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>("Gauge/gauge" + puntuacion * 10);
        Dictionary<int, plan_info> newDictionary = new Dictionary<int, plan_info>();
        newDictionary.Add(0, this.top[2]);
        newDictionary.Add(1, this.top[0]);
        newDictionary.Add(2, this.top[1]);
        this.top = newDictionary;
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
        for (int i = 0; i < this.top[1].usersUrl.Count; i++)
        {
            StartCoroutine(cargaImagen((string)this.top[1].usersUrl[i], this.transform.Find("Canvas/0/Friend" + (i+1)).GetComponent<UnityEngine.UI.Image>()));
        }
        int puntuacion = (int)this.top[1].ratings[0];
        if (puntuacion != -1) this.transform.Find("Canvas/0/Friend1/Gauge").GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>("Gauge/gauge" + puntuacion * 10);
        Dictionary<int, plan_info> newDictionary = new Dictionary<int, plan_info>();
        Debug.Log("POSICION 0" + this.top[0].ratings[0]);
        Debug.Log("POSICION 1" + this.top[1].ratings[0]);
        Debug.Log("POSICION 2" + this.top[2].ratings[0]);
        newDictionary.Add(0, this.top[1]);
        newDictionary.Add(1, this.top[2]);
        newDictionary.Add(2, this.top[0]);
        this.top = newDictionary;
        Debug.Log("POSICION 0" + this.top[0].ratings[0]);
        Debug.Log("POSICION 1" + this.top[1].ratings[0]);
        Debug.Log("POSICION 2" + this.top[2].ratings[0]);
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
