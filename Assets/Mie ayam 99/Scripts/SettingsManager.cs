using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// =============================================================================
// SettingsManager.cs — Sistem Pengaturan Game Lengkap & Modular
// =============================================================================
// Mengontrol pengaturan Game Sound (SFX), Music Sound (Musik), Batasan FPS,
// Mode Tampilan (Layar Penuh / Jendela), dan Resolusi Layar.
// Menyimpan pilihan otomatis menggunakan PlayerPrefs agar persisten.
// =============================================================================
public class SettingsManager : MonoBehaviour
{
    [Header("Komponen UI Audio")]
    [Tooltip("Slider untuk mengatur volume suara Game (SFX)")]
    public Slider sliderVolumeGame;

    [Tooltip("Slider untuk mengatur volume suara Musik")]
    public Slider sliderVolumeMusic;

    [Header("Komponen UI Tampilan & Performa")]
    [Tooltip("Dropdown untuk memilih Mode Tampilan (Layar Penuh / Jendela)")]
    public TMP_Dropdown dropdownDisplayMode;

    [Tooltip("Dropdown untuk memilih Batasan FPS")]
    public TMP_Dropdown dropdownFPS;

    [Tooltip("Dropdown untuk memilih resolusi layar")]
    public TMP_Dropdown dropdownResolusi;

    // Daftar resolusi unik yang didukung monitor
    private List<Resolution> daftarResolusiUnik = new List<Resolution>();

    // Batasan FPS yang didukung (30, 60, 120, -1 untuk Tanpa Batas)
    private readonly int[] opsiFPS = new int[] { 30, 60, 120, -1 };

    // Nama key untuk penyimpanan PlayerPrefs
    private const string KEY_VOLUME_GAME = "VolumeGame";
    private const string KEY_VOLUME_MUSIC = "VolumeMusic";
    private const string KEY_DISPLAY_MODE = "DisplayMode";
    private const string KEY_FPS_LIMIT = "FPSLimit";
    private const string KEY_RESOLUSI = "ResolusiLayar";

    private void Start()
    {
        // 1. Inisialisasi Pilihan Resolusi Layar
        InisialisasiDropdownResolusi();

        // 2. Inisialisasi Dropdown Statis (Display Mode & FPS) jika kosong
        InisialisasiDropdownStatis();

        // 3. Muat pengaturan yang tersimpan atau gunakan nilai default
        MuatDanTerapkanPengaturan();

        // 4. Daftarkan event listener secara dinamis agar langsung berjalan otomatis
        if (sliderVolumeGame != null)
            sliderVolumeGame.onValueChanged.AddListener(SetVolumeGame);

        if (sliderVolumeMusic != null)
            sliderVolumeMusic.onValueChanged.AddListener(SetVolumeMusic);

        if (dropdownDisplayMode != null)
            dropdownDisplayMode.onValueChanged.AddListener(SetDisplayMode);

        if (dropdownFPS != null)
            dropdownFPS.onValueChanged.AddListener(SetFPSLimit);

        if (dropdownResolusi != null)
            dropdownResolusi.onValueChanged.AddListener(SetResolusi);
    }

    // =========================================================================
    // Inisialisasi Pilihan Dropdown
    // =========================================================================
    
    private void InisialisasiDropdownResolusi()
    {
        if (dropdownResolusi == null) return;

        dropdownResolusi.ClearOptions();

        Resolution[] semuaResolusi = Screen.resolutions;
        List<string> opsiTeksResolusi = new List<string>();
        HashSet<string> resolusiTerdaftar = new HashSet<string>();

        daftarResolusiUnik.Clear();

        for (int i = 0; i < semuaResolusi.Length; i++)
        {
            string formatResolusi = semuaResolusi[i].width + " x " + semuaResolusi[i].height;
            
            if (!resolusiTerdaftar.Contains(formatResolusi))
            {
                resolusiTerdaftar.Add(formatResolusi);
                daftarResolusiUnik.Add(semuaResolusi[i]);
                opsiTeksResolusi.Add(formatResolusi);
            }
        }

        dropdownResolusi.AddOptions(opsiTeksResolusi);
    }

    private void InisialisasiDropdownStatis()
    {
        // Inisialisasi Dropdown Mode Tampilan
        if (dropdownDisplayMode != null)
        {
            dropdownDisplayMode.ClearOptions();
            List<string> opsiDisplay = new List<string> { "Layar Penuh (Fullscreen)", "Mode Jendela (Windowed)" };
            dropdownDisplayMode.AddOptions(opsiDisplay);
        }

        // Inisialisasi Dropdown FPS
        if (dropdownFPS != null)
        {
            dropdownFPS.ClearOptions();
            List<string> opsiFPSLabel = new List<string> { "30 FPS (Hemat Baterai)", "60 FPS (Standar)", "120 FPS (Sangat Mulus)", "Tanpa Batas (Unlimited)" };
            dropdownFPS.AddOptions(opsiFPSLabel);
        }
    }

