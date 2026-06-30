using UnityEngine;

// =============================================================================
// TombolAntarPesanan.cs — Tombol "Antar Pesanan" di Area Dapur
// =============================================================================
// Script ini ditempel ke objek yang bisa diklik di area dapur.
// Bisa berupa:
//   A. SpriteRenderer + Collider2D (klik via OnMouseDown) ← SETUP SAAT INI
//   B. UI Button (klik via OnClick event di Inspector)
//
// Saat ditekan, script ini akan:
//   1. Membaca isi mangkok dapur (berapa mie, baso, ayam, sayur di dalamnya)
//   2. Menyimpan data itu ke GameManager sebagai "bawaan pemain"
//   3. Mengosongkan mangkok dapur (reset semua counter & hapus visual topping)
//   4. Otomatis memindahkan kamera ke Area Depan
//
// Setup di Unity:
//   1. Tempel script ini ke objek yang punya Collider2D (SpriteRenderer)
//   2. Assign referensi mangkokDapur dan navigasiKamera dari Inspector
//   3. Tombol otomatis bisa diklik berkat OnMouseDown()
// =============================================================================

[RequireComponent(typeof(Collider2D))]
public class TombolAntarPesanan : MonoBehaviour
{
    // =========================================================================
    // REFERENSI — Assign dari Inspector
    // =========================================================================
    [Header("Referensi Objek")]
    [Tooltip("Drag mangkok dapur ke sini")]
    public Mangkok mangkokDapur;

    [Tooltip("Drag Main Camera (yang punya NavigasiKamera) ke sini")]
    public NavigasiKamera navigasiKamera;

    [Tooltip("Drag objek Canvas yang memiliki script PopupKonfirmasi ke sini")]
    public PopupKonfirmasi popupKonfirmasi;

    // =========================================================================
    // OnMouseDown — Dipanggil otomatis oleh Unity saat objek ini diklik
    // Ini cara kerja yang sama dengan script lain di proyek ini
    // (bidcontrol, kompor, mangkok → semua pakai OnMouseDown + Collider2D)
    //
    // KENAPA BUTUH INI?
    // Objek tombol di scene pakai SpriteRenderer + Collider2D, bukan UI Button.
    // UI Button butuh Canvas + EventSystem dan pakai sistem OnClick().
    // SpriteRenderer butuh OnMouseDown() + Collider2D untuk deteksi klik.
    // =========================================================================
    private void OnMouseDown()
    {
        AntarPesanan();
    }

    // =========================================================================
    // AntarPesanan — Fungsi inisiasi untuk mulai mengantar pesanan
    // Fungsi ini akan memvalidasi masakan terlebih dahulu, lalu memunculkan
    // popup konfirmasi agar pemain bisa memeriksa ulang masakannya.
    // =========================================================================
    public void AntarPesanan()
    {
        // ----- GUARD: Pastikan GameManager ada -----
        if (GameManager.instance == null)
        {
            Debug.LogWarning("GameManager belum ada di scene!");
            return;
        }

        // ----- GUARD: Cek apakah ada pesanan aktif yang perlu diantar -----
        // Kalau tidak ada NPC yang pesan, tombol tidak ngapa-ngapain
        if (GameManager.instance.DaftarNPCAktif.Count == 0)
        {
            Debug.Log("Belum ada pesanan yang perlu diantar.");
            if (popupKonfirmasi != null)
            {
                popupKonfirmasi.TampilkanPeringatan("Belum ada pelanggan yang datang memesan! Silakan tunggu pelanggan terlebih dahulu.");
            }
            return;
        }

        // ----- GUARD: Cek apakah pemain sudah membawa makanan -----
        // Kalau sudah bawa (belum diserahkan ke NPC), jangan bisa antar lagi
        if (GameManager.instance.BawaMakanan == true)
        {
            Debug.Log("Kamu sudah membawa makanan! Serahkan dulu ke NPC.");
            if (popupKonfirmasi != null)
            {
                popupKonfirmasi.TampilkanPeringatan("Kamu sudah membawa makanan di tangan! Serahkan dulu pesanan ini ke pelanggan di gerobak depan.");
            }
            return;
        }

        // ----- GUARD: Cek apakah referensi mangkok sudah ada -----
        if (mangkokDapur == null)
        {
            Debug.LogWarning("Referensi mangkok belum di-assign di Inspector!");
            return;
        }

        // ----- GUARD: Cek apakah mangkok kosong (mencegah pesanan 0 0 0 0) -----
        // Kenapa ditambah? Supaya kalau pemain belum masukin mie sama sekali,
        // tombol antar ini nggak akan merespons. Jadi nggak akan ada lagi
        // bug mangkok 0 0 0 0 yang tiba-tiba kebawa.
        if (mangkokDapur.AdaMie == false)
        {
            Debug.Log("Mangkok masih kosong! Tidak bisa diantar.");
            if (popupKonfirmasi != null)
            {
                popupKonfirmasi.TampilkanPeringatan("Mangkok Anda masih kosong! Masukkan mie terlebih dahulu sebelum mengantar pesanan.");
            }
            return;
        }

        // =====================================================================
        // TAMPILKAN POPUP KONFIRMASI
        // Jika referensi popupKonfirmasi di-assign, kita munculkan popup.
        // Jika tidak di-assign (fallback), langsung kirim pesanan seperti biasa.
        // =====================================================================
        if (popupKonfirmasi != null)
        {
            // Buat string rincian isi mangkok secara manual agar ramah pemula dan rapi
            string detailMasakan = "Isi Mangkok Saat Ini:\n" +
                                   "• Mie : " + (mangkokDapur.AdaMie ? "include" : "none") + "\n" +
                                   "• Ayam : " + mangkokDapur.TotalAyam + "\n" +
                                   "• Bakso : " + mangkokDapur.Totalbaso + "\n" +
                                   "• Sayur : " + mangkokDapur.TotalSayur + "";


            popupKonfirmasi.Tampilkan(this, detailMasakan);
        }
        else
        {
            Debug.LogWarning("PopupKonfirmasi belum di-assign di Inspector! Langsung mengirim tanpa konfirmasi.");
            KonfirmasiAntarPesanan();
        }
    }

