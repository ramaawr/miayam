using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

// =============================================================================
// MainMenuManager.cs — Manajer utama untuk navigasi dan transisi Main Menu
// =============================================================================
// Tempelkan script ini ke objek manajer di scene Main Menu (misal Canvas atau MainMenuManager).
// Mengontrol pembukaan panel sub-menu (Pengaturan, Level) dan perpindahan scene
// dengan efek transisi memudar (Fade In/Out) yang halus.
// =============================================================================
public class MainMenuManager : MonoBehaviour
{
    [Header("Pengaturan Scene")]
    [Tooltip("Nama scene gameplay yang akan dimuat saat tombol Mulai ditekan")]
    public string namaSceneGameplay = "newest";

    [Header("Panel Sub-Menu")]
    [Tooltip("Panel utama yang berisi tombol-tombol menu awal")]
    public GameObject panelUtama;

    [Tooltip("Panel menu pengaturan (Options)")]
    public GameObject panelPengaturan;

    [Tooltip("Panel menu pilihan level (jika ada)")]
    public GameObject panelLevel;

    [Header("Efek Transisi (Fade)")]
    [Tooltip("CanvasGroup gambar penutup hitam/putih untuk transisi fade")]
    public CanvasGroup faderCanvasGroup;

    [Tooltip("Durasi waktu transisi memudar (detik)")]
    public float durasiFade = 1.0f;

    private bool isTransitioning = false;

    private void Start()
    {
        // Pastikan panel aktif dengan benar di awal game
        if (panelUtama != null) panelUtama.SetActive(true);
        if (panelPengaturan != null) panelPengaturan.SetActive(false);
        if (panelLevel != null) panelLevel.SetActive(false);

        // Jika ada CanvasGroup Fader, lakukan transisi masuk (Fade In)
        if (faderCanvasGroup != null)
        {
            faderCanvasGroup.gameObject.SetActive(true);
            faderCanvasGroup.alpha = 1f;
            faderCanvasGroup.blocksRaycasts = true; // Halangi input selama transisi
            StartCoroutine(FadeScreen(0f, () => {
                faderCanvasGroup.blocksRaycasts = false; // Buka input kembali setelah selesai
            }));
        }
    }

    // =========================================================================
    // Fungsi Navigasi Utama (Dipanggil oleh Tombol UI)
    // =========================================================================

    // Fungsi untuk memulai game dengan efek transisi
    public void MulaiGame()
    {
        if (isTransitioning) return;

        Debug.Log("MainMenuManager: Memulai game, bersiap memuat scene: " + namaSceneGameplay);
        
        if (faderCanvasGroup != null)
        {
            faderCanvasGroup.blocksRaycasts = true;
            StartCoroutine(FadeScreen(1f, () => {
                SceneManager.LoadScene(namaSceneGameplay);
            }));
        }
        else
        {
            SceneManager.LoadScene(namaSceneGameplay);
        }
    }

    // Buka Panel Pengaturan
    public void BukaPanelPengaturan()
    {
        if (panelUtama != null) panelUtama.SetActive(false);
        if (panelPengaturan != null) panelPengaturan.SetActive(true);
        if (panelLevel != null) panelLevel.SetActive(false);
    }

    // Tutup Panel Pengaturan (Kembali ke menu utama)
    public void TutupPanelPengaturan()
    {
        if (panelUtama != null) panelUtama.SetActive(true);
        if (panelPengaturan != null) panelPengaturan.SetActive(false);
        if (panelLevel != null) panelLevel.SetActive(false);
    }

    // Buka Panel Pilihan Level
    public void BukaPanelLevel()
    {
        if (panelUtama != null) panelUtama.SetActive(false);
        if (panelPengaturan != null) panelPengaturan.SetActive(false);
        if (panelLevel != null) panelLevel.SetActive(true);
    }

    // Tutup Panel Level (Kembali ke menu utama)
    public void TutupPanelLevel()
    {
        if (panelUtama != null) panelUtama.SetActive(true);
        if (panelPengaturan != null) panelPengaturan.SetActive(false);
        if (panelLevel != null) panelLevel.SetActive(false);
    }

    // Keluar dari game
    public void KeluarGame()
    {
        Debug.Log("MainMenuManager: Mengeluarkan game...");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Menghentikan playmode di Editor
        #else
        Application.Quit(); // Menutup game di build PC/Android
        #endif
    }

    // =========================================================================
    // Coroutine untuk Transisi Layar (Fade Screen)
    // =========================================================================
    private IEnumerator FadeScreen(float targetAlpha, System.Action onComplete)
    {
        isTransitioning = true;
        float alphaAwal = faderCanvasGroup.alpha;
        float waktuBerjalan = 0f;

        while (waktuBerjalan < durasiFade)
        {
            waktuBerjalan += Time.deltaTime;
            // Gunakan interpolasi linear (Lerp) untuk transisi yang mulus
            faderCanvasGroup.alpha = Mathf.Lerp(alphaAwal, targetAlpha, waktuBerjalan / durasiFade);
            yield return null;
        }

        faderCanvasGroup.alpha = targetAlpha;
        isTransitioning = false;
        
        // Panggil callback setelah transisi selesai
        if (onComplete != null)
        {
            onComplete.Invoke();
        }
    }
}
