using UnityEngine;
using System.Collections;

public class Mangkok : MonoBehaviour
{
    public bidcontrol Mie;
    public bidcontrol Baso;
    public bidcontrol Ayam;
    public bidcontrol Sayur;
    public GameObject Visualmiedimangkok;
    public GameObject Visualayamdimangkok;

    public int Totalbaso = 0;
    public int TotalAyam = 0;
    public int TotalSayur = 0;

    private bool Mangkokterisi = false;
    private bool Mangkokterisiayam = false;
    private bool Mangkokterisisayur = false;


    public bool AdaMie
    {
        get { return Mangkokterisi; }
    }

   
    public void KosongkanMangkokDapur()
    {
        // 1. Reset semua flag boolean
        Mangkokterisi = false;
        Mangkokterisiayam = false;
        Mangkokterisisayur = false;

        // 2. Reset semua counter
        Totalbaso = 0;
        TotalAyam = 0;
        TotalSayur = 0;

        // 3. Matikan visual mie dan ayam di mangkok
        if (Visualmiedimangkok != null)
        {
            Visualmiedimangkok.SetActive(false);
        }
        if (Visualayamdimangkok != null)
        {
            Visualayamdimangkok.SetActive(false);
        }

        // 4. Hapus semua klon topping (bakso & sayur) yang ada di mangkok
        // Loop mundur (dari belakang) supaya index transform child aman saat dihapus
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform anak = transform.GetChild(i);

            // Cek apakah child ini adalah topping (punya InteraksiBaso, interaksisayur, atau interaksiayam)
            bool adalahBaso = anak.GetComponent<InteraksiBaso>() != null;
            bool adalahSayur = anak.GetComponent<interaksisayur>() != null;
            bool adalahAyam = anak.GetComponent<interaksiayam>() != null;

            if (adalahBaso || adalahSayur || adalahAyam)
            {
                Destroy(anak.gameObject);
            }
        }

