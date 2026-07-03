using UnityEngine;

// =============================================================================
// NPCPelanggan.cs — Interaksi NPC Pelanggan dengan Dual Mode Pesanan
// =============================================================================
// Script ini ditempel ke setiap GameObject NPC di Area Depan.
// Setiap NPC bisa diklik untuk:
//   1. Menerima pesanan (generate pesanan baru)
//   2. Menyerahkan makanan (validasi bawaan pemain)
//
// FITUR DUAL MODE:
//   - Mode Manual  → Developer set sendiri isi pesanan dari Inspector
//                     (cocok untuk level dengan cerita / pesanan spesifik)
//   - Mode Random  → Pesanan digenerate random dengan aturan:
//                     Mie minimal 1, Ayam minimal 1, Baso & Sayur random
//                     (cocok untuk mode endless / practice)
//
// Cara pakai:
//   1. Tempel script ini ke NPC (yang punya Collider2D!)
//   2. Pilih ModePesanan di Inspector: Manual atau Random
//   3. Kalau Manual → isi PesananManual di Inspector
//   4. Kalau Random → atur range min/max di Inspector
//   5. Assign BubbleChat ke referensi UI Bubble Chat
// =============================================================================

public class NPCPelanggan : MonoBehaviour
{
    // =========================================================================
    // ENUM MODE PESANAN — Pemilih mode di Inspector (dropdown)
    // =========================================================================
    public enum ModePesananType
    {
        Manual, // Developer atur sendiri isi pesanan
        Random  // Pesanan digenerate random
    }

    // =========================================================================
    // PENGATURAN MODE — Pilih dari Inspector
    // =========================================================================
    [Header("Mode Pesanan")]
    [Tooltip("Manual = isi pesanan diatur sendiri. Random = pesanan diacak otomatis.")]
    public ModePesananType ModePesanan = ModePesananType.Random;

    // =========================================================================
    // PESANAN MANUAL — Diisi developer dari Inspector
    // Hanya dipakai kalau ModePesanan == Manual
    // Cocok untuk level spesifik dengan cerita per pesanan
    // =========================================================================
    [Header("Pesanan Manual (untuk Mode Manual)")]
    [Tooltip("Isi pesanan yang sudah ditentukan. Hanya berlaku jika Mode = Manual.")]
    public DataPesanan PesananManual = new DataPesanan();

    // =========================================================================
    // PENGATURAN RANDOM — Range minimum & maximum untuk random
    // Hanya dipakai kalau ModePesanan == Random
    //
    // ATURAN RANDOM:
    //   - Mie  → selalu 1 (minimal, tidak bisa 0)
    //   - Ayam → minimal 1 (tidak bisa 0)
    //   - Baso → bisa 0 sampai MaxBaso
    //   - Sayur → bisa 0 sampai MaxSayur
    // =========================================================================
    [Header("Pengaturan Random (untuk Mode Random)")]
    [Tooltip("Jumlah baso minimum saat random")]
    public int MinBaso = 0;
    [Tooltip("Jumlah baso maksimum saat random")]
    public int MaxBaso = 3;

    [Tooltip("Jumlah ayam minimum saat random (minimal 1)")]
    public int MinAyam = 1;
    [Tooltip("Jumlah ayam maksimum saat random")]
    public int MaxAyam = 1;

    [Tooltip("Jumlah sayur minimum saat random")]
    public int MinSayur = 0;
    [Tooltip("Jumlah sayur maksimum saat random")]
    public int MaxSayur = 3;

    // =========================================================================
    // REFERENSI UI — Bubble Chat untuk menampilkan pesanan
    // Assign dari Inspector: drag BubbleChat GameObject ke sini
    // =========================================================================
    [Header("Referensi UI")]
    [Tooltip("Drag komponen BubbleChat milik NPC ini ke sini")]
    public BubbleChat bubbleChat;

