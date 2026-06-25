using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class kompor : MonoBehaviour
{
    public bidcontrol tanganpemain;
    public GameObject HoldMieMateng;
    public Slider barwaktu;

    //waktu masak seting di sini
    public float WaktuMasak = 10f;

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
        if (statusKompor == 1)
        {
            waktuMasakSekarang += Time.deltaTime;
            barwaktu.value = waktuMasakSekarang / WaktuMasak;

            if (waktuMasakSekarang >= WaktuMasak)
            {
                statusKompor = 2;
            }
        }
    }

    void OnMouseDown()
    {
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

    }
}
