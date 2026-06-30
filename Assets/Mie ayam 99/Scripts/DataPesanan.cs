using UnityEngine;

[System.Serializable] // Agar class ini bisa muncul dan diedit di Inspector Unity
public class DataPesanan
{
    #region DATA BAHAN
    // Variabel-variabel ini menyimpan jumlah masing-masing bahan pesanan
    // Dibuat public supaya bisa dilihat dan diubah dari Inspector Unity
    // Jika ada bahan baru (misal: pangsit), tambahkan variabel di bawah sini
    public int JumlahMie = 0;
    public int JumlahBaso = 0;
    public int JumlahAyam = 0;
    public int JumlahSayur = 0;
    #endregion

    #region FUNGSI UTAMA
    // Mengembalikan semua nilai bahan ke 0 saat transaksi selesai atau mulai pesanan baru
    public void ResetSemua()
    {
        JumlahMie = 0;
        JumlahBaso = 0;
        JumlahAyam = 0;
        JumlahSayur = 0;
    }

    // Mengecek apakah isi bawaan pemain cocok dengan data pesanan (membandingkan semua bahan)
    public bool SamaDengan(DataPesanan lainnya)
    {
        // Kalau datanya null (kosong), berarti tidak ada data untuk dicocokkan
        if (lainnya == null)
        {
            return false;
        }

        // Membandingkan bahan satu per satu untuk memastikan semuanya sama
        bool mieCocok = (JumlahMie == lainnya.JumlahMie);
        bool basoCocok = (JumlahBaso == lainnya.JumlahBaso);
        bool ayamCocok = (JumlahAyam == lainnya.JumlahAyam);
        bool sayurCocok = (JumlahSayur == lainnya.JumlahSayur);

        // Hanya akan me-return true (cocok) jika semua bahan sesuai
        return mieCocok && basoCocok && ayamCocok && sayurCocok;
    }

    // Menyalin isi dari data pesanan lain ke diri sendiri tanpa mengubah referensi asli
    public void SalinDari(DataPesanan sumber)
    {
        // Mencegah error NullReferenceException jika data di Inspector Unity belum ter-serialize
        if (sumber == null)
        {
            Debug.LogWarning("DataPesanan: Sumber data yang akan disalin null! Menggunakan nilai default kosong.");
            ResetSemua();
            return;
        }

        JumlahMie = sumber.JumlahMie;
        JumlahBaso = sumber.JumlahBaso;
        JumlahAyam = sumber.JumlahAyam;
        JumlahSayur = sumber.JumlahSayur;
    }

    // Mengubah isi pesanan menjadi format teks biasa (biasanya untuk keperluan Debug.Log)
    public string TampilkanSebagaiTeks()
    {
        return "Mie:" + JumlahMie +
               " Baso:" + JumlahBaso +
               " Ayam:" + JumlahAyam +
               " Sayur:" + JumlahSayur;
    }
    #endregion
}
