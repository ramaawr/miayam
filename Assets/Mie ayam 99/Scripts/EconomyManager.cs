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

    #region BIAYA HARIAN
    [Header("Biaya Harian (Konstan)")]
    [Tooltip("Biaya tetap yang dipotong setiap akhir hari untuk modal bahan")]
    public int BiayaBahanHarian = 5000;
    
    [Tooltip("Biaya tetap yang dipotong setiap akhir hari untuk pungli/keamanan")]
    public int BiayaPungliHarian = 2000;
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

    #region PENCATATAN HARIAN (STRUK)
    // Variabel ini untuk mencatat berapa banyak bahan yang sudah diberikan ke pelanggan hari ini
    [HideInInspector] public int TotalMieTerjual = 0;
    [HideInInspector] public int TotalBasoTerjual = 0;
    [HideInInspector] public int TotalAyamTerjual = 0;
    [HideInInspector] public int TotalSayurTerjual = 0;
    [HideInInspector] public int TotalBonusHarian = 0;
    [HideInInspector] public int TotalDendaHarian = 0;
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

        // --- 1. Kalkulasi Modal / Harga Dasar & Catat Penjualan ---
        // Jika ada mie, tambahkan harga mie ke total
        if (bawaan.JumlahMie > 0) 
        {
            pendapatanTransaksi = pendapatanTransaksi + HargaMie;
            TotalMieTerjual = TotalMieTerjual + 1; // 1 porsi mangkok dihitung 1 mie
        }
        // Menambahkan harga dari jumlah topping baso, ayam, dan sayur
        pendapatanTransaksi = pendapatanTransaksi + (bawaan.JumlahBaso * HargaBaso);
        TotalBasoTerjual = TotalBasoTerjual + bawaan.JumlahBaso;

        pendapatanTransaksi = pendapatanTransaksi + (bawaan.JumlahAyam * HargaAyam);
        TotalAyamTerjual = TotalAyamTerjual + bawaan.JumlahAyam;

        pendapatanTransaksi = pendapatanTransaksi + (bawaan.JumlahSayur * HargaSayur);
        TotalSayurTerjual = TotalSayurTerjual + bawaan.JumlahSayur;

        // --- 2. Tentukan Bonus atau Denda ---
        if (pesananBenar == true)
        {
            // Pesanan sempurna! Tambahkan uang bonus tip ke total
            pendapatanTransaksi = pendapatanTransaksi + BonusCocok;
            TotalBonusHarian = TotalBonusHarian + BonusCocok;
            Debug.Log("Transaksi Sempurna! Pemain dapat harga bahan + bonus. Total: Rp " + pendapatanTransaksi);
        }
        else
        {
            // Pesanan keliru! Kurangi total dengan denda
            pendapatanTransaksi = pendapatanTransaksi - DendaSalah;
            TotalDendaHarian = TotalDendaHarian + DendaSalah;
            
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

    // Dijalankan di akhir level oleh LevelManager sebelum memunculkan popup
    public void ProsesPotonganAkhirHari()
    {
        // Memotong uang total keseluruhan dengan biaya harian tetap
        TotalUangKeseluruhan = TotalUangKeseluruhan - BiayaBahanHarian;
        TotalUangKeseluruhan = TotalUangKeseluruhan - BiayaPungliHarian;

        // Pastikan uang tidak minus (opsional, tapi disarankan agar saldo tidak aneh)
        if (TotalUangKeseluruhan < 0) 
        {
            TotalUangKeseluruhan = 0;
        }

        // Perbarui HUD agar sinkron dengan potongan yang baru terjadi
        UpdateHUDUang();
    }

    // Mengembalikan teks panjang yang berisi rincian jualan hari ini
    public string DapatkanTeksStruk()
    {
        int pendapatanBersih = PendapatanHariIni - BiayaBahanHarian - BiayaPungliHarian;

        return "RINCIAN PENJUALAN:\n" +
               "Mie (" + TotalMieTerjual + "x) : Rp " + (TotalMieTerjual * HargaMie) + "\n" +
               "Baso (" + TotalBasoTerjual + "x) : Rp " + (TotalBasoTerjual * HargaBaso) + "\n" +
               "Ayam (" + TotalAyamTerjual + "x) : Rp " + (TotalAyamTerjual * HargaAyam) + "\n" +
               "Sayur (" + TotalSayurTerjual + "x) : Rp " + (TotalSayurTerjual * HargaSayur) + "\n" +
               "Tip/Bonus : Rp " + TotalBonusHarian + "\n" +
               "Denda Salah : -Rp " + TotalDendaHarian + "\n\n" +
               "POTONGAN HARIAN:\n" +
               "Biaya Bahan : -Rp " + BiayaBahanHarian + "\n" +
               "Pungli : -Rp " + BiayaPungliHarian + "\n\n" +
               "Pendapatan Bersih Hari Ini: Rp " + pendapatanBersih;
    }

    // Dijalankan di akhir level untuk me-reset catatan uang khusus hari itu saja
    public void ResetPendapatanHarian()
    {
        PendapatanHariIni = 0;
        TotalMieTerjual = 0;
        TotalBasoTerjual = 0;
        TotalAyamTerjual = 0;
        TotalSayurTerjual = 0;
        TotalBonusHarian = 0;
        TotalDendaHarian = 0;
        Debug.Log("Pendapatan & Catatan Harian telah direset menjadi 0.");
    }
    #endregion
}
