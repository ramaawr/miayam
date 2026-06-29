using UnityEngine;

// =============================================================================
// Clearbutton.cs — Tombol untuk membersihkan isi mangkok di dapur
// =============================================================================
// Script ini ditempelkan ke objek tombol reset/clear di area dapur.
// Saat diklik, script ini akan:
//   1. Mereset counter bahan di mangkok dapur menjadi 0.
//   2. Menghancurkan semua objek topping visual (bakso & sayur) yang ada di mangkok.
//   3. Menyembunyikan visual mie dan ayam di mangkok.
//   4. Mereset status boolean internal mangkok.
//
// PENTING: Script ini SEKARANG juga membersihkan data bawaan pemain di GameManager.
// Jadi, kalau pemain kepencet "Antar Pesanan" lalu mau membatalkannya,
// menekan tombol clear ini di dapur akan menghapus makanan yang sedang dibawa.
// =============================================================================

[RequireComponent(typeof(Collider2D))]
public class Clearbutton : MonoBehaviour
{
    [Header("Referensi Objek")]
    [Tooltip("Drag mangkok dapur yang ingin dibersihkan ke sini")]
    public Mangkok mangkokDapur;

    // =========================================================================
    // OnMouseDown — Dipanggil otomatis oleh Unity saat objek tombol ini diklik
    // (Membutuhkan komponen Collider2D pada objek ini)
    // =========================================================================
    private void OnMouseDown()
    {
        BersihkanMangkokDapur();
    }

    // =========================================================================
    // BersihkanMangkokDapur — Logika utama untuk mengosongkan mangkok dapur
    // dibuat public agar bisa juga dipanggil lewat event UI Button (OnClick) jika dibutuhkan
    // =========================================================================
    public void BersihkanMangkokDapur()
    {
        // Guard: Pastikan referensi mangkok tidak kosong
        if (mangkokDapur == null)
        {
            Debug.LogWarning("Clearbutton: Referensi mangkok dapur belum di-assign di Inspector!");
            return;
        }

        // 1. Kosongkan mangkok dapur (visual, data counter, & child topping) secara total
        // Menggunakan fungsi terpusat di script Mangkok agar tidak terjadi duplikasi kode
        mangkokDapur.KosongkanMangkokDapur();

        // 2. Hancurkan item apa pun yang sedang dipegang/di-drag di kursor pemain (mie/baso/ayam/sayur)
        // Kita cari semua script bidcontrol yang ada di scene, lalu jika ada item yang dipegang kursor,
        // kita hancurkan GameObject tersebut dan set variabelnya menjadi null.
        bidcontrol[] semuaTangan = FindObjectsOfType<bidcontrol>();
        foreach (bidcontrol tangan in semuaTangan)
        {
            if (tangan.dipegang != null)
            {
                Destroy(tangan.dipegang);
                tangan.dipegang = null;
            }
        }

        // 3. Reset bawaan pemain di GameManager jika pemain sedang memegang makanan dari dapur
        if (GameManager.instance != null && GameManager.instance.BawaMakanan == true)
        {
            GameManager.instance.BersihkanBawaan();
        }

        Debug.Log("Clearbutton: Mangkok dapur bersih total & item di tangan dihancurkan!");
    }
}