    // =========================================================================
    // Memuat dan Menerapkan Pengaturan dari PlayerPrefs
    // =========================================================================
    private void MuatDanTerapkanPengaturan()
    {
        // 1. Muat & Terapkan Volume Game (Default: 1.0)
        float volGame = PlayerPrefs.GetFloat(KEY_VOLUME_GAME, 1.0f);
        if (sliderVolumeGame != null) sliderVolumeGame.value = volGame;
        // Catatan: Anda bisa menggunakan nilai ini untuk mengatur AudioSource SFX di game Anda nantinya.

        // 2. Muat & Terapkan Volume Musik (Default: 0.8)
        float volMusic = PlayerPrefs.GetFloat(KEY_VOLUME_MUSIC, 0.8f);
        if (sliderVolumeMusic != null) sliderVolumeMusic.value = volMusic;
        // Catatan: Nilai ini bisa dibaca oleh pemutar musik latar belakang (BGM).
        
        // Atur volume listener global ke nilai rata-rata atau master untuk sementara waktu
        AudioListener.volume = (volGame + volMusic) / 2f;

        // 3. Muat & Terapkan Mode Tampilan (Default: 0 - Fullscreen)
        int modeTampilan = PlayerPrefs.GetInt(KEY_DISPLAY_MODE, 0);
        if (dropdownDisplayMode != null) dropdownDisplayMode.value = modeTampilan;
        bool isFullscreen = (modeTampilan == 0);
        Screen.fullScreen = isFullscreen;

        // 4. Muat & Terapkan Batasan FPS (Default: 1 - 60 FPS)
        int indeksFPS = PlayerPrefs.GetInt(KEY_FPS_LIMIT, 1);
        if (dropdownFPS != null) dropdownFPS.value = indeksFPS;
        Application.targetFrameRate = opsiFPS[indeksFPS];

        // 5. Muat & Terapkan Resolusi Layar (Default: Resolusi tertinggi layar)
        if (dropdownResolusi != null && daftarResolusiUnik.Count > 0)
        {
            int indeksDefault = CariIndeksResolusiSaatIni();
            int indeksResolusiDisimpan = PlayerPrefs.GetInt(KEY_RESOLUSI, indeksDefault);

            if (indeksResolusiDisimpan >= daftarResolusiUnik.Count)
            {
                indeksResolusiDisimpan = daftarResolusiUnik.Count - 1;
            }

            dropdownResolusi.value = indeksResolusiDisimpan;
            dropdownResolusi.RefreshShownValue();

            Resolution res = daftarResolusiUnik[indeksResolusiDisimpan];
            Screen.SetResolution(res.width, res.height, isFullscreen);
        }
    }

    // =========================================================================
    // Fungsi Pengubah Pengaturan (Dipanggil oleh Event Slider/Dropdown)
    // =========================================================================

    public void SetVolumeGame(float volume)
    {
        PlayerPrefs.SetFloat(KEY_VOLUME_GAME, volume);
        PlayerPrefs.Save();
        
        // Sesuaikan volume global AudioListener berdasarkan rata-rata
        float volMusic = PlayerPrefs.GetFloat(KEY_VOLUME_MUSIC, 0.8f);
        AudioListener.volume = (volume + volMusic) / 2f;

        Debug.Log("SettingsManager: Volume Suara Game (SFX) diatur ke " + (volume * 100f).ToString("F0") + "%");
    }

    public void SetVolumeMusic(float volume)
    {
        PlayerPrefs.SetFloat(KEY_VOLUME_MUSIC, volume);
        PlayerPrefs.Save();

        // Sesuaikan volume global AudioListener berdasarkan rata-rata
        float volGame = PlayerPrefs.GetFloat(KEY_VOLUME_GAME, 1.0f);
        AudioListener.volume = (volGame + volume) / 2f;

        Debug.Log("SettingsManager: Volume Musik diatur ke " + (volume * 100f).ToString("F0") + "%");
    }

    public void SetDisplayMode(int indeksMode)
    {
        bool isFullscreen = (indeksMode == 0);
        Screen.fullScreen = isFullscreen;

        PlayerPrefs.SetInt(KEY_DISPLAY_MODE, indeksMode);
        PlayerPrefs.Save();

        // Terapkan ulang resolusi saat ini agar mode display stabil
        if (dropdownResolusi != null && dropdownResolusi.value < daftarResolusiUnik.Count)
        {
            Resolution res = daftarResolusiUnik[dropdownResolusi.value];
            Screen.SetResolution(res.width, res.height, isFullscreen);
        }

        Debug.Log("SettingsManager: Mode Tampilan diatur ke " + (isFullscreen ? "Layar Penuh" : "Mode Jendela"));
    }

    public void SetFPSLimit(int indeksFPS)
    {
        if (indeksFPS < 0 || indeksFPS >= opsiFPS.Length) return;

        int limitTarget = opsiFPS[indeksFPS];
        Application.targetFrameRate = limitTarget;

        PlayerPrefs.SetInt(KEY_FPS_LIMIT, indeksFPS);
        PlayerPrefs.Save();

        string fpsText = limitTarget == -1 ? "Tanpa Batas" : limitTarget + " FPS";
        Debug.Log("SettingsManager: Batasan FPS diatur ke " + fpsText);
    }

    public void SetResolusi(int indeksResolusi)
    {
        if (indeksResolusi < 0 || indeksResolusi >= daftarResolusiUnik.Count) return;

        Resolution res = daftarResolusiUnik[indeksResolusi];
        bool isFullscreen = Screen.fullScreen;

        Screen.SetResolution(res.width, res.height, isFullscreen);

        PlayerPrefs.SetInt(KEY_RESOLUSI, indeksResolusi);
        PlayerPrefs.Save();

        Debug.Log("SettingsManager: Resolusi Layar diatur ke " + res.width + "x" + res.height);
    }

    private int CariIndeksResolusiSaatIni()
    {
        int lebarSaatIni = Screen.width;
        int tinggiSaatIni = Screen.height;

        for (int i = 0; i < daftarResolusiUnik.Count; i++)
        {
            if (daftarResolusiUnik[i].width == lebarSaatIni && daftarResolusiUnik[i].height == tinggiSaatIni)
            {
                return i;
            }
        }
        return daftarResolusiUnik.Count - 1;
    }
}