    // =========================================================================
    // STATUS NPC — Apakah NPC ini sudah punya pesanan aktif?
    // =========================================================================
    [Header("Status NPC (Otomatis, jangan diedit)")]
    public bool SudahPesan = false; // Apakah NPC ini sudah punya pesanan yang belum dilayani?
    
    // "Tameng Pelindung" untuk mencegah race condition (double click) saat NPC sudah diberi makan
    public bool sedangPulang = false; 
    
    // "Tameng Pelindung" untuk mencegah race condition saat NPC sedang berdialog sebelum order
    public bool sedangBicara = false;

    // Data pesanan yang sedang aktif untuk NPC ini
    // (disimpan di NPC masing-masing, bukan di GameManager,
    //  supaya bisa support multiple NPC sekaligus)
    [HideInInspector]
    public DataPesanan PesananAktif = new DataPesanan();

    // Referensi profil yang sedang dimuat (untuk menyimpan data persisten seperti kesalahan)
    private ProfilNPC profilAktif;

    // =========================================================================
    // OnMouseDown — Saat NPC diklik oleh pemain
    // =========================================================================
    void OnMouseDown()
    {
        // ----- GUARD: Tameng Pelindung (Mencegah Race Condition) -----
        if (sedangPulang || sedangBicara) return;

        // ----- GUARD: Cek apakah pemain ada di Area Depan ----- 
        // Kalau masih di dapur, NPC tidak bisa diinteraksi
        if (GameManager.instance == null)
        {
            Debug.LogWarning("GameManager belum ada di scene!");
            return;
        }

        if (GameManager.instance.SedangDiDapur == true)
        {
            // Pemain masih di dapur, tidak bisa klik NPC
            return;
        }

        // ----- GUARD: Cek apakah kamera sedang transisi -----
        // Kalau kamera masih gerak, jangan terima input
        NavigasiKamera kamera = FindObjectOfType<NavigasiKamera>();
        if (kamera != null && kamera.SedangTransisi)
        {
            return;
        }

        // =================================================================
        // KASUS 1: Pemain bawa makanan → SERAHKAN ke NPC
        // =================================================================
        if (GameManager.instance.BawaMakanan == true && SudahPesan == true)
        {
            SerahkanPesanan();
            return;
        }

        // =================================================================
        // KASUS 2: NPC belum punya pesanan → BUAT PESANAN BARU
        // =================================================================
        if (SudahPesan == false)
        {
            // Memeriksa apakah ada dialog cerita sebelum order makanan
            if (DialogueManager.instance != null && profilAktif != null && profilAktif.DialogSebelumOrder != null)
            {
                // Nyalakan tameng sedangBicara agar tidak bisa diklik lagi saat dialog berjalan
                sedangBicara = true;

                // Mainkan dialog terlebih dahulu, baru panggil BuatPesananBaru setelah dialog selesai
                DialogueManager.instance.MulaiDialog(profilAktif.DialogSebelumOrder, BuatPesananBaru);
            }
            else
            {
                // Jika tidak ada dialog, langsung buat pesanan baru secara normal
                BuatPesananBaru();
            }
            return;
        }

        // =================================================================
        // KASUS 3: NPC sudah punya pesanan, tapi pemain belum bawa makanan
        //          → TAMPILKAN ULANG pesanan sebagai pengingat
        // =================================================================
        if (SudahPesan == true && GameManager.instance.BawaMakanan == false)
        {
            // Tampilkan bubble chat lagi sebagai pengingat
            if (bubbleChat != null)
            {
                bubbleChat.TampilkanPesanan(PesananAktif);
                Debug.Log("NPC mengingatkan pesanannya: " + PesananAktif.TampilkanSebagaiTeks());
            }
        }
    }

