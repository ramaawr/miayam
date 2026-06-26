using UnityEngine;
using System.Collections;

public class Mangkok : MonoBehaviour
{
    public bidcontrol Mie;
    public bidcontrol Baso;
    public GameObject Visualmiedimangkok;

 

    //sistem baso di itunh
    public int Totalbaso= 0;

    private bool Mangkokterisi = false;

    void Start()
    {
        
    }


    void Update()
    {
        

    }

    void OnMouseDown() 
    {
        //ini pokonya buat mi di masukin ke mangkok
        if (Mangkokterisi == false && Mie.dipegang != null && Mie.dipegang.name.Contains("HoldMieMateng"))
        {
            Destroy(Mie.dipegang);
            Mie.dipegang = null;
            Mangkokterisi = true;
            Visualmiedimangkok.SetActive(true);
        }

        //ini bwat basooo
        if (Mangkokterisi == true && Baso.dipegang != null && Baso.dipegang.name.Contains("HoldBasoMateng"))
        {
            GameObject Placedbaso = Baso.dipegang;
            Baso.dipegang = null;

            Placedbaso.transform.SetParent(this.transform);

            Totalbaso = Totalbaso + 1;
            Debug.Log("Total Baso di mangkok sekarang: " + Totalbaso);

        }
         
        
    }
}


