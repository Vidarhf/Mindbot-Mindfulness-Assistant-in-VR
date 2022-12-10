using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct Gesture
{
    public string name;
    public List<Vector3> fingerDatas;
    public UnityEvent onRecognized;
}
public class GestureDetector : MonoBehaviour
{
    [SerializeField]
    public float RecogThreshold = 0.1f;
    public OVRSkeleton skeleton;
    public List<Gesture> gestures;
    public bool DebugMode;
    public bool HasStarted = false;
    private List<OVRBone> fingerBones;
    private Gesture previousGesture;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("PopulateBones");
        previousGesture = new Gesture();
        
    }

    private void Update()
    {
        if(DebugMode && Input.GetKeyDown(KeyCode.DownArrow))
        {
            SaveGesture();
            Debug.Log("Gesture saved");
        }

        if (HasStarted)
        { 
            Gesture currentGesture = Recognize();
            bool hasRecognized = !currentGesture.Equals(new Gesture());
            //Check if current gesture wasnt found
            if(hasRecognized && !currentGesture.Equals(previousGesture))
            {
                //new gesture
                Debug.Log("New gesture:" + currentGesture.name);
                previousGesture = currentGesture;
                currentGesture.onRecognized.Invoke();
            }
        }
    }

    private IEnumerator PopulateBones()
    {
        Debug.Log("PopBones: started");
        yield return new WaitUntil(BonesGreaterThanZero);
        yield return new WaitForSeconds(1);
        fingerBones = new List<OVRBone>(skeleton.Bones);
        yield return new WaitForSeconds(1);
        HasStarted = true;

    }

    bool BonesGreaterThanZero()
    {
        if (skeleton.Bones.Count > 0)
        {
            return true;
        }
        else return false;
    }

    void SaveGesture()
    {
        Gesture g = new Gesture();
        g.name = "New Gesture";
        List<Vector3> data = new List<Vector3>();
        foreach (var bone in fingerBones)
        {
            //Finger position relative to root
            data.Add(skeleton.transform.InverseTransformPoint(bone.Transform.position));
        }

        g.fingerDatas = data;
        gestures.Add(g);

    }

    private Gesture Recognize()
    {
        Gesture currentGesture = new Gesture();
        float currentMin = Mathf.Infinity;

        foreach (var gesture in gestures)
        {
            float sumDistance = 0;
            bool isDiscarded = false;

            //Expensive, change to squared magnitude
            for (int i = 0; i < fingerBones.Count; i++)
            {
                Vector3 currentData = skeleton.transform.InverseTransformPoint(fingerBones[i].Transform.position);
                float distance = Vector3.Distance(currentData, gesture.fingerDatas[i]);
                if (distance > RecogThreshold)
                {
                    isDiscarded = true;
                    break;
                }
                sumDistance += distance;
            }
            if (!isDiscarded && sumDistance < currentMin)
            {
                currentMin = sumDistance;
                currentGesture = gesture;
            }

        }
            return currentGesture;
    }
}