        Debug.Log("Mangkok dapur berhasil dikosongkan secara total (visual & data).");
    }


    public void ResetMangkok()
    {
        KosongkanMangkokDapur();
    }


    void Start()
    {

    }

    void Update()
    {

    }

    void OnMouseDown()
    {
        
        //baso
        InteraksiBaso basoYangDipindah = CariBasoYangSedangDipindahkan();
        if (basoYangDipindah != null)
        {
            // Panggil fungsi SelesaiPindah di InteraksiBaso
            // Ini akan: set flag false, kembalikan z, dan set parent ke mangkok
            basoYangDipindah.SelesaiPindah(this.transform);
            Debug.Log("Bakso berhasil dipindahkan ke posisi baru di mangkok!");
            return; 
        }

        //sayur
        interaksisayur sayurYangDipindah = CariSayurYangSedangDipindahkan();
        if (sayurYangDipindah != null)
        {
            // Panggil fungsi SelesaiPindah di InteraksiBaso
            // Ini akan: set flag false, kembalikan z, dan set parent ke mangkok
            sayurYangDipindah.sayurSelesaiPindah(this.transform);
            Debug.Log("Sayur berhasil dipindahkan ke posisi baru di mangkok!");
            return;
        }

        //ayam
        interaksiayam ayamYangDipindah = CariAyamYangSedangDipindahkan();
        if (ayamYangDipindah != null)
        {
            ayamYangDipindah.ayamSelesaiPindah(this.transform);
            Debug.Log("Ayam berhasil dipindahkan ke posisi baru di mangkok!");
            return;
        }

        //munculin hiden objek

        //mie
        if (Mangkokterisi == false && Mie.dipegang != null && Mie.dipegang.name.Contains("HoldMieMateng"))
        {
            Destroy(Mie.dipegang);
            Mie.dipegang = null;
            Mangkokterisi = true;
            Visualmiedimangkok.SetActive(true);
        }

        //ayyyam
        if (Mangkokterisi == true && Ayam.dipegang != null && Ayam.dipegang.name.Contains("ayam"))
        {
            GameObject Placedayam = Ayam.dipegang;
            Ayam.dipegang = null;

            Placedayam.transform.SetParent(this.transform);

            Vector3 posAyam = Placedayam.transform.localPosition;
            posAyam.z = -1f;
            Placedayam.transform.localPosition = posAyam;

            if (Placedayam.GetComponent<interaksiayam>() == null)
            {
                Placedayam.AddComponent<interaksiayam>();
            }

            if (Placedayam.GetComponent<Collider2D>() == null)
            {
                Placedayam.AddComponent<PolygonCollider2D>();
            }

            Placedayam.layer = LayerMask.NameToLayer("Default");

            Mangkokterisiayam = true;
            TotalAyam = TotalAyam + 1;
            Debug.Log("ada ayam nih: " + TotalAyam);
        }


        // ============================================================
        // PRIORITAS 3: Taruh bakso mateng baru ke mangkok
        // ============================================================
        if (Mangkokterisi == true && Baso.dipegang != null && Baso.dipegang.name.Contains("HoldBasoMateng"))
        {
            GameObject Placedbaso = Baso.dipegang;
            Baso.dipegang = null;

            // Jadikan bakso anak dari mangkok supaya ikut posisi mangkok
            Placedbaso.transform.SetParent(this.transform);

            // Tambahkan Z-axis agar collider bakso berada di depan mangkok
            // (Tameng pelindung dari bentrok collider / overlap)
            Vector3 posBaso = Placedbaso.transform.localPosition;
            posBaso.z = -1f;
            Placedbaso.transform.localPosition = posBaso;

            // Tambahkan komponen InteraksiBaso ke bakso yang baru ditaruh,
            // supaya bakso ini bisa diklik dan dipindahkan nanti.
            // Cek dulu apakah sudah punya komponen ini (jaga-jaga kalau prefab sudah ada)
            if (Placedbaso.GetComponent<InteraksiBaso>() == null)
            {
                Placedbaso.AddComponent<InteraksiBaso>();
            }

            // Tambahkan Collider2D kalau belum ada, supaya bakso bisa diklik
            // Tanpa collider, OnMouseDown di InteraksiBaso tidak akan jalan
            if (Placedbaso.GetComponent<Collider2D>() == null)
            {
                Placedbaso.AddComponent<CircleCollider2D>();
            }

            // PENTING: Ganti layer bakso dari "Ignore Raycast" ke "Default".
            // Kenapa? Karena saat bakso dibawa kursor (dari kompor), layer-nya "Ignore Raycast"
            // supaya tidak menghalangi klik ke mangkok. Tapi setelah ditaruh di mangkok,
            // bakso harus bisa diklik untuk dipindahkan, jadi layer-nya harus "Default".
            Placedbaso.layer = LayerMask.NameToLayer("Default");

            Totalbaso = Totalbaso + 1;
            Debug.Log("Total Baso di mangkok sekarang: " + Totalbaso);
        }

        //sayur
        if (Mangkokterisi == true && Sayur.dipegang != null && Sayur.dipegang.name.Contains("sayur"))
        {
            GameObject Placedsayur = Sayur.dipegang;
            Sayur.dipegang = null;

            // Jadikan bakso anak dari mangkok supaya ikut posisi mangkok
            Placedsayur.transform.SetParent(this.transform);

            // Tambahkan Z-axis agar collider sayur berada di depan mangkok
            // (Tameng pelindung dari bentrok collider / overlap)
            Vector3 posSayur = Placedsayur.transform.localPosition;
            posSayur.z = -1f;
            Placedsayur.transform.localPosition = posSayur;

            // Tambahkan komponen interaksisayur ke sayur yang baru ditaruh,
            // supaya sayur ini bisa diklik dan dipindahkan nanti.
            // Cek dulu apakah sudah punya komponen ini (jaga-jaga kalau prefab sudah ada)
            if (Placedsayur.GetComponent<interaksisayur>() == null)
            {
                Placedsayur.AddComponent<interaksisayur>();
            }

            // Tambahkan Collider2D kalau belum ada, supaya bakso bisa diklik
            // Tanpa collider, OnMouseDown di InteraksiBaso tidak akan jalan
            if (Placedsayur.GetComponent<Collider2D>() == null)
            {
                Placedsayur.AddComponent<PolygonCollider2D>();
            }

            // PENTING: Ganti layer bakso dari "Ignore Raycast" ke "Default".
            // Kenapa? Karena saat bakso dibawa kursor (dari kompor), layer-nya "Ignore Raycast"
            // supaya tidak menghalangi klik ke mangkok. Tapi setelah ditaruh di mangkok,
            // bakso harus bisa diklik untuk dipindahkan, jadi layer-nya harus "Default".
            Placedsayur.layer = LayerMask.NameToLayer("Default");

            TotalSayur = TotalSayur + 1;
            Debug.Log("Total SAYUR di mangkok sekarang: " + TotalSayur);
        }
    }

    // Fungsi pembantu: cari apakah ada bakso anak mangkok yang sedang dipindahkan
    // Mengecek semua InteraksiBaso yang ada di scene
    // baso
    private InteraksiBaso CariBasoYangSedangDipindahkan()
    {
        InteraksiBaso[] semuaBaso = FindObjectsOfType<InteraksiBaso>();

        foreach (InteraksiBaso baso in semuaBaso)
        {
            // Kalau ketemu bakso yang flag-nya true, berarti user sedang mindahin bakso ini
            if (baso.sedangDipindahkan)
            {
                return baso;
            }
        }

        // Tidak ada bakso yang sedang dipindahkan
        return null;
    }
    //sayur
    private interaksisayur CariSayurYangSedangDipindahkan()
    {
        interaksisayur[] semuasayur = FindObjectsOfType<interaksisayur>();

        foreach (interaksisayur sayur in semuasayur)
        {
            // Kalau ketemu bakso yang flag-nya true, berarti user sedang mindahin bakso ini
            if (sayur.sayursedangDipindahkan)
            {
                return sayur;
            }
        }

        // Tidak ada bakso yang sedang dipindahkan
        return null;
    }

    private interaksiayam CariAyamYangSedangDipindahkan()
    {
        interaksiayam[] semuaayam = FindObjectsOfType<interaksiayam>();

        foreach (interaksiayam ayam in semuaayam)
        {
            if (ayam.ayamsedangDipindahkan)
            {
                return ayam;
            }
        }

        return null;
    }
}
