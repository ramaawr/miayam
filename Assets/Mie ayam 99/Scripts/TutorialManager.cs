using UnityEngine;
using UnityEngine.UI;

// =============================================================================
// TutorialManager.cs — Pengatur Sistem Popup Tutorial (Banyak Halaman)
// =============================================================================
// Script ini digunakan untuk membuat popup tutorial berlapis (multi-page).
// Sangat mudah digunakan karena setiap halaman adalah GameObject terpisah.
// Jadi Anda bisa mendesain teks dan gambar sebebas mungkin di dalam Canvas.
// =============================================================================

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;

    [Header("Referensi UI Utama")]
    [Tooltip("Wadah/Panel utama popup tutorial yang akan dimunculkan/disembunyikan")]
    public GameObject PanelTutorialUtama;

    [Header("Daftar Halaman")]
    [Tooltip("Masukkan semua grup UI (GameObject) halaman tutorial secara berurutan. Halaman 1 di Element 0, dst.")]
    // Kita menggunakan array GameObject agar developer bisa mendesain setiap halaman
    // dengan bebas (bisa berisi kombinasi teks, gambar, atau animasi apa saja).
    public GameObject[] DaftarHalaman;

    [Header("Tombol Navigasi")]
    [Tooltip("Tombol UI untuk mundur ke halaman sebelumnya")]
    public Button TombolSebelumnya;
    
    [Tooltip("Tombol UI untuk maju ke halaman berikutnya")]
    public Button TombolBerikutnya;
    
    [Tooltip("Tombol UI untuk menutup panel tutorial (tombol X)")]
    public Button TombolTutup;

    // Mengingat pemain sedang berada di halaman ke berapa
    private int halamanSekarang = 0;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Pastikan popup tutorial tertutup saat game baru dimulai
        if (PanelTutorialUtama != null) 
        {
            PanelTutorialUtama.SetActive(false);
        }
    }

    // =========================================================================
    // FUNGSI UNTUK DIPANGGIL DARI TOMBOL (ON CLICK)
    // =========================================================================

    // Sambungkan fungsi ini ke Tombol Tanda Tanya (?) di layar
    public void BukaTutorial()
    {
        halamanSekarang = 0; // Selalu mulai dari halaman pertama
        
        if (PanelTutorialUtama != null) 
        {
            PanelTutorialUtama.SetActive(true);
        }
        
        PerbaruiTampilan();
    }

    // Sambungkan fungsi ini ke Tombol Tutup (X) di dalam popup
    public void TutupTutorial()
    {
        if (PanelTutorialUtama != null) 
        {
            PanelTutorialUtama.SetActive(false);
        }
    }

    // Sambungkan fungsi ini ke Tombol Berikutnya (Next)
    public void KeHalamanBerikutnya()
    {
        if (halamanSekarang < DaftarHalaman.Length - 1)
        {
            halamanSekarang++;
            PerbaruiTampilan();
        }
    }

    // Sambungkan fungsi ini ke Tombol Sebelumnya (Back)
    public void KeHalamanSebelumnya()
    {
        if (halamanSekarang > 0)
        {
            halamanSekarang--;
            PerbaruiTampilan();
        }
    }

    // =========================================================================
    // FUNGSI INTERNAL
    // =========================================================================
    private void PerbaruiTampilan()
    {
        // 1. Matikan (sembunyikan) semua halaman terlebih dahulu agar bersih
        for (int i = 0; i < DaftarHalaman.Length; i++)
        {
            if (DaftarHalaman[i] != null) 
            {
                DaftarHalaman[i].SetActive(false);
            }
        }

        // 2. Hanya hidupkan halaman yang sesuai dengan indeks saat ini
        if (DaftarHalaman.Length > 0 && DaftarHalaman[halamanSekarang] != null)
        {
            DaftarHalaman[halamanSekarang].SetActive(true);
        }

        // 3. Atur kemunculan tombol navigasi
        // Tombol "Back" hanya muncul jika bukan di halaman pertama
        if (TombolSebelumnya != null) 
        {
            TombolSebelumnya.gameObject.SetActive(halamanSekarang > 0);
        }
        
        // Tombol "Next" hanya muncul jika bukan di halaman terakhir
        if (TombolBerikutnya != null) 
        {
            TombolBerikutnya.gameObject.SetActive(halamanSekarang < DaftarHalaman.Length - 1);
        }
    }
}
