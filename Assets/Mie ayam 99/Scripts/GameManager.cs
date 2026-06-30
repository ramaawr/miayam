using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    #region SINGLETON
    // Jalur cepat akses dari script lain agar bisa diakses global (contoh: GameManager.instance.BawaMakanan)
    // Cuma boleh ada 1 GameManager di scene
    public static GameManager instance;
    #endregion

    #region STATUS AREA
    [Header("Status Posisi Pemain")]
    // Menyimpan posisi pemain saat ini: true = di dapur (area masak), false = di depan (area pelanggan)
    // Dipakai oleh NPCPelanggan untuk mengecek apakah pemain bisa diajak interaksi
    public bool SedangDiDapur = false;
    #endregion

    #region BAWAAN PEMAIN
    [Header("Makanan Yang Dibawa Dari Dapur")]
    // Penanda apakah pemain saat ini sedang membawa makanan dari dapur ke area depan
    public bool BawaMakanan = false; 
    
    // Wadah data yang menyimpan detail isi mangkok (jumlah mie, baso, dll) yang sedang dibawa pemain
    public DataPesanan BawaanPemain = new DataPesanan(); 
    #endregion

    #region DAFTAR NPC AKTIF
    [Header("Daftar NPC Aktif")]
    // Menyimpan daftar NPC mana saja yang sedang memesan di depan gerobak
    // Berguna untuk melacak antrian atau interaksi lebih lanjut
    public List<NPCPelanggan> DaftarNPCAktif = new List<NPCPelanggan>();
    #endregion

    #region RIWAYAT TRANSAKSI
    [Header("Riwayat Transaksi")]
    // Menghitung total seluruh pesanan yang sudah diantar (baik benar maupun salah)
    public int TotalTransaksi = 0;    
    
    // Menghitung jumlah pesanan yang diantar dengan isi mangkok yang sesuai (benar)
    public int TransaksiBenar = 0;    
    
    // Menghitung jumlah pesanan yang diantar namun isi mangkoknya keliru (salah)
    public int TransaksiSalah = 0;    
    #endregion

    #region FUNGSI AWAL MULA (AWAKE)
    void Awake()
    {
        // Memastikan wadah bawaan pemain tidak null untuk menghindari error sistem Unity
        if (BawaanPemain == null)
        {
            BawaanPemain = new DataPesanan();
        }

        // Mengecek apakah instance Singleton sudah ada
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            // Jika ada GameManager duplikat (lebih dari satu), hancurkan yang baru agar sistem tidak bingung
            Debug.LogWarning("Ada duplikat GameManager! Yang duplikat dihancurkan.");
            Destroy(gameObject);
        }
    }
    #endregion

    #region FUNGSI DAPUR & PESANAN
    // Dipanggil oleh TombolAntarPesanan untuk memindahkan isi mangkok ke memori sistem (tangan pemain)
    public void SimpanBawaanDariDapur(bool adaMie, int baso, int ayam, int sayur)
    {
        // Mengubah status bool mie menjadi angka 1 (ada) atau 0 (tidak ada)
        BawaanPemain.JumlahMie = adaMie ? 1 : 0; 
        
        // Menyimpan jumlah masing-masing topping ke dalam memori bawaan pemain
        BawaanPemain.JumlahBaso = baso;
        BawaanPemain.JumlahAyam = ayam;
        BawaanPemain.JumlahSayur = sayur;

        // Mengubah status sistem agar mengenali bahwa pemain sekarang sedang memegang makanan
        BawaMakanan = true;

        Debug.Log("Pemain bawa makanan dari dapur: " + BawaanPemain.TampilkanSebagaiTeks());
    }

    // Dipanggil oleh NPCPelanggan setelah mengecek pesanan untuk mencatat hasilnya ke skor
    public void CatatTransaksi(bool pesananBenar)
    {
        // Menambah hitungan total transaksi setiap kali pesanan diserahkan
        TotalTransaksi = TotalTransaksi + 1;

        if (pesananBenar)
        {
            // Jika pesanan sesuai, tambah skor benar
            TransaksiBenar = TransaksiBenar + 1;
            Debug.Log("Transaksi BENAR! Total benar: " + TransaksiBenar);
        }
        else
        {
            // Jika pesanan keliru, tambah skor salah
            TransaksiSalah = TransaksiSalah + 1;
            Debug.Log("Transaksi SALAH! Total salah: " + TransaksiSalah);
        }
    }

    // Dipanggil oleh NPCPelanggan setelah pesanan diterima untuk mengosongkan kembali tangan pemain
    public void BersihkanBawaan()
    {
        // Mereset semua data bahan makanan di memori kembali menjadi nol
        BawaanPemain.ResetSemua();
        
        // Mematikan status bawa makanan karena pesanan sudah diserahkan
        BawaMakanan = false;
        Debug.Log("Bawaan pemain sudah dikosongkan.");
    }
    #endregion

    #region FUNGSI ANTRIAN NPC
    // Dipanggil oleh script NPC saat mereka baru muncul dan siap memesan
    public void DaftarkanNPC(NPCPelanggan npc)
    {
        // Mengecek agar tidak ada NPC yang mendaftar dobel di daftar aktif
        if (DaftarNPCAktif.Contains(npc) == false)
        {
            // Menambahkan NPC baru ke dalam daftar antrian aktif
            DaftarNPCAktif.Add(npc);
            Debug.Log("NPC terdaftar aktif. Total NPC aktif: " + DaftarNPCAktif.Count);
        }
    }

    // Dipanggil saat NPC pulang (setelah transaksi selesai) untuk membersihkan antrian
    public void HapusNPCDariDaftar(NPCPelanggan npc)
    {
        // Memastikan NPC tersebut memang ada di dalam daftar sebelum dihapus
        if (DaftarNPCAktif.Contains(npc))
        {
            // Mengeluarkan NPC dari daftar karena pesanannya sudah selesai
            DaftarNPCAktif.Remove(npc);
            Debug.Log("NPC dihapus dari daftar aktif. Sisa NPC aktif: " + DaftarNPCAktif.Count);
        }
    }
    #endregion
}
