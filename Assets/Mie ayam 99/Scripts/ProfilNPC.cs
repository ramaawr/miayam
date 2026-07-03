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

    #region DATA DIALOG NPC
    [Header("Dialog Cerita NPC")]
    [Tooltip("Percakapan sebelum NPC memesan makanan (opsional)")]
    // Menyimpan dialog sebelum pesanan (order bubble) muncul
    public DialogueSequence DialogSebelumOrder;

    [Tooltip("Percakapan setelah pemain menyerahkan pesanan yang benar (opsional)")]
    // Menyimpan dialog penutup sebelum NPC pergi membawa makanan yang benar
    public DialogueSequence DialogSetelahOrder;
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
    

}
