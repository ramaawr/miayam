using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bidcontrol : MonoBehaviour
{
    public GameObject HoldItemOBJ;
    public GameObject dipegang;


    void Start()
    {

    }

    void Update()
    {
        // buat obejknya nempel di kursor
        if (dipegang != null )
        {
            Vector3 posisiLayar = Input.mousePosition;
            posisiLayar.z = 10f;
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(posisiLayar);
            mousePos.z = 0f;
            dipegang.transform.position = mousePos;

            float putaranMouse = Input.mouseScrollDelta.y; 

            if (putaranMouse != 0) 
            {
                dipegang.transform.Rotate(0, 0, putaranMouse * 15f);
            }

            if  (Input.GetMouseButtonDown(1) && dipegang != null)
            {
                Destroy(dipegang);
                dipegang = null;
            }
        }

    }

    private void OnMouseDown()
    {

        bidcontrol[] SemuaBak = FindObjectsOfType<bidcontrol>();

        foreach (bidcontrol BakAktif in SemuaBak)
        {
            if(BakAktif.dipegang != null) 
            {
            return;
            }
        }


        if (dipegang == null)
        {
            dipegang = Instantiate(HoldItemOBJ, transform.position, Quaternion.identity);
        }
        
    }
}