    // =========================================================================
    // MuatProfil — Dipanggil oleh LevelManager untuk menyuntikkan data NPC
    // Fungsi ini akan mengganti wujud NPC dan menimpa aturan pesanannya.
    // =========================================================================
    public void MuatProfil(ProfilNPC profil)
    {
        // Guard: Pastikan data profil tidak null
        if (profil == null)
        {
            Debug.LogError("MuatProfil: Profil NPC null!");
            return;
        }

        // 1. Ganti Wujud (Sprite) NPC
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null && profil.VisualNPC != null)
        {
            renderer.sprite = profil.VisualNPC;
        }

        // 2. Timpa pengaturan pesanan NPC ini dengan data dari ProfilNPC
        ModePesanan = profil.ModePesanan;
        if (ModePesanan == ModePesananType.Manual)
        {
            // Pastikan PesananManual milik NPC ini tidak null sebelum disalin
            if (PesananManual == null)
            {
                PesananManual = new DataPesanan();
            }
            PesananManual.SalinDari(profil.PesananManual);
        }
        else
        {
            MinBaso = profil.MinBaso;
            MaxBaso = profil.MaxBaso;
            MinAyam = profil.MinAyam;
            MaxAyam = profil.MaxAyam;
            MinSayur = profil.MinSayur;
            MaxSayur = profil.MaxSayur;
        }

        // Simpan referensi profil untuk mencatat kesalahan (fitur masa depan)
        profilAktif = profil;

        // Reset status (berjaga-jaga saat di-reuse)
        SudahPesan = false;
        sedangPulang = false; // Mematikan tameng perlindungan
        sedangBicara = false; // Mematikan tameng perlindungan dialog

        // Pastikan PesananAktif tidak null sebelum di-reset
        if (PesananAktif == null)
        {
            PesananAktif = new DataPesanan();
        }
        PesananAktif.ResetSemua();