    // =========================================================================
    // KonfirmasiAntarPesanan — Fungsi eksekusi utama setelah pemain memilih YES
    // Fungsi ini dipanggil dari PopupKonfirmasi.cs ketika pemain menyetujui pesanan.
    // =========================================================================
    public void KonfirmasiAntarPesanan()
    {
        // =====================================================================
        // LANGKAH 1: Baca isi mangkok dapur
        // Ambil semua counter dari script Mangkok yang sudah ada
        // =====================================================================
        bool adaMie = mangkokDapur.AdaMie;             // Apakah ada mie di mangkok?
        int jumlahBaso = mangkokDapur.Totalbaso;       // Berapa baso yang ditaruh?
        int jumlahAyam = mangkokDapur.TotalAyam;       // Berapa ayam yang ditaruh?
        int jumlahSayur = mangkokDapur.TotalSayur;     // Berapa sayur yang ditaruh?

        Debug.Log("Membaca mangkok — Mie:" + (adaMie ? 1 : 0)
                 + " Baso:" + jumlahBaso
                 + " Ayam:" + jumlahAyam
                 + " Sayur:" + jumlahSayur);

        // =====================================================================
        // LANGKAH 2: Simpan data ke GameManager sebagai bawaan pemain
        // Fungsi ini juga otomatis set BawaMakanan = true
        // =====================================================================
        GameManager.instance.SimpanBawaanDariDapur(adaMie, jumlahBaso, jumlahAyam, jumlahSayur);

        // =====================================================================
        // LANGKAH 3: Kosongkan mangkok dapur
        // Hapus semua topping visual dan reset counter
        // =====================================================================
        KosongkanMangkok();

        // =====================================================================
        // LANGKAH 4: Pindahkan kamera ke Area Depan
        // Pemain otomatis dibawa ke area pelanggan untuk menyerahkan pesanan
        // =====================================================================
        if (navigasiKamera != null)
        {
            navigasiKamera.PindahKeDepan();
        }
        else
        {
            Debug.LogWarning("NavigasiKamera belum di-assign! Kamera tidak bisa pindah.");
        }

        Debug.Log("Pesanan diantar! Menuju Area Depan...");
    }

    // =========================================================================
    // KosongkanMangkok — Reset mangkok dapur ke kondisi kosong
    // =========================================================================
    private void KosongkanMangkok()
    {
        // Memanggil fungsi pengosongan terpusat di script mangkokDapur
        // untuk menghindari duplikasi kode yang rawan bug.
        mangkokDapur.KosongkanMangkokDapur();
        Debug.Log("Mangkok dapur sudah dikosongkan!");
    }
}
