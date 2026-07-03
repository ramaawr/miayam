using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class kompor : MonoBehaviour
{
    public bidcontrol Mie;
    public bidcontrol Baso;
    public GameObject HoldMieMateng;
    public GameObject HoldBasoMateng;
    public Slider barwaktu;

    //waktu masak seting di sini
    public float WaktuMasakMie = 10f;
    public float WaktuMasakBaso = 10f;

    // 0 = Kosong, 1 = Masak, 2 = Matang (untuk mie)
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
                if (TryGetComponent<JuiceAnimator>(out var juice)) juice.PlayPopAnimation();
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
                if (TryGetComponent<JuiceAnimator>(out var juice)) juice.PlayPopAnimation();
            }
        }
    }

    void OnMouseDown()
    {
        //mie
        if (statusKompor == 0 && Mie.dipegang != null && Mie.dipegang.name.Contains("HoldMie"))
        {
            Destroy(Mie.dipegang);
            Mie.dipegang = null;
          

            statusKompor = 1;
            waktuMasakSekarang = 0f;

            barwaktu.value = 0f;
            barwaktu.gameObject.SetActive(true);
            if (TryGetComponent<JuiceAnimator>(out var juice)) juice.StartCookingWobble();
        }

        else if (statusKompor == 2 && Mie.dipegang == null)
        {

            Mie.dipegang = Instantiate(HoldMieMateng, Mie.transform.position, Quaternion.identity);

            statusKompor = 0;
            barwaktu.gameObject.SetActive(false);
            if (TryGetComponent<JuiceAnimator>(out var juice)) juice.StopAnimation();
        }

        //baso
        if (statusKompor == 0 && Baso.dipegang != null && Baso.dipegang.name.Contains("HoldBaso"))
        {
            Destroy(Baso.dipegang);
            Baso.dipegang = null;

            statusKompor = 4;
            waktuMasakSekarang = 0f;

            barwaktu.value = 0f;
            barwaktu.gameObject.SetActive(true);
            if (TryGetComponent<JuiceAnimator>(out var juice)) juice.StartCookingWobble();
        }

        else if (statusKompor == 5 && Baso.dipegang == null)
        {

            Baso.dipegang = Instantiate(HoldBasoMateng, Baso.transform.position, Quaternion.identity);

            statusKompor = 0;
            barwaktu.gameObject.SetActive(false);
            if (TryGetComponent<JuiceAnimator>(out var juice)) juice.StopAnimation();
        }

    }
}
