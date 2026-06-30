using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Wajib untuk mengakses UI Slider

public class NPCMoralitas : MonoBehaviour
{
    [Header("Pengaturan Moralitas")]
    // Nilai moralitas awal saat game/npc ini dimulai
    [SerializeField] private int moralitasSaatIni = 50;
    
    // Batas maksimal moralitas yang bisa dicapai (contoh: 100)
    [SerializeField] private int moralitasMaksimal = 100;

    [Header("Referensi UI")]
    // Referensi ke UI Slider yang bertugas menjadi 'Bar' (seperti Health Bar)
    [SerializeField] private Slider sliderMoralitas;

    void Start()
    {
        // Pastikan konfigurasi maksimal di slider sesuai dengan variabel kita
        if (sliderMoralitas != null)
        {
            sliderMoralitas.maxValue = moralitasMaksimal;
            sliderMoralitas.value = moralitasSaatIni;
        }
        else
        {
            Debug.LogWarning("UI Slider Moralitas belum dimasukkan di Inspector!");
        }
    }

    // Fungsi publik ini bisa dipanggil oleh SistemDialog untuk menambah atau mengurangi moralitas
    // Contoh: UbahMoralitas(10) untuk menambah, UbahMoralitas(-10) untuk mengurangi
    public void UbahMoralitas(int efekNilai)
    {
        // Tambahkan efek ke moralitas saat ini
        moralitasSaatIni += efekNilai;

        // Gunakan Mathf.Clamp untuk mengunci nilai agar tidak kurang dari 0 dan tidak lebih dari maksimal
        moralitasSaatIni = Mathf.Clamp(moralitasSaatIni, 0, moralitasMaksimal);

        // Update tampilan slider di UI
        if (sliderMoralitas != null)
        {
            sliderMoralitas.value = moralitasSaatIni;
        }

        // Tampilkan informasi ke console untuk membantu debugging
        Debug.Log("Moralitas NPC berubah menjadi: " + moralitasSaatIni);
    }
}