        if (bubbleChat != null)
        {
            bubbleChat.SembunyikanBubble();
        }
    }

    // =========================================================================
    // TinggalkanToko — Menyembunyikan NPC dari layar setelah dilayani
    // Dipanggil oleh LevelManager
    // =========================================================================
    public void TinggalkanToko()
    {
        gameObject.SetActive(false);
    }

    // =========================================================================
    // BuatPesananBaru — Generate pesanan berdasarkan mode yang dipilih
    // =========================================================================
    private void BuatPesananBaru()
    {
        // Matikan tameng bicara karena dialog (jika ada) sudah selesai
        sedangBicara = false;

        // Pastikan PesananAktif tidak null
        if (PesananAktif == null)
        {
            PesananAktif = new DataPesanan();
        }

        if (ModePesanan == ModePesananType.Manual)
        {
            // Pastikan PesananManual tidak null
            if (PesananManual == null)
            {
                PesananManual = new DataPesanan();
            }
            // ----- MODE MANUAL: Salin dari pesanan yang sudah di-set di Inspector -----
            PesananAktif.SalinDari(PesananManual);
            Debug.Log("NPC buat pesanan MANUAL: " + PesananAktif.TampilkanSebagaiTeks());
        }
        else
        {
            // ----- MODE RANDOM: Generate angka acak sesuai range -----

            // Mie selalu 1 (wajib ada di setiap pesanan)
            PesananAktif.JumlahMie = 1;

            // Ayam minimal 1 (aturan dari developer)
            // Random.Range(min, max+1) → termasuk min dan max
            PesananAktif.JumlahAyam = Random.Range(MinAyam, MaxAyam + 1);

            // Baso bisa 0 (tidak wajib)
            PesananAktif.JumlahBaso = Random.Range(MinBaso, MaxBaso + 1);

            // Sayur bisa 0 (tidak wajib)
            PesananAktif.JumlahSayur = Random.Range(MinSayur, MaxSayur + 1);

            Debug.Log("NPC buat pesanan RANDOM: " + PesananAktif.TampilkanSebagaiTeks());
        }

        // Tandai NPC ini sudah punya pesanan
        SudahPesan = true;

        // Daftarkan NPC ke GameManager sebagai NPC aktif
        if (GameManager.instance != null)
        {
            GameManager.instance.DaftarkanNPC(this);
        }

        // Tampilkan bubble chat pesanan
        if (bubbleChat != null)
        {
            bubbleChat.TampilkanPesanan(PesananAktif);
        }
    }

    // =========================================================================
    // SerahkanPesanan — Validasi bawaan pemain dengan pesanan NPC
    // =========================================================================
    private void SerahkanPesanan()
    {
        // Mengaktifkan tameng agar NPC tidak bisa diklik lagi selama proses animasi pulang berjalan
        sedangPulang = true;

        // Ambil data bawaan pemain dari GameManager
        DataPesanan bawaan = GameManager.instance.BawaanPemain;

        // Bandingkan bawaan pemain dengan pesanan NPC
        bool pesananCocok = PesananAktif.SamaDengan(bawaan);

        if (pesananCocok)
        {
            // ----- PESANAN BENAR -----
            Debug.Log("COCOK! Pesanan NPC terpenuhi dengan benar!");

            // Tampilkan feedback positif di bubble chat
            if (bubbleChat != null)
            {
                bubbleChat.TampilkanFeedback("Terima kasih! Pesanan benar!");
            }
        }
        else
        {
            // ----- PESANAN SALAH (tetap diterima, tapi dicatat) -----
            Debug.Log("SALAH! Pesanan NPC: " + PesananAktif.TampilkanSebagaiTeks()
                     + " | Bawaan: " + bawaan.TampilkanSebagaiTeks());

            // Tampilkan feedback di bubble chat
            if (bubbleChat != null)
            {
                bubbleChat.TampilkanFeedback("Hmm, ini bukan pesananku... tapi ya sudah.");
            }


        }

        // Panggil sistem ekonomi untuk menghitung uang dari pesanan ini
        if (EconomyManager.instance != null)
        {
            EconomyManager.instance.HitungPendapatan(bawaan, pesananCocok);
        }

        // Catat transaksi ke riwayat (untuk fitur skor kedepannya)
        GameManager.instance.CatatTransaksi(pesananCocok);

        // Bersihkan bawaan pemain (makanan sudah diserahkan)
        GameManager.instance.BersihkanBawaan();

        // Reset status NPC ini
        SudahPesan = false;
        PesananAktif.ResetSemua();

        // Hapus NPC dari daftar aktif di GameManager
        GameManager.instance.HapusNPCDariDaftar(this);

        // Memulai coroutine untuk menunda NPC pergi agar pemain sempat membaca bubble chat feedback
        StartCoroutine(ProsesSerahkanPesanan(pesananCocok));
    }

    private System.Collections.IEnumerator ProsesSerahkanPesanan(bool pesananCocok)
    {
        // Tunggu feedback bubble chat selesai (mengikuti durasi yang disetel di BubbleChat)
        float waktuTunggu = 2.5f;
        if (bubbleChat != null)
        {
            waktuTunggu = bubbleChat.DurasiFeedback;
        }
        
        yield return new WaitForSeconds(waktuTunggu);

        // Sembunyikan bubble chat setelah durasi habis
        if (bubbleChat != null)
        {
            bubbleChat.SembunyikanBubble();
        }

        // Memeriksa apakah pesanan benar dan ada dialog setelah order (penutup NPC)
        if (pesananCocok && DialogueManager.instance != null && profilAktif != null && profilAktif.DialogSetelahOrder != null)
        {
            // Mainkan dialog penutup terlebih dahulu, lalu NPC baru pergi (TinggalkanToko) setelah selesai
            DialogueManager.instance.MulaiDialog(profilAktif.DialogSetelahOrder, () =>
            {
                if (LevelManager.instance != null)
                {
                    LevelManager.instance.NPCSelesaiDilayani(this);
                }
            });
        }
        else
        {
            // Jika pesanan salah atau tidak ada dialog penutup, NPC langsung pulang seperti biasa
            if (LevelManager.instance != null)
            {
                LevelManager.instance.NPCSelesaiDilayani(this);
            }
        }
    }
}

