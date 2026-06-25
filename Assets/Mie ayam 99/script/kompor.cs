using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class kompor : MonoBehaviour
{
    public bidcontrol tanganpemain;
    public bidcontrol tanganpemain2;
    public GameObject HoldMieMateng;
    public GameObject HoldBasoMateng;
    public Slider barwaktu;

    //waktu masak seting di sini
    public float WaktuMasakMie = 10f;
    public float WaktuMasakBaso = 10f;

    // 0 = Kosong, 1 = Masak, 2 = Matang
    private int statusKompor = 0;
    private float waktuMasakSekarang = 0f;
    void Start()
    {
        barwaktu.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // mie
        if (statusKompor == 1)
        {
            waktuMasakSekarang += Time.deltaTime;
            barwaktu.value = waktuMasakSekarang / WaktuMasakMie;

            if (waktuMasakSekarang >= WaktuMasakMie)
            {
                statusKompor = 2;
            }
        }

        //baso
        if (statusKompor == 4)
        {
            waktuMasakSekarang += Time.deltaTime;
            barwaktu.value = waktuMasakSekarang / WaktuMasakBaso;

            if (waktuMasakSekarang >= WaktuMasakBaso)
            {
                statusKompor = 5;
            }
        }
    }

    void OnMouseDown()
    {
        //mie
        if (statusKompor == 0 && tanganpemain.dipegang != null && tanganpemain.dipegang.name.Contains("HoldMie"))
        {
            Destroy(tanganpemain.dipegang);
            tanganpemain.dipegang = null;

            statusKompor = 1;
            waktuMasakSekarang = 0f;

            barwaktu.value = 0f;
            barwaktu.gameObject.SetActive(true);
        }

        else if (statusKompor == 2 && tanganpemain.dipegang == null)
        {

            tanganpemain.dipegang = Instantiate(HoldMieMateng, tanganpemain.transform.position, Quaternion.identity);

            statusKompor = 0;
            barwaktu.gameObject.SetActive(false);
        }

        //baso
        if (statusKompor == 0 && tanganpemain2.dipegang != null && tanganpemain2.dipegang.name.Contains("HoldBaso"))
        {
            Destroy(tanganpemain2.dipegang);
            tanganpemain2.dipegang = null;

            statusKompor = 4;
            waktuMasakSekarang = 0f;

            barwaktu.value = 0f;
            barwaktu.gameObject.SetActive(true);
        }

        else if (statusKompor == 5 && tanganpemain2.dipegang == null)
        {

            tanganpemain2.dipegang = Instantiate(HoldBasoMateng, tanganpemain2.transform.position, Quaternion.identity);

            statusKompor = 0;
            barwaktu.gameObject.SetActive(false);
        }

    }
}
