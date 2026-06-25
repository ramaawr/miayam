using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public bidcontrol Tanganpemain;
    public bidcontrol Tanganpemain2;
    public GameObject Visualmiedimangkok;

    //sistem baso di itunh
    public int Totalbaso= 0;

    private bool Mangkokterisi = false;

    void Start()
    {
        Visualmiedimangkok.SetActive(false);
    }


    void Update()
    {
                
    }

    void OnMouseDown() 
    {
        //ini pokonya buat mi di masukin ke mangkok
        if (Mangkokterisi == false && Tanganpemain.dipegang != null && Tanganpemain.dipegang.name.Contains("HoldMieMateng"))
        {
            Destroy(Tanganpemain.dipegang);
            Tanganpemain.dipegang = null;
            Mangkokterisi = true;
            Visualmiedimangkok.SetActive(true);
        }

        //ini bwat basooo
        if (Mangkokterisi == true && Tanganpemain2.dipegang != null && Tanganpemain2.dipegang.name.Contains("HoldBasoMateng"))
        {
            GameObject Placedbaso = Tanganpemain2.dipegang;
            Tanganpemain2.dipegang = null;

            Placedbaso.transform.SetParent(this.transform);
            Totalbaso = Totalbaso + 1;
            Debug.Log("Total Baso di mangkok sekarang: " + Totalbaso);
        }
        
    }
}


