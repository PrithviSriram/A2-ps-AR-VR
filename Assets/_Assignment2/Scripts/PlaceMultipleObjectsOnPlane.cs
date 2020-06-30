using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class PlaceMultipleObjectsOnPlane : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Instantiates this prefab on a plane at the touch location.")]
    GameObject m_PlacedPrefab;
    public GameObject DCalc;
    List<GameObject> currlist;
    List<GameObject> linelist;
    [SerializeField]
    [Tooltip("Draws a line between the objects on the screen.")]
    GameObject m_DistanceVisualizer;
    private Camera cam;
    List<GameObject> distline;
    void Start()
    {
        cam = Camera.main;
        currlist = new List<GameObject>();
        linelist = new List<GameObject>();
        distline = new List<GameObject>();
    }

    /// <summary>
    /// The prefab to instantiate on touch.
    /// </summary>
    public GameObject placedPrefab
    {
        get { return m_PlacedPrefab; }
        set { m_PlacedPrefab = value; }
    }

    public GameObject distanceVisualizer
    {
        get { return m_DistanceVisualizer; }
        set { m_DistanceVisualizer = value; }
    }


    public GameObject currline { get; private set; }


    /// <summary>
    /// The object instantiated as a result of a successful raycast intersection with a plane.
    /// </summary>
    public GameObject spawnedObject { get; private set; }

    /// <summary>
    /// Invoked whenever an object is placed in on a plane.
    /// </summary>
    public static event Action onPlacedObject;

    ARRaycastManager m_RaycastManager;

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    void Awake()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (m_RaycastManager.Raycast(touch.position, s_Hits, TrackableType.PlaneWithinPolygon))
                {
                    Pose hitPose = s_Hits[0].pose;

                    spawnedObject = Instantiate(m_PlacedPrefab, hitPose.position, hitPose.rotation);
                    currlist.Add((GameObject)spawnedObject);
                    if (currlist.Count >= 2)
                    {
                        GameObject first = currlist[currlist.Count - 2];
                        //m_DistanceVisualizer.GetComponent<LineRenderer>().SetPosition(0, first.transform.position);
                        Vector3 prevpos = first.transform.position;
                        m_DistanceVisualizer.GetComponent<LineRenderer>().SetPosition(0, prevpos);
                        Vector3 currposi = currlist[currlist.Count - 1].transform.position;
                        Vector3 finpos = Vector3.Lerp(prevpos, currposi, 0.1f);
                        m_DistanceVisualizer.GetComponent<LineRenderer>().SetPosition(1, finpos);
                        //m_DistanceVisualizer.GetComponent<LineRenderer>().SetPosition(1, Vector3.Lerp(first,currposi, 0.1f) );
                        GameObject curline = GameObject.Instantiate(m_DistanceVisualizer, spawnedObject.transform.position, Quaternion.identity, spawnedObject.transform);
                        curline.GetComponent<CompleteLine>().finalPos = currlist[currlist.Count - 1].transform.position;
                        linelist.Add(curline);

                        float distanceCalc = Vector3.Distance(currlist[currlist.Count - 2].transform.position, currlist[currlist.Count - 1].transform.position);
                        GameObject dist = Instantiate(DCalc);
                        dist.transform.position = (currlist[currlist.Count - 2].transform.position + currlist[currlist.Count - 1].transform.position) / 2.0f;
                        TextMesh myText = dist.GetComponent<TextMesh>();
                        myText.text = distanceCalc.ToString("#0.00") + 'm';
                        dist.transform.rotation = cam.transform.rotation;
                        distline.Add(dist);
                    }

                    if (onPlacedObject != null)
                    {
                        onPlacedObject();
                    }
                }
            }
        }
    }

    public void undo()
    {
        GameObject undoCube = currlist[currlist.Count - 1];
        currlist.RemoveAt(currlist.Count - 1);
        Destroy(undoCube);
        if (distline.Count > 0)
        {
            float distanceCalc = Vector3.Distance(currlist[currlist.Count - 2].transform.position, currlist[currlist.Count - 1].transform.position);
            GameObject undodist = distline[distline.Count - 1];
            TextMesh myText = distline[distline.Count - 1].GetComponent<TextMesh>();
            myText.text = distanceCalc.ToString("      ");
            distline.RemoveAt(distline.Count - 1);
            Destroy(undodist);
        }
        if (currlist.Count > 0)
        {
            GameObject undoline = linelist[linelist.Count - 1];
            linelist.RemoveAt(linelist.Count - 1);
            Destroy(undoline);
        }

    }
    public void reset()
    {
        while (distline.Count >= 1)
        {
            float distanceCalc = Vector3.Distance(currlist[currlist.Count - 2].transform.position, currlist[currlist.Count - 1].transform.position);
            GameObject undodist = distline[distline.Count - 1];
            TextMesh myText = distline[distline.Count - 1].GetComponent<TextMesh>();
            myText.text = distanceCalc.ToString("       ");
            distline.RemoveAt(distline.Count - 1);
            Destroy(undodist);
        }
        while (currlist.Count >= 1)
        {
            GameObject undoCube = currlist[currlist.Count - 1];
            currlist.RemoveAt(currlist.Count - 1);
            Destroy(undoCube);
        }
        while(linelist.Count >= 1)
        {
            GameObject undoline = linelist[linelist.Count - 1];
            linelist.RemoveAt(linelist.Count - 1);
            Destroy(undoline);
        }

    }
}