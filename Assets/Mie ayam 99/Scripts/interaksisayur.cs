using UnityEngine;

public class interaksisayur : MonoBehaviour
{
    [HideInInspector]
    public bool sayursedangDipindahkan = false;
    private Vector3 posisiAwal;
    private float zAwal;

    

    // Update is called once per frame
    void Update()
    {
        if (sayursedangDipindahkan)
        {
            Vector3 posisiLayar = Input.mousePosition;
            posisiLayar.z = 10f;
            Vector3 posisiMouse = Camera.main.ScreenToWorldPoint(posisiLayar);

            // Pakai z awal biar bakso tetap di layer yang sama, tidak loncat ke depan/belakang
            posisiMouse.z = zAwal;

            // Pindahkan bakso ke posisi mouse
            transform.position = posisiMouse;

            float putaranMouse = Input.mouseScrollDelta.y;

            if (putaranMouse != 0)
            {
                // Putar dirinya sendiri (transform sayur ini) di sumbu Z
                transform.Rotate(0, 0, putaranMouse * 15f);
            }

            // Kalau user klik kanan saat sedang mindahin bakso → batalkan, kembalikan ke posisi semula
            if (Input.GetMouseButtonDown(1))
            {
                BatalkanPindahsayur();
            }
        }
    }
    void OnMouseDown()
    {
        
        if (sayursedangDipindahkan)
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
                return; // Kalau lagi megang baso, sayur batal diangkat!
            }
        }

        posisiAwal = transform.position;
        zAwal = transform.position.z;

        sayursedangDipindahkan = true;

        // biar ga bug ama muncul paling atas (bug muncul di z yg ngaco)
        Vector3 posSekarang = transform.position;
        posSekarang.z = -5f;
        transform.position = posSekarang;

        //pas di taro itu jadi default pas di angkat jadi raycast lagi
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        transform.SetParent(null);

    }

    public void BatalkanPindahsayur()
    {
        sayursedangDipindahkan = false;
        //ini pas dia balik di cancel harus ke default lagi biar bisa di klik
        gameObject.layer = LayerMask.NameToLayer("Default");

        transform.position = posisiAwal;

        Mangkok mangkok = FindObjectOfType<Mangkok>();
        if (mangkok != null)
        {
            transform.SetParent(mangkok.transform);
        }
    }

    public void sayurSelesaiPindah(Transform parentMangkok)
    {
        sayursedangDipindahkan = false;

        // layer balik ke default supaya bakso bisa diklik lagi
        gameObject.layer = LayerMask.NameToLayer("Default");

        Vector3 posFinal = transform.position;
        posFinal.z = zAwal;
        transform.position = posFinal;

        transform.SetParent(parentMangkok);
    }
}

