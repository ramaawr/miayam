using UnityEngine;

public class InteraksiBaso : MonoBehaviour
{
    // Flag untuk menandai apakah bakso ini sedang "diangkat" / dipindahkan oleh user
    [HideInInspector]
    public bool sedangDipindahkan = false;
    private Vector3 posisiAwal;
    private float zAwal;

    void Update()
    {
        // Kalau bakso ini sedang dipindahkan, posisinya ikutin kursor mouse
        if (sedangDipindahkan)
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
                BatalkanPindah();
            }
        }
    }

    void OnMouseDown()
    {
        // biar kaga dabel kata gemini
        if (sedangDipindahkan)
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

        interaksisayur[] semuaSayur = FindObjectsOfType<interaksisayur>();
        foreach (interaksisayur sayur in semuaSayur)
        {
            if (sayur.sayursedangDipindahkan)
            {
                return; // Kalau lagi megang sayur, baso batal diangkat!
            }
        }

        //fix bug ngangkat 2 bao
        InteraksiBaso[] semuaBaso = FindObjectsOfType<InteraksiBaso>();
        foreach (InteraksiBaso baso in semuaBaso)
        {
            if (baso.sedangDipindahkan)
            {
                return;
            }
        }

        posisiAwal = transform.position;
        zAwal = transform.position.z;

        sedangDipindahkan = true;

        // biar ga bug ama muncul paling atas (bug muncul di z yg ngaco)
        Vector3 posSekarang = transform.position;
        posSekarang.z = -5f;
        transform.position = posSekarang;

        //pas di taro itu jadi default pas di angkat jadi raycast lagi
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        transform.SetParent(null);
    }

    public void BatalkanPindah()
    {
        sedangDipindahkan = false;
        //ini pas dia balik di cancel harus ke default lagi biar bisa di klik
        gameObject.layer = LayerMask.NameToLayer("Default");
       
        transform.position = posisiAwal;

        Mangkok mangkok = FindObjectOfType<Mangkok>();
        if (mangkok != null)
        {
            transform.SetParent(mangkok.transform);
        }
    }

    // Fungsi yang dipanggil oleh mangkok.cs saat user klik mangkok untuk meletakkan bakso
    // di posisi baru. Posisi sudah di-set di Update() (ikut kursor), jadi tinggal finalisasi.
    public void SelesaiPindah(Transform parentMangkok)
    {
        sedangDipindahkan = false;

        // layer balik ke default supaya bakso bisa diklik lagi
        gameObject.layer = LayerMask.NameToLayer("Default");

        Vector3 posFinal = transform.position;
        posFinal.z = zAwal;
        transform.position = posFinal;

        transform.SetParent(parentMangkok);
    }
}
