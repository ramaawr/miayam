using UnityEngine;
using TMPro;

public class EconomyManager : MonoBehaviour
{
    #region SINGLETON
    // Akses cepat dari script lain (seperti GameManager / NPCPelanggan) 
    // Contoh pemakaian: EconomyManager.instance.TotalUangKeseluruhan
    public static EconomyManager instance;
    #endregion

    #region DATA DOMPET PEMAIN
    [Header("Data Uang (Dompet)")]
    [Tooltip("Uang total pemain yang tidak akan kereset saat ganti level")]
    // Menyimpan saldo total uang pemain yang bersifat permanen antar level
    public int TotalUangKeseluruhan = 0;

    [Tooltip("Uang yang HANYA didapatkan di level ini (akan direset di akhir level)")]
    // Menyimpan pendapatan sementara khusus untuk sesi level hari ini saja
    public int PendapatanHariIni = 0;
    #endregion

    #region HARGA BAHAN DASAR
    [Header("Harga Bahan Dasar")]
    // Menentukan harga satuan dari setiap bahan yang berhasil dimasak dan diantar
    public int HargaMie = 5000;
    public int HargaBaso = 2000;
    public int HargaAyam = 3000;
    public int HargaSayur = 1000;
    #endregion

    #region BONUS DAN DENDA
    [Header("Bonus & Denda")]
    [Tooltip("Tip / uang tambahan jika pesanan 100% sempurna")]
    // Bonus tambahan (tip) yang diberikan jika pemain mengantar pesanan dengan akurasi 100%
    public int BonusCocok = 2000;
    
    [Tooltip("Potongan uang (denda) jika pesanan salah (kurang/kelebihan bahan)")]
    // Penalti pengurangan pendapatan apabila pemain melakukan kesalahan komposisi bahan
    public int DendaSalah = 3000;
    #endregion

    #region REFERENSI ANTARMUKA (UI)
    [Header("Referensi UI HUD")]
    [Tooltip("Teks UI di pojok layar untuk menampilkan uang secara real-time")]
    // Referensi ke komponen UI Teks untuk menampilkan total saldo uang saat bermain
    public TextMeshProUGUI TeksHUDTotalUang;
    #endregion

    #region INISIALISASI AWAL
    void Awake()
    {
        // Pastikan hanya ada 1 EconomyManager di dalam scene
        if (instance == null) 
        {
            instance = this;
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Memperbarui UI di awal permainan agar teks tidak kosong/menggunakan tulisan default
        UpdateHUDUang();
    }
    #endregion

    #region LOGIKA PENDAPATAN
    // Dijalankan oleh NPCPelanggan.cs saat pemain menyerahkan mangkok ke pelanggan
    public void HitungPendapatan(DataPesanan bawaan, bool pesananBenar)
    {
        // Wadah sementara untuk menghitung total pendapatan di satu kali transaksi
        int pendapatanTransaksi = 0;

        // --- 1. Kalkulasi Modal / Harga Dasar ---
        // Jika ada mie, tambahkan harga mie ke total
        if (bawaan.JumlahMie > 0) 
        {
            pendapatanTransaksi = pendapatanTransaksi + HargaMie;
        }
        // Menambahkan harga dari jumlah topping baso, ayam, dan sayur
        pendapatanTransaksi = pendapatanTransaksi + (bawaan.JumlahBaso * HargaBaso);
        pendapatanTransaksi = pendapatanTransaksi + (bawaan.JumlahAyam * HargaAyam);
        pendapatanTransaksi = pendapatanTransaksi + (bawaan.JumlahSayur * HargaSayur);

        // --- 2. Tentukan Bonus atau Denda ---
        if (pesananBenar == true)
        {
            // Pesanan sempurna! Tambahkan uang bonus tip ke total
            pendapatanTransaksi = pendapatanTransaksi + BonusCocok;
            Debug.Log("Transaksi Sempurna! Pemain dapat harga bahan + bonus. Total: Rp " + pendapatanTransaksi);
        }
        else
        {
            // Pesanan keliru! Kurangi total dengan denda
            pendapatanTransaksi = pendapatanTransaksi - DendaSalah;
            
            // Mencegah nilai uang menjadi minus dari 1 pelanggan, minimal dapat Rp 0
            if (pendapatanTransaksi < 0) 
            {
                pendapatanTransaksi = 0;
            }
            Debug.Log("Transaksi Salah! Kena denda potongan. Total didapat: Rp " + pendapatanTransaksi);
        }

        // --- 3. Masukkan ke Dompet ---
        // Memasukkan hasil hitungan uang ke catatan pendapatan hari ini dan total uang permanen
        PendapatanHariIni = PendapatanHariIni + pendapatanTransaksi;
        TotalUangKeseluruhan = TotalUangKeseluruhan + pendapatanTransaksi;

        // --- 4. Perbarui UI Layar ---
        UpdateHUDUang();
    }

    // Fungsi internal (private) khusus untuk memperbarui teks UI saldo uang di pojok layar
    private void UpdateHUDUang()
    {
        if (TeksHUDTotalUang != null)
        {
            // Mengubah teks dengan format 'Rp [jumlah]'
            TeksHUDTotalUang.text = "Rp " + TotalUangKeseluruhan;
        }
    }

    // Dijalankan di akhir level untuk me-reset catatan uang khusus hari itu saja
    public void ResetPendapatanHarian()
    {
        PendapatanHariIni = 0;
        Debug.Log("Pendapatan Harian telah direset menjadi 0.");
    }
    #endregion
}
