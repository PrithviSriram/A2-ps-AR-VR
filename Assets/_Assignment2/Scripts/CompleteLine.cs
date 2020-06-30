using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CompleteLine : MonoBehaviour
{
    public Vector3 finalPos;
    LineRenderer lineRend;
    Vector3 initpos;
    float Dist;
    Vector3 center;
    GameObject Text;
/*    public string standByText = "Click Platform";
    public string startedText = "Good Luck Prithvi!";*/
    public TextMeshProUGUI tmpr;
    //bool onetime = false;
    void Start()
    {
        lineRend = this.GetComponent<LineRenderer>();
        initpos = lineRend.GetPosition(1);
        Dist = Vector3.Distance(initpos, finalPos);
        center = new Vector3((initpos.x + finalPos.x) / 2.0f, (initpos.y + finalPos.y) / 2.0f, (initpos.z + finalPos.z) / 2.0f);
        tmpr = GetComponent<TextMeshProUGUI>();
        tmpr.text = "";
        tmpr.transform.position = center;
        Instantiate(tmpr);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 currPos = lineRend.GetPosition(1);
        if (Vector3.Distance(currPos, finalPos) > 0.01f)
        {

            currPos = Vector3.Lerp(currPos, finalPos, 0.1f);
            lineRend.SetPosition(1, currPos);
        }
        else
        {
            TextOut(); 
        /*    if(!onetime)
            {

                onetime = true;
            }*/
        }
    }

    void TextOut()
    {
        tmpr.text = Dist.ToString() + "m";
    }
}
