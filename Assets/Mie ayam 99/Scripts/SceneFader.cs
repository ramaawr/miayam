using UnityEngine;
using System.Collections;

// =============================================================================
// SceneFader.cs — Script sederhana untuk membuat efek transisi layar memudar
// =============================================================================
// Tempelkan script ini ke GameObject yang memiliki CanvasGroup (misal FaderOverlay).
// Script ini akan otomatis memudarkan layar dari hitam (alpha 1) ke transparan (alpha 0)
// ketika scene baru saja dimulai (dimuat).
// =============================================================================
public class SceneFader : MonoBehaviour
{
    [Header("Pengaturan Transisi")]
    [Tooltip("CanvasGroup yang digunakan untuk memudarkan layar")]
    public CanvasGroup faderCanvasGroup;

    [Tooltip("Durasi waktu layar memudar (dalam detik)")]
    public float durasiFade = 1.0f;

    [Tooltip("Apakah layar harus otomatis memudar saat scene baru dimulai?")]
    public bool fadeMulaiOtomatis = true;

    private void Start()
    {
        // Pastikan CanvasGroup sudah di-assign secara otomatis jika dikosongkan di Inspector
        if (faderCanvasGroup == null)
        {
            faderCanvasGroup = GetComponent<CanvasGroup>();
        }

        // Jalankan fade masuk (dari hitam ke transparan) jika diaktifkan
        if (fadeMulaiOtomatis && faderCanvasGroup != null)
        {
            StartCoroutine(FadeMasuk());
        }
    }

    // Coroutine untuk memudarkan layar dari hitam (1) ke transparan (0)
    public IEnumerator FadeMasuk()
    {
        // Set fader agar aktif menutupi seluruh layar dan menghalangi input
        faderCanvasGroup.gameObject.SetActive(true);
        faderCanvasGroup.alpha = 1.0f;
        faderCanvasGroup.blocksRaycasts = true;

        float waktuBerjalan = 0f;
        while (waktuBerjalan < durasiFade)
        {
            waktuBerjalan += Time.deltaTime;
            // Ubah alpha secara perlahan menggunakan Lerp agar transisi terasa mulus
            faderCanvasGroup.alpha = Mathf.Lerp(1.0f, 0.0f, waktuBerjalan / durasiFade);
            yield return null;
        }

        // Pastikan alpha benar-benar habis di angka nol
        faderCanvasGroup.alpha = 0.0f;
        // Matikan blocksRaycasts agar pemain bisa mengklik tombol/objek di bawahnya kembali
        faderCanvasGroup.blocksRaycasts = false;
        // Nonaktifkan objek fader agar tidak memakan performa render Unity
        faderCanvasGroup.gameObject.SetActive(false);
    }

    // Coroutine untuk menggelapkan layar dari transparan (0) ke hitam (1)
    // Bisa dipanggil oleh script lain sebelum melakukan perpindahan scene
    public IEnumerator FadeKeluar()
    {
        faderCanvasGroup.gameObject.SetActive(true);
        faderCanvasGroup.alpha = 0.0f;
        faderCanvasGroup.blocksRaycasts = true;

        float waktuBerjalan = 0f;
        while (waktuBerjalan < durasiFade)
        {
            waktuBerjalan += Time.deltaTime;
            faderCanvasGroup.alpha = Mathf.Lerp(0.0f, 1.0f, waktuBerjalan / durasiFade);
            yield return null;
        }

        faderCanvasGroup.alpha = 1.0f;
    }
}
