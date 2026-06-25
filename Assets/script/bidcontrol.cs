using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bidcontrol : MonoBehaviour
{
    public GameObject HoldMieOBJ;
    public GameObject topbunOBJ;
    public GameObject burgerOBJ;
    public GameObject backrollOBJ;
    public GameObject frontrollOBJ;
    public GameObject HotdogOBJ;

    public GameObject dipegang;
    private bool udahdipegang = false;

    void Start()
    {

    }

    void Update()
    {
        // buat obejknya nempel di kursor
        if (dipegang != null)
        {
            Vector3 posisiLayar = Input.mousePosition;
            posisiLayar.z = 10f;
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(posisiLayar);
            mousePos.z = 0f;
            dipegang.transform.position = mousePos;
        }

        udahdipegang = false;
    }

    private void OnMouseDown()
    {      
        if (dipegang == null)
        {
            dipegang = Instantiate(HoldMieOBJ, transform.position, Quaternion.identity);
            udahdipegang = true;
        }
    }
}