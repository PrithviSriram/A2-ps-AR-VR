using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;

/// <summary>
/// Listens for touch events and performs an AR raycast from the screen touch point.
/// AR raycasts will only hit detected trackables like feature points and planes.
///
/// If a raycast hits a trackable, the <see cref="placedPrefab"/> is instantiated
/// and moved to the hit position.
/// </summary>
[RequireComponent(typeof(ARRaycastManager))]
public class ARTapToPlaceObject : MonoBehaviour
{
    private IEnumerator coroutine;
    float movementSmooth = 0.1f;
    [SerializeField]
    [Tooltip("Instantiates this prefab on a plane at the touch location.")]
    GameObject m_PlacedPrefab;
    //Vector3 touchcor = new Vector3(0,0,0);
    //Vector3 touchrot = new Vector3(0,0,0);
    
    /// <summary>
    /// The prefab to instantiate on touch.
    /// </summary>
    public GameObject placedPrefab
    {
        get { return m_PlacedPrefab; }
        set { m_PlacedPrefab = value; }
    }

    /// <summary>
    /// The object instantiated as a result of a successful raycast intersection with a plane.
    /// </summary>
    public GameObject spawnedObject { get; private set; }

    void Awake()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();
    }

    bool TryGetTouchPosition(out Vector2 touchPosition)
    {
#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            var mousePosition = Input.mousePosition;
            touchPosition = new Vector2(mousePosition.x, mousePosition.y);
            return true;
        }
#else
        if (Input.touchCount > 0)
        {
            touchPosition = Input.GetTouch(0).position;
            return true;
        }
#endif

        touchPosition = default;
        return false;
    }

    void Update()
    {
        if (!TryGetTouchPosition(out Vector2 touchPosition))
            return;

        if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon))
        {
            // Raycast hits are sorted by distance, so the first one
            // will be the closest hit.
            var hitPose = s_Hits[0].pose;

            if (spawnedObject == null)
            {
                spawnedObject = Instantiate(m_PlacedPrefab, hitPose.position, hitPose.rotation);
            }
            else
            {
                StartCoroutine(Translate(s_Hits[0]));
                /*while (Mathf.Abs(spawnedObject.transform.position.x - hitPose.position.x) > 0.03)
                {
                    spawnedObject.transform.position = Vector3.Lerp(spawnedObject.transform.position, hitPose.position, Time.deltaTime * movementSmooth * 10);
                    spawnedObject.transform.rotation = Quaternion.Lerp(spawnedObject.transform.rotation, hitPose.rotation, Time.deltaTime * movementSmooth);
                }
            */
            }
        }
    }
    IEnumerator Translate(ARRaycastHit s_H)
    {
        var HP = s_H.pose;
        while (Mathf.Abs(spawnedObject.transform.position.x - HP.position.x) > 0.03)
        {
            spawnedObject.transform.position = Vector3.Lerp(spawnedObject.transform.position, HP.position, Time.deltaTime * movementSmooth * 10);
            spawnedObject.transform.rotation = Quaternion.Lerp(spawnedObject.transform.rotation, HP.rotation, Time.deltaTime * movementSmooth);
        }
        yield return null;
    }
    //StartCoroutine(Translate(hitInfo));

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    ARRaycastManager m_RaycastManager;
}
