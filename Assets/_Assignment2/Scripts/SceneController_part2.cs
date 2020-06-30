using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;
using UnityEngine.UI;



//[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(ARRaycastManager))]
public class SceneController_part2 : MonoBehaviour
{
    [SerializeField]
    [Tooltip("AR Session Origin.")]
    ARSessionOrigin m_Session_Origin;
    [SerializeField]
    [Tooltip("AR camera point below the camera.")]
    ARCamera aRCamera;
    [SerializeField]
    [Tooltip("AR the final point where prefab needs to be placed.")]
    ARCamera cubep;
    [SerializeField]
    [Tooltip("AR Session Origin.")]

    public GameObject belcam;

    [SerializeField]
    [Tooltip("AR Session Origin.")]

    public GameObject cubepl;

    public GameObject DCalc;
    [SerializeField]
    [Tooltip("Instantiates this prefab at the end of the line.")]
    GameObject m_PlacedPrefab;
    List<GameObject> currlist;
    List<GameObject> linelist;
    List<GameObject> distline;
    [SerializeField]
    [Tooltip("Draws a line between the objects on the screen.")]
    public LineRenderer fixedLine;
    [SerializeField]
    [Tooltip("Draws a line between the objects on the screen.")]
    GameObject m_DistanceVisualizer_blue;

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
        get { return m_DistanceVisualizer_yellow; }
        set { m_DistanceVisualizer_yellow = value; }
    }

    public GameObject distanceVisualizer_
    {
        get { return m_DistanceVisualizer_blue; }
        set { m_DistanceVisualizer_blue = value; }
    }



    public GameObject currline { get; private set; }


    /// <summary>
    /// The object instantiated as a result of a successful raycast intersection with a plane.
    /// </summary>
    public GameObject spawnedObject { get; private set; }
    public GameObject centralObject { get; private set; }
    public GameObject shad { get; private set; }
    /// <summary>
    /// Invoked whenever an object is placed in on a plane.
    /// </summary>
    public static event Action onPlacedObject;
    ARRaycastManager m_RaycastManager;
    public Slider slid;
    public GameObject Shadow;

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
    public float smoothTime = 0.18f;
    private Vector3 velocity = Vector3.zero;
    private int numberOfPoints = 50;
    private Camera cam;
    private Pose hitPose;
    LineRenderer fl;
    float zpos = 3.0f;
    bool bvar = false;
    void Start()
    {
        m_RaycastManager = m_Session_Origin.GetComponent<ARRaycastManager>();

        cam = Camera.main;
        if (!fixedLine)
        {
            fixedLine = GetComponent<LineRenderer>();
        }
        currlist = new List<GameObject>();
        linelist = new List<GameObject>();
        distline = new List<GameObject>();
        centralObject = Instantiate(m_PlacedPrefab, cubepl.transform.position, cubepl.transform.rotation);
        //m_DistanceVisualizer_yellow.GetComponent<LineRenderer>().SetPosition(0, belcam.transform.position);
        //Vector3 finpos = Vector3.Lerp(cubepl.transform.position, belcam.transform.position, 0.1f);
        Vector3 finpos = cubepl.transform.position;
        //m_DistanceVisualizer_yellow.GetComponent<LineRenderer>().SetPosition(1, finpos);
        //m_DistanceVisualizer_yellow.GetComponent<LineRenderer>().material.color = Color.yellow;
        //fixedline = GameObject.Instantiate(m_DistanceVisualizer_yellow, centralObject.transform.position, Quaternion.identity, centralObject.transform);
        //fixedline.GetComponent<CompleteLine>().finalPos = belcam.transform.position;
        fl = Instantiate(fixedLine);
        cubepl.transform.LookAt(cam.transform);
        shad = Instantiate(Shadow);
    }

    void Awake()
    {
        //m_Session_Origin = m_Session_Origin.GetComponent<ARSessionOrigin>();
        //m_RaycastManager = m_Session_Origin.GetComponent<ARRaycastManager>();

    }

    bool TryGetTouchPosition(out Vector2 touchPosition)
    {
        if (Input.GetMouseButton(0))
        {
            var mousePosition = Input.mousePosition;
            touchPosition = new Vector2(mousePosition.x, mousePosition.y);
            return true;
        }

        if (Input.touchCount > 0)
        {
            touchPosition = Input.GetTouch(0).position;
            return true;
        }

        touchPosition = default;
        return false;
    }

    void Update()
    {
        centralObject.transform.position = cubepl.transform.position;
        m_DistanceVisualizer_yellow.GetComponent<LineRenderer>().SetPosition(0, belcam.transform.position);
        //Vector3 finpos = Vector3.Lerp(cubepl.transform.position, belcam.transform.position, 0.1f);
        Vector3 finpos = cubepl.transform.position;
        m_DistanceVisualizer_yellow.GetComponent<LineRenderer>().SetPosition(1, finpos);
        m_DistanceVisualizer_yellow.GetComponent<LineRenderer>().material.color = Color.yellow;
        //GameObject curline = GameObject.Instantiate(m_DistanceVisualizer_yellow, centralObject.transform.position, Quaternion.identity, centralObject.transform);
        fixedline.GetComponent<CompleteLine>().finalPos = belcam.transform.position;


        //centralObject.transform.position = cubepl.transform.position;

        //Vector3 finpos = Vector3.Lerp(cubepl.transform.position, belcam.transform.position, 0.1f);
        Vector3 finpos = cubepl.transform.position;
        Vector3 startpos = belcam.transform.position;
        centralObject.transform.position = Vector3.SmoothDamp(centralObject.transform.position, finpos, ref velocity, smoothTime);
        Bezier(startpos, centralObject.transform.position);

        m_DistanceVisualizer_yellow.GetComponent<LineRenderer>().SetPosition(0, belcam.transform.position);
        m_DistanceVisualizer_yellow.GetComponent<LineRenderer>().SetPosition(1, finpos);
        m_DistanceVisualizer_yellow.GetComponent<LineRenderer>().material.color = Color.yellow;
        //GameObject curline = GameObject.Instantiate(m_DistanceVisualizer_yellow, centralObject.transform.position, Quaternion.identity, centralObject.transform);
        fixedline.GetComponent<CompleteLine>().finalPos = belcam.transform.position;

        if (!TryGetTouchPosition(out Vector2 touchPosition))
            return;

        if (m_RaycastManager.Raycast(touchPosition, s_Hits))
        {
            // Raycast hits are sorted by distance, so the first one
            // will be the closest hit.
            hitPose = s_Hits[0].pose;
        }

        //RaycastHit2D hit = Physics2D.Raycast(centralObject.transform.position, -Vector2.up);

        Ray ray = new Ray(centralObject.transform.position, centralObject.transform.up * (-1.0f));

        if (m_RaycastManager.Raycast(ray, s_Hits, TrackableType.PlaneWithinPolygon))
        {
            if (!bvar)
            {
                shad = Instantiate(Shadow);
                bvar = true;
            }
            hitPose = s_Hits[0].pose;
            shad.transform.position = hitPose.position;
            shad.transform.rotation = hitPose.rotation;
        }

        if (hit.collider != null)
        {
            if (!shad)
            {
                shad = Instantiate(Shadow);
            }
            shad.transform.position = hit.point;
            shad.transform.position.x = centralObject.transform.position.x;
            shad.transform.position.z = centralObject.transform.position.z;
            shad.transform.position.y = hit.point.y;
        }


        shadow.transform.position = hit.point;
        shadow.transform.LookAt(hit.normal);

    }

    private void Bezier(Vector3 startpos, Vector3 endpos)
    {
        // set points of quadratic Bezier curve
        Vector3 p0 = startpos;
        Vector3 p1 = Vector3.Lerp(startpos, endpos, 0.5f);
        p1 = p1 + velocity / 4;
        Vector3 p2 = endpos;
        float t;
        Vector3 position;
        fl.positionCount = numberOfPoints;
        for (int i = 0; i < numberOfPoints; i++)
        {
            t = i / (numberOfPoints - 1.0f);
            position = (1.0f - t) * (1.0f - t) * p0 + 2.0f * (1.0f - t) * t * p1 + t * t * p2;
            fl.SetPosition(i, position);
        }
    }

    public void place()
    {
        spawnedObject = Instantiate(m_PlacedPrefab, cubepl.transform.position, cubepl.transform.rotation);
        currlist.Add((GameObject)spawnedObject);
        if (currlist.Count >= 2)
        {
            GameObject first = currlist[currlist.Count - 2];
            //m_DistanceVisualizer.GetComponent<LineRenderer>().SetPosition(0, first.transform.position);
            Vector3 prevpos = first.transform.position;
            m_DistanceVisualizer_blue.GetComponent<LineRenderer>().SetPosition(0, prevpos);
            Vector3 currposi = currlist[currlist.Count - 1].transform.position;
            Vector3 finpos = Vector3.Lerp(prevpos, currposi, 0.1f);
            m_DistanceVisualizer_blue.GetComponent<LineRenderer>().SetPosition(1, finpos);
            //m_DistanceVisualizer.GetComponent<LineRenderer>().SetPosition(1, Vector3.Lerp(first,currposi, 0.1f) );
            m_DistanceVisualizer_blue.GetComponent<LineRenderer>().material.color = Color.blue;
            GameObject curline = GameObject.Instantiate(m_DistanceVisualizer_blue, spawnedObject.transform.position, Quaternion.identity, spawnedObject.transform);
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
                        m_DistanceVisualizer.GetComponent<LineRenderer>().material.color = Color.blue;
                        GameObject curline = GameObject.Instantiate(m_DistanceVisualizer, spawnedObject.transform.position, Quaternion.identity, spawnedObject.transform);
                        curline.GetComponent<CompleteLine>().finalPos = currlist[currlist.Count - 1].transform.position;
                        linelist.Add(curline);
                    }

                    if (onPlacedObject != null)
                    {
                        onPlacedObject();
                    }
                }
            }
        }
    }

    public void slider()
    {
        float value = slid.value;
        float newFinPos = zpos + value;
        cubepl.transform.position = new Vector3(cubepl.transform.position.x, cubepl.transform.position.y, newFinPos);
    }



    public void undo()
    {
        GameObject undoCube = currlist[currlist.Count - 1];
        currlist.RemoveAt(currlist.Count - 1);
        Destroy(undoCube);
        if (currlist.Count > 0)
        {
            GameObject undoline = linelist[linelist.Count - 1];
            linelist.RemoveAt(linelist.Count - 1);
            Destroy(undoline);
        }
        if (distline.Count > 0)
        {
            GameObject undodist = distline[distline.Count - 1];
            distline.RemoveAt(distline.Count - 1);
            Destroy(undodist);
        }
    }


    public void reset()
    {
        while (currlist.Count >= 1)
        {
            GameObject undoCube = currlist[currlist.Count - 1];
            currlist.RemoveAt(currlist.Count - 1);
            Destroy(undoCube);
        }
        while (linelist.Count >= 1)
        {
            GameObject undoline = linelist[linelist.Count - 1];
            linelist.RemoveAt(linelist.Count - 1);
            Destroy(undoline);
        }
        while (distline.Count >= 1)
        {
            GameObject undodist = distline[distline.Count - 1];
            distline.RemoveAt(distline.Count - 1);
            Destroy(undodist);
        }
    }
}
