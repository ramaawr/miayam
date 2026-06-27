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

    void Start()
    {

    }

    void Update()
    {

    }

    void OnMouseDown()
    {
        // ============================================================
        // PRIORITAS 1: Cek apakah ada bakso yang sedang dipindahkan.
        // Kalau ada, letakkan bakso itu di posisi kursor saat ini.
        // Ini HARUS dicek duluan sebelum logika lain, supaya klik di mangkok
        // tidak malah menambah bakso baru saat user cuma mau mindahin bakso.
        // ============================================================

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
            Debug.Log("Bakso berhasil dipindahkan ke posisi baru di mangkok!");
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
        if (Mangkokterisi == true && Mangkokterisiayam == false && Ayam.dipegang != null && Ayam.dipegang.name.Contains("ayam"))
        {
            Destroy(Ayam.dipegang);
            Ayam.dipegang = null;
            
            Visualayamdimangkok.SetActive(true);
            Mangkokterisiayam = true;
            TotalAyam = TotalAyam + 1;
            Debug.Log("ada ayam nih" + TotalAyam);
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
}
