using Vuforia;
using UnityEngine;
using TMPro;
using UnityEngine.Video;
using UnityEngine.UI;
public class tracka : MonoBehaviour, ICloudRecoEventHandler
{
    private CloudRecoBehaviour mCloudRecoBehaviour;
    private bool mIsScanning = false;
    private string mTargetMetadata = "";
    // Use this for initialization
    public ImageTargetBehaviour ImageTargetTemplate;
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
        
        ImageTargetBehaviour itb = Instantiate(ImageTargetTemplate);

        if (itb)
        {
            // enable the new result with the same ImageTargetBehaviour:
            ObjectTracker tracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
            ImageTargetBehaviour imageTargetBehaviour = (ImageTargetBehaviour)tracker.TargetFinder.EnableTracking(targetSearchResult, itb.gameObject);
            imageTargetBehaviour.GetComponentInChildren<TextMeshPro>().text = imageTargetBehaviour.TrackableName;

            Button button = imageTargetBehaviour.GetComponentInChildren<Canvas>().GetComponentInChildren<Button>();
            button.onClick.AddListener(() => {
                Debug.Log("dkajdkwdkjahdw");
            });

            VideoPlayer videoPlayer = imageTargetBehaviour.GetComponentInChildren<VideoPlayer>();
            videoPlayer.Play();
        }
    }

    void OnGUI()
    {
        
    }
}