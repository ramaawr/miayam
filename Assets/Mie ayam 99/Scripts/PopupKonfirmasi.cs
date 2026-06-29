using UnityEngine;
using TMPro; // Kita pakai TextMeshPro untuk menampilkan daftar masakan agar teks terlihat tajam dan bagus

// =============================================================================
// PopupKonfirmasi.cs — Pengontrol Tampilan Dialog Konfirmasi Kirim Pesanan
// =============================================================================
// Script ini diletakkan pada GameObject Panel Popup di Canvas.
// Tugasnya:
//   1. Menampilkan/menyembunyikan panel popup saat dipicu.
//   2. Mengisi teks daftar topping mie ayam yang saat ini sedang dibuat.
//   3. Menyediakan fungsi tombol YES dan NO untuk diklik pemain.
// =============================================================================
public class PopupKonfirmasi : MonoBehaviour
{
    [Header("Referensi Komponen UI")]
    [Tooltip("Drag GameObject Panel Utama dari Popup ini ke sini (yang mau di-aktifkan/nonaktifkan)")]
    public GameObject PanelPopupUtama;

    [Tooltip("Drag komponen TextMeshProUGUI untuk bagian deskripsi makanan ke sini")]
    public TextMeshProUGUI TeksDeskripsiMasakan;

    [Tooltip("Drag GameObject Tombol NO ke sini (akan disembunyikan jika mode peringatan)")]
    public GameObject TombolNo;

    [Tooltip("Drag komponen TextMeshProUGUI milik Tombol YES ke sini (untuk ganti teks YES/OK)")]
    public TextMeshProUGUI TeksTombolYes;

    // Menyimpan referensi tombol yang membuka popup ini
    // agar saat tombol YES ditekan, kita tahu script mana yang harus melanjutkan proses antar
    private TombolAntarPesanan tombolPemicu;

    private void Start()
    {
        // Saat game baru mulai, pastikan panel popup dalam keadaan tertutup/nonaktif
        if (PanelPopupUtama != null)
        {
            PanelPopupUtama.SetActive(false);
        }
    }

    // =========================================================================
    // Tampilkan — Fungsi untuk membuka popup konfirmasi 2 tombol (YES & NO)
    // =========================================================================
    // Parameter:
    //   pemicu    → script TombolAntarPesanan yang memanggil popup ini
    //   deskripsi → teks berisi rincian isi mangkok saat ini
    // =========================================================================
    public void Tampilkan(TombolAntarPesanan pemicu, string deskripsi)
    {
        // Simpan siapa yang memicu popup ini
        tombolPemicu = pemicu;

        // Update teks deskripsi dengan rincian makanan terbaru
        if (TeksDeskripsiMasakan != null)
        {
            TeksDeskripsiMasakan.text = deskripsi;
        }

        // Mode Konfirmasi: Pastikan tombol NO aktif
        if (TombolNo != null)
        {
            TombolNo.SetActive(true);
        }

        // Mode Konfirmasi: Teks tombol YES tetap "YES"
        if (TeksTombolYes != null)
        {
            TeksTombolYes.text = "YES";
        }

        // Tampilkan panel popup ke layar
        if (PanelPopupUtama != null)
        {
            PanelPopupUtama.SetActive(true);
        }
        
        Debug.Log("Popup Konfirmasi Terbuka!");
    }

    // =========================================================================
    // TampilkanPeringatan — Fungsi untuk membuka popup peringatan 1 tombol (OK)
    // =========================================================================
    // Parameter:
    //   pesan → teks pesan kesalahan / petunjuk untuk pemain
    // =========================================================================
    public void TampilkanPeringatan(string pesan)
    {
        // Peringatan tidak butuh pemicu balik karena hanya butuh tombol OK untuk menutup
        tombolPemicu = null;

        // Update teks dengan pesan kesalahan
        if (TeksDeskripsiMasakan != null)
        {
            TeksDeskripsiMasakan.text = pesan;
        }

        // Mode Peringatan: Sembunyikan tombol NO
        if (TombolNo != null)
        {
            TombolNo.SetActive(false);
        }

        // Mode Peringatan: Ubah teks tombol YES menjadi "OK"
        if (TeksTombolYes != null)
        {
            TeksTombolYes.text = "OK";
        }

        // Tampilkan panel popup ke layar
        if (PanelPopupUtama != null)
        {
            PanelPopupUtama.SetActive(true);
        }

        Debug.Log("Popup Peringatan Terbuka: " + pesan);
    }

    // =========================================================================
    // TombolYesDitekan — Dipanggil saat pemain mengklik tombol YES di UI
    // =========================================================================
    // Hubungkan fungsi ini ke event OnClick() tombol YES di Unity Inspector
    // =========================================================================
    public void TombolYesDitekan()
    {
        Debug.Log("Pemain memilih YES: Pesanan dikonfirmasi!");

        // 1. Simpan referensi pemicu ke variabel lokal agar tidak hilang saat di-reset
        TombolAntarPesanan pemicuLokal = tombolPemicu;

        // 2. Tutup panel popup terlebih dahulu
        TutupPopup();

        // 3. Jalankan fungsi pengantaran pesanan yang sesungguhnya di script TombolAntarPesanan
        if (pemicuLokal != null)
        {
            pemicuLokal.KonfirmasiAntarPesanan();
        }
    }

    // =========================================================================
    // TombolNoDitekan — Dipanggil saat pemain mengklik tombol NO di UI
    // =========================================================================
    // Hubungkan fungsi ini ke event OnClick() tombol NO di Unity Inspector
    // =========================================================================
    public void TombolNoDitekan()
    {
        Debug.Log("Pemain memilih NO: Pengiriman dibatalkan.");

        // Cukup tutup popup, makanan tetap ada di mangkok dapur agar bisa diedit kembali
        TutupPopup();
    }

    // =========================================================================
    // TutupPopup — Fungsi internal untuk menyembunyikan panel popup
    // =========================================================================
    public void TutupPopup()
    {
        if (PanelPopupUtama != null)
        {
            PanelPopupUtama.SetActive(false);
        }

        // Bersihkan referensi agar tidak terjadi salah panggil di kemudian hari
        tombolPemicu = null;
    }
}
