using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Profil NPC Baru", menuName = "Mie Ayam/Profil NPC")]
public class ProfilNPC : ScriptableObject
{
    #region IDENTITAS NPC
    [Header("Identitas NPC")]
    [Tooltip("Nama NPC (Opsional, untuk memudahkan developer melacak data)")]
    // Menyimpan nama NPC untuk keperluan pelacakan dan identifikasi di editor Unity
    public string NamaNPC = "NPC Tanpa Nama";

    [Tooltip("Wujud gambar (Sprite) NPC ini saat muncul di gerobak")]
    // Mengatur gambar wujud 2D dari NPC yang akan dimunculkan saat pelanggan datang
    public Sprite VisualNPC;
    #endregion

    #region PENGATURAN PESANAN
    [Header("Pengaturan Pesanan")]
    [Tooltip("Manual = pesanan fix. Random = pesanan diacak.")]
    // Menentukan apakah NPC ini menggunakan pesanan yang sudah ditetapkan secara manual atau diacak otomatis
    public NPCPelanggan.ModePesananType ModePesanan = NPCPelanggan.ModePesananType.Random;

    [Tooltip("Isi pesanan jika memilih mode Manual")]
    // Menyimpan data detail pesanan spesifik jika mode Manual dipilih
    public DataPesanan PesananManual = new DataPesanan();
    #endregion

    #region BATAS PESANAN ACAK
    [Header("Batas Random (Jika mode Random)")]
    // Menentukan batas minimum dan maksimum untuk jumlah baso yang akan dipesan pelanggan
    public int MinBaso = 0;
    public int MaxBaso = 3;
    // Menentukan batas minimum dan maksimum untuk porsi ayam yang akan dipesan
    public int MinAyam = 1;
    public int MaxAyam = 1;
    // Menentukan batas minimum dan maksimum untuk porsi sayur yang akan dipesan
    public int MinSayur = 0;
    public int MaxSayur = 3;
    #endregion
    
    #region PENGATURAN DIALOG NPC
    [Header("Pengaturan Dialog Khusus NPC")]
    [Tooltip("Dialog yang muncul otomatis saat NPC ini baru tiba di depan gerobak, SEBELUM pesanan dibuat.")]
    // Obrolan perkenalan atau basa-basi dari pelanggan
    public List<BlokDialog> DialogSebelumPesan = new List<BlokDialog>();

    [Tooltip("Dialog yang muncul setelah pemain menyerahkan pesanan, SEBELUM NPC pulang.")]
    // Obrolan penutup, ucapan terima kasih, atau komplain setelah menerima makanan
    public List<BlokDialog> DialogSetelahPesan = new List<BlokDialog>();
    #endregion

    #region DATA REPUTASI
    [Header("Data Reputasi (Untuk Pengembangan Masa Depan)")]
    [Tooltip("Jumlah kesalahan yang pernah dilakukan pemain ke NPC ini.")]
    // Menyimpan rekam jejak jumlah kesalahan pemain terhadap NPC ini, berguna untuk fitur penalti/reputasi nantinya
    public int JumlahKesalahanPemain = 0;
    #endregion
}
