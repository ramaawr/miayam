using UnityEngine;

public class interaksiayam : MonoBehaviour
{
    [HideInInspector]
    public bool ayamsedangDipindahkan = false;
    private Vector3 posisiAwal;
    private float zAwal;

    void Update()
    {
        if (ayamsedangDipindahkan)
        {
            Vector3 posisiLayar = Input.mousePosition;
            posisiLayar.z = 10f;
            Vector3 posisiMouse = Camera.main.ScreenToWorldPoint(posisiLayar);

            posisiMouse.z = zAwal;
            transform.position = posisiMouse;

            float putaranMouse = Input.mouseScrollDelta.y;
            if (putaranMouse != 0)
            {
                transform.Rotate(0, 0, putaranMouse * 15f);
            }

            if (Input.GetMouseButtonDown(1))
            {
                BatalkanPindahayam();
            }
        }
    }

    void OnMouseDown()
    {
        if (ayamsedangDipindahkan)
        {
            return;
        }

        bidcontrol[] semuaBak = FindObjectsOfType<bidcontrol>();
        foreach (bidcontrol bak in semuaBak)
        {
            if (bak.dipegang != null)
            {
                return;
            }
        }

        interaksisayur[] semuasayur = FindObjectsOfType<interaksisayur>();
        foreach (interaksisayur sayur in semuasayur)
        {
            if (sayur.sayursedangDipindahkan)
            {
                return;
            }
        }

        InteraksiBaso[] semuaBasoTopping = FindObjectsOfType<InteraksiBaso>();
        foreach (InteraksiBaso baso in semuaBasoTopping)
        {
            if (baso.sedangDipindahkan)
            {
                return;
            }
        }

        interaksiayam[] semuaayam = FindObjectsOfType<interaksiayam>();
        foreach (interaksiayam ayam in semuaayam)
        {
            if (ayam.ayamsedangDipindahkan)
            {
                return;
            }
        }

        posisiAwal = transform.position;
        zAwal = transform.position.z;

        ayamsedangDipindahkan = true;

        Vector3 posSekarang = transform.position;
        posSekarang.z = -5f;
        transform.position = posSekarang;

        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        transform.SetParent(null);
    }

    public void BatalkanPindahayam()
    {
        ayamsedangDipindahkan = false;
        gameObject.layer = LayerMask.NameToLayer("Default");
        transform.position = posisiAwal;

        Mangkok mangkok = FindObjectOfType<Mangkok>();
        if (mangkok != null)
        {
            transform.SetParent(mangkok.transform);
        }
    }

    public void ayamSelesaiPindah(Transform parentMangkok)
    {
        ayamsedangDipindahkan = false;
        gameObject.layer = LayerMask.NameToLayer("Default");
        Vector3 posFinal = transform.position;
        posFinal.z = zAwal;
        transform.position = posFinal;

        transform.SetParent(parentMangkok);
    }
}
