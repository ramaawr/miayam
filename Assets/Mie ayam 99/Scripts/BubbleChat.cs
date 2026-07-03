using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

// =============================================================================
// BubbleChat.cs — UI Bubble Chat untuk Menampilkan Pesanan NPC
// =============================================================================
// Script ini ditempel ke Panel UI yang menjadi bubble chat.
// Setiap NPC punya BubbleChat-nya sendiri (1 NPC = 1 BubbleChat).
//
// Desain Visual (Minimal Luxury — Putih + Aksen Biru):
//   - Panel background putih bersih (#FFFFFF)
//   - Teks label hitam, angka jumlah biru (#4A90D9)
//   - Corner rounded (via Image component di Unity)
//
// Setup di Unity:
//   1. Buat Canvas (World Space) → atur posisi di atas NPC
//   2. Buat Panel (Image putih, rounded) sebagai PanelBubble
//   3. Tambahkan 4 Text untuk mie/baso/ayam/sayur
//   4. Tambahkan 1 Text untuk feedback (terima kasih / pesanan salah)
//   5. Tempel script ini ke Panel, assign semua referensi
// =============================================================================

public class BubbleChat : MonoBehaviour
{
    // =========================================================================
    // REFERENSI UI — Assign dari Inspector
    // =========================================================================
    [Header("Panel Utama")]
    [Tooltip("Panel bubble chat (di-aktifkan/matikan untuk show/hide)")]
    public GameObject PanelBubble;

    [Header("Teks Pesanan — Assign tiap Text element")]
    [Tooltip("Text untuk menampilkan jumlah mie")]
    public TextMeshProUGUI TeksMie;

    [Tooltip("Text untuk menampilkan jumlah baso")]
    public TextMeshProUGUI TeksBaso;

    [Tooltip("Text untuk menampilkan jumlah ayam")]
    public TextMeshProUGUI TeksAyam;

    [Tooltip("Text untuk menampilkan jumlah sayur")]
    public TextMeshProUGUI TeksSayur;

    [Header("Teks Feedback — Untuk respon setelah penyerahan")]
    [Tooltip("Text untuk menampilkan 'Terima kasih!' atau 'Pesanan salah'")]
    public TextMeshProUGUI TeksFeedback;

    // =========================================================================
    // PENGATURAN TAMPILAN
    // =========================================================================
    [Header("Pengaturan")]
    [Tooltip("Berapa detik feedback ditampilkan sebelum otomatis hilang")]
    public float DurasiFeedback = 2.5f;

    // Referensi ke Coroutine yang sedang berjalan (untuk auto-hide feedback)
    private Coroutine coroutineFeedback;

    // =========================================================================
    // START — Sembunyikan bubble chat saat game mulai
    // =========================================================================
    void Start()
    {
    
    }

    // =========================================================================
    // TampilkanPesanan — Isi teks pesanan dan munculkan bubble
    // Dipanggil oleh NPCPelanggan saat pesanan baru dibuat atau diingatkan
    //
    // Parameter:
    //   pesanan → DataPesanan yang berisi jumlah masing-masing bahan
    // =========================================================================
    public void TampilkanPesanan(DataPesanan pesanan)
    {
        // Hentikan coroutine feedback kalau masih jalan
        // (supaya bubble tidak tiba-tiba hilang saat baru ditampilin)
        if (coroutineFeedback != null)
        {
            StopCoroutine(coroutineFeedback);
            coroutineFeedback = null;
            if (TeksMie != null) TeksMie.gameObject.SetActive(true);
            if (TeksBaso != null) TeksBaso.gameObject.SetActive(true);
            if (TeksAyam != null) TeksAyam.gameObject.SetActive(true);
            if (TeksSayur != null) TeksSayur.gameObject.SetActive(true);
        }

        // Isi teks masing-masing bahan
        // Format: "Mie: 1", "Baso: 2", dst
        if (TeksMie != null)
        {
            TeksMie.text = "Mie: " + pesanan.JumlahMie;

            // Kalau jumlahnya 0, samarkan teksnya (abu-abu)
            // Kalau ada isinya, pakai warna biru aksen (#4A90D9)
            TeksMie.color = pesanan.JumlahMie > 0
                ? new Color(0.29f, 0.56f, 0.85f) // Biru aksen
                : new Color(0.7f, 0.7f, 0.7f);    // Abu-abu
        }

        if (TeksBaso != null)
        {
            TeksBaso.text = "Baso: " + pesanan.JumlahBaso;
            TeksBaso.color = pesanan.JumlahBaso > 0
                ? new Color(0.29f, 0.56f, 0.85f)
                : new Color(0.7f, 0.7f, 0.7f);
        }

        if (TeksAyam != null)
        {
            TeksAyam.text = "Ayam: " + pesanan.JumlahAyam;
            TeksAyam.color = pesanan.JumlahAyam > 0
                ? new Color(0.29f, 0.56f, 0.85f)
                : new Color(0.7f, 0.7f, 0.7f);
        }

        if (TeksSayur != null)
        {
            TeksSayur.text = "Sayur: " + pesanan.JumlahSayur;
            TeksSayur.color = pesanan.JumlahSayur > 0
                ? new Color(0.29f, 0.56f, 0.85f)
                : new Color(0.7f, 0.7f, 0.7f);
        }

        // Sembunyikan teks feedback (kalau ada sisa dari sebelumnya)
        if (TeksFeedback != null)
        {
            TeksFeedback.gameObject.SetActive(false);
        }

        // PENTING: Aktifkan Canvas root (gameObject ini) TERLEBIH DAHULU
        // Kalau Canvas root mati, PanelBubble tidak akan terlihat walaupun aktif.
        // Ini akar masalah kenapa bubble chat tidak muncul sebelumnya!
        gameObject.SetActive(true);

        // Aktifkan panel bubble (child dari Canvas)
        if (PanelBubble != null)
        {
            PanelBubble.SetActive(true);
        }

        if (TryGetComponent<JuiceAnimator>(out var juice)) juice.PlayBubblePopUp();

        Debug.Log("BubbleChat ditampilkan: " + pesanan.TampilkanSebagaiTeks());
    }

