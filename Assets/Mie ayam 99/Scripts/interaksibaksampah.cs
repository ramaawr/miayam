using MCPForUnity.Editor.Tools;
using UnityEngine;

public class interaksibaksampah : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnMouseDown()
    {
        bidcontrol[] SemuaBak = FindObjectsOfType<bidcontrol>();

        foreach (bidcontrol BakAktif in SemuaBak)
        {
            if (BakAktif.dipegang != null)
            {
                Destroy(BakAktif.dipegang);
                BakAktif.dipegang = null;
                return;
            }
        }

        InteraksiBaso[] semuaBaso = FindObjectsOfType<InteraksiBaso>();
        foreach (InteraksiBaso baso in semuaBaso)
        {
            if (baso.sedangDipindahkan == true)
            {
                Destroy(baso.gameObject);
                baso.sedangDipindahkan = false;

                Mangkok mangkoks = FindObjectOfType<Mangkok>();
                if (mangkoks != null)
                {
                    mangkoks.Totalbaso -= 1;
                    Debug.Log("Baso dibuang! Sisa memori baso di mangkok: " + mangkoks.Totalbaso);
                }
                return;
            }
        }

        interaksisayur[] semuasayur = FindObjectsOfType<interaksisayur>();
        foreach (interaksisayur sayur in semuasayur)
        {
            if (sayur.sayursedangDipindahkan == true)
            {
                Destroy(sayur.gameObject);
                sayur.sayursedangDipindahkan = false;

                Mangkok mangkoks = FindObjectOfType<Mangkok>();
                if (mangkoks != null)
                {
                    mangkoks.TotalSayur -= 1;
                    Debug.Log("Sayur dibuang! Sisa memori baso di mangkok: " + mangkoks.TotalSayur);
                }
                return;
            }
        }
    }
}