    // =========================================================================
    // TampilkanFeedback — Tampilkan pesan respon setelah penyerahan
    // Otomatis hilang setelah DurasiFeedback detik
    //
    // Parameter:
    //   pesan → teks yang ditampilkan (misal: "Terima kasih!")
    // =========================================================================
    public void TampilkanFeedback(string pesan)
    {
        // Sembunyikan teks pesanan, tampilkan teks feedback
        if (TeksMie != null) TeksMie.gameObject.SetActive(false);
        if (TeksBaso != null) TeksBaso.gameObject.SetActive(false);
        if (TeksAyam != null) TeksAyam.gameObject.SetActive(false);
        if (TeksSayur != null) TeksSayur.gameObject.SetActive(false);

        if (TeksFeedback != null)
        {
            TeksFeedback.text = pesan;
            TeksFeedback.gameObject.SetActive(true);
        }

        // Aktifkan Canvas root dulu, baru PanelBubble
        gameObject.SetActive(true);

        if (PanelBubble != null)
        {
            PanelBubble.SetActive(true);
        }

        if (TryGetComponent<JuiceAnimator>(out var juice)) juice.PlayBubblePopUp();

        // Mulai timer untuk auto-hide feedback
        // Kalau ada timer lama yang masih jalan, hentikan dulu
        if (coroutineFeedback != null)
        {
            StopCoroutine(coroutineFeedback);
        }
        coroutineFeedback = StartCoroutine(AutoSembunyikanFeedback());

        Debug.Log("BubbleChat feedback: " + pesan);
    }

    // =========================================================================
    // SembunyikanBubble — Matikan panel bubble chat
    // =========================================================================
    public void SembunyikanBubble()
    {
        // Pastikan semua teks aktif kembali untuk tampilan berikutnya
        // (dilakukan SEBELUM mematikan Canvas, supaya state-nya siap)
        if (TeksMie != null) TeksMie.gameObject.SetActive(true);
        if (TeksBaso != null) TeksBaso.gameObject.SetActive(true);
        if (TeksAyam != null) TeksAyam.gameObject.SetActive(true);
        if (TeksSayur != null) TeksSayur.gameObject.SetActive(true);

        if (TeksFeedback != null)
        {
            TeksFeedback.gameObject.SetActive(false);
        }

        // Matikan Canvas root (gameObject ini) supaya bubble benar-benar hilang
        // Ini lebih efektif daripada cuma matikan PanelBubble,
        // karena Canvas root yang mati = tidak ada render sama sekali
        gameObject.SetActive(false);
    }

    // =========================================================================
    // AutoSembunyikanFeedback — Coroutine untuk menyembunyikan feedback
    // otomatis setelah beberapa detik
    //
    // Coroutine = fungsi yang bisa "pause" di tengah jalan.
    // yield return new WaitForSeconds(x) = tunggu x detik, lalu lanjut.
    // =========================================================================
    private IEnumerator AutoSembunyikanFeedback()
    {
        // Tunggu selama DurasiFeedback detik
        yield return new WaitForSeconds(DurasiFeedback);

        // Setelah waktu habis, sembunyikan bubble
        SembunyikanBubble();

        // Bersihkan referensi coroutine
        coroutineFeedback = null;
    }
}
