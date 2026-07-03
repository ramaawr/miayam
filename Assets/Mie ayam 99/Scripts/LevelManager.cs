using UnityEngine;
using TMPro; 
using System.Collections;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    #region SINGLETON
    // Akses cepat dari script lain (seperti GameManager) tanpa harus mencari GameObject-nya
    public static LevelManager instance;
    #endregion

    #region PENGATURAN LEVEL & ANTREAN
    [Header("Pengaturan Level")]
    [Tooltip("Daftar level yang akan dimainkan secara berurutan")]
    // Menyimpan rentetan database level (Level 1, Level 2, dst) yang sudah dibuat di Inspector
    public List<DataLevel> DaftarLevel = new List<DataLevel>();

    [Tooltip("Indeks level saat ini (0 = level pertama)")]
    // Mengingat posisi level pemain saat ini di dalam list (dimulai dari indeks 0)
    private int indexLevelSekarang = 0;

    [Tooltip("Indeks NPC ke-berapa yang sedang dilayani di level ini")]
    // Mengingat antrean nomor berapa yang sedang dilayani pemain pada level tersebut
    private int indexAntreanNPC = 0;
    #endregion

    #region PENGATURAN NPC & JEDA
    [Header("Pengaturan NPC & Jeda")]
    [Tooltip("Drag 1 GameObject NPC dari area depan ke sini")]
    // Referensi tunggal ke karakter NPC yang berdiri di depan gerobak
    public NPCPelanggan NPCUtama;

    [Tooltip("Waktu tunggu (detik) setelah NPC lama pergi sebelum NPC baru muncul")]
    // Durasi kekosongan/jeda sebelum pembeli selanjutnya berjalan masuk ke layar
    public float JedaWaktuMunculNPC = 2.0f;
    #endregion

    #region REFERENSI UI
    [Header("Referensi UI HUD")]
    [Tooltip("Drag Teks UI untuk penunjuk level (misal tulisan 'LEVEL 1') ke sini")]
    // Mengubah tulisan yang menunjukkan level saat ini di layar HUD pemain
    public TextMeshProUGUI TeksLevelCounter;

    [Header("Referensi UI Popup Result")]
    [Tooltip("Panel UI yang berisi rincian pendapatan hari ini")]
    // Referensi ke panel rekap yang akan muncul saat sebuah level selesai (habis NPC-nya)
    public GameObject PanelPopupResult;
    
    [Tooltip("Teks judul di popup (misal: Hari ke-1 Selesai!)")]
    // Mengubah judul pada panel rekap agar sesuai dengan nomor level yang baru diselesaikan
    public TextMeshProUGUI TeksJudulResult;
    
    [Tooltip("Teks untuk menampilkan pendapatan di level ini")]
    // Menampilkan jumlah uang yang HANYA didapatkan pada level/hari tersebut
    public TextMeshProUGUI TeksPendapatanResult;
    
    [Tooltip("Teks untuk menampilkan total uang secara keseluruhan")]
    // Menampilkan seluruh uang pemain (akumulasi) di panel rekap
    public TextMeshProUGUI TeksTotalUangResult;

    [Header("Transisi Level")]
    [Tooltip("Panel UI yang menutupi layar untuk efek Fade Out/In. Wajib punya CanvasGroup.")]
    // Ini digunakan agar layar bisa menjadi gelap/putih perlahan saat ganti hari (Fade)
    public CanvasGroup PanelFadeTransisi;
    
    [Tooltip("Waktu yang dibutuhkan untuk layar menjadi gelap atau terang (dalam detik)")]
    public float DurasiFade = 1.0f;
    #endregion

    #region INISIALISASI AWAL
    void Awake()
    {
        // Memastikan hanya ada satu pengontrol level di permainan
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Menyembunyikan NPC terlebih dahulu agar tidak langsung terlihat diam di awal game
        if (NPCUtama != null)
        {
            NPCUtama.gameObject.SetActive(false);
        }

        // Menyembunyikan panel rekap level agar layar pemain bersih saat mulai bermain
        if (PanelPopupResult != null)
        {
            PanelPopupResult.SetActive(false);
        }
        
        // Memastikan layar tidak gelap karena fade transisi
        if (PanelFadeTransisi != null)
        {
            PanelFadeTransisi.alpha = 0f;
            PanelFadeTransisi.gameObject.SetActive(false);
        }

        // Langsung memulai permainan dari indeks 0 (level pertama)
        MulaiLevel(0);
    }
    #endregion

    #region ALUR PERMAINAN (LEVELING)
    // Dipanggil untuk mengganti dan memulai level baru beserta mereset antreannya
    private void MulaiLevel(int indexLevel)
    {
        // Mengecek apakah masih ada level selanjutnya di dalam daftar (tidak out of bounds)
        if (indexLevel < DaftarLevel.Count)
        {
            // Mengatur level saat ini ke indeks yang diminta
            indexLevelSekarang = indexLevel;
            DataLevel levelAktif = DaftarLevel[indexLevelSekarang];

            Debug.Log("==== MEMULAI LEVEL " + levelAktif.NomorLevel + " ====");

            // Memperbarui tulisan penunjuk level di UI jika komponennya sudah di-assign
            if (TeksLevelCounter != null)
            {
                TeksLevelCounter.text = "" + levelAktif.NomorLevel;
            }

            // Memastikan antrean selalu dimulai dari orang pertama (indeks 0) setiap ganti level
            indexAntreanNPC = 0;

            // Memeriksa apakah ada dialog awal level untuk dimainkan terlebih dahulu
            if (DialogueManager.instance != null && levelAktif.DialogAwalLevel != null)
            {
                // Memulai dialog awal level sebelum NPC pertama di-spawn
                DialogueManager.instance.MulaiDialog(levelAktif.DialogAwalLevel, MulaiAntreanNPC);
            }
            else
            {
                // Jika tidak ada dialog, langsung memulai antrean NPC secara langsung
                MulaiAntreanNPC();
            }
        }
        else
        {
            // Jika sudah tidak ada level lagi, berarti game tamat
            Debug.Log("==== SEMUA LEVEL TELAH SELESAI! TELAATTT ====");
            if (TeksLevelCounter != null) TeksLevelCounter.text = "TAMAT!";
        }
    }



    // Fungsi khusus untuk memulai memanggil NPC pertama
    public void MulaiAntreanNPC()
    {
        StartCoroutine(ProsesMunculkanNPC(0f)); 
    }

    // Fungsi Coroutine untuk menunggu beberapa saat sebelum menyuruh NPC berikutnya maju
    private IEnumerator ProsesMunculkanNPC(float waktuJeda)
    {
        // Menunda eksekusi kode selanjutnya sesuai detik yang ditentukan
        yield return new WaitForSeconds(waktuJeda);

        DataLevel levelAktif = DaftarLevel[indexLevelSekarang];
        ProfilNPC profilBerikutnya = levelAktif.AntreanNPC[indexAntreanNPC];

        // Menyuntikkan (inject) data profil (wajah, pesanan, dialog) ke badan NPC yang ada di scene
        NPCUtama.MuatProfil(profilBerikutnya);

        // Menampilkan kembali NPC tersebut agar seolah-olah berjalan masuk
        NPCUtama.gameObject.SetActive(true);

        Debug.Log("NPC Muncul: " + profilBerikutnya.NamaNPC);

        // NPC sudah selesai masuk dan siap menunggu diklik
        Debug.Log("NPC siap diklik untuk memesan.");
    }


    #endregion

    #region TRANSAKSI DAN ANTRIAN


    // Dipanggil saat NPC benar-benar selesai (setelah dialog penutup jika ada)
    public void NPCSelesaiDilayani(NPCPelanggan npc)
    {
        // Mematikan NPC di layar (seolah-olah dia pulang)
        npc.TinggalkanToko();

        // Menggeser nomor antrean ke orang berikutnya
        indexAntreanNPC++;

        DataLevel levelAktif = DaftarLevel[indexLevelSekarang];

        // Mengecek apakah masih ada orang yang mengantre di level ini
        if (indexAntreanNPC < levelAktif.AntreanNPC.Count)
        {
            // Jika masih ada, panggil orang berikutnya setelah waktu jeda yang ditentukan
            StartCoroutine(ProsesMunculkanNPC(JedaWaktuMunculNPC));
        }
        else
        {
            // Semua orang di level ini sudah habis
            Debug.Log("Antrean Level " + levelAktif.NomorLevel + " Selesai!");

            // Memeriksa apakah ada dialog penutup di akhir level hari ini
            if (DialogueManager.instance != null && levelAktif.DialogAkhirLevel != null)
            {
                // Mainkan dialog penutup terlebih dahulu sebelum masuk ke fase akhir
                DialogueManager.instance.MulaiDialog(levelAktif.DialogAkhirLevel, () => {
                    LanjutKeFaseAkhir(levelAktif);
                });
            }
            else
            {
                // Langsung lanjut ke fase akhir tanpa dialog penutup
                LanjutKeFaseAkhir(levelAktif);
            }
        }
    }

    // Fungsi tambahan untuk memisahkan logika pemanggilan dialog moralitas
    // Bertindak sebagai jembatan antara selesainya dialog biasa dan munculnya Popup Result
    private void LanjutKeFaseAkhir(DataLevel levelAktif)
    {
        // Memeriksa apakah ada dialog moralitas yang diatur untuk level ini
        if (MoralityManager.instance != null && levelAktif.DialogMoralitasAkhirLevel != null)
        {
            // Memanggil sistem moralitas, dan memerintahkan memanggil Popup Result setelahnya
            MoralityManager.instance.MulaiDialogMoralitas(levelAktif.DialogMoralitasAkhirLevel, () => {
                TampilkanPopupResult(levelAktif.NomorLevel);
            });
        }
        else
        {
            // Jika tidak ada data dialog moralitas, langsung rekap pendapatan harian
            TampilkanPopupResult(levelAktif.NomorLevel);
        }
    }
    #endregion

    #region REKAP HARIAN (POPUP RESULT) & TRANSISI BUKU HARIAN
    // Fungsi untuk memunculkan layar rekap keuangan di penghujung level
    private void TampilkanPopupResult(int nomorLevel)
    {
        if (PanelPopupResult != null)
        {
            // Menyalakan (menampilkan) panel popup di atas layar
            PanelPopupResult.SetActive(true);

            // Menyesuaikan teks judul popup dengan nomor hari yang baru selesai
            if (TeksJudulResult != null)
                TeksJudulResult.text = "Hari ke-" + nomorLevel + " Selesai!";

            // Mengambil angka-angka keuangan dari kasir (EconomyManager)
            if (EconomyManager.instance != null)
            {
                if (TeksPendapatanResult != null)
                    TeksPendapatanResult.text = "Rp " + EconomyManager.instance.PendapatanHariIni;
                
                if (TeksTotalUangResult != null)
                    TeksTotalUangResult.text = "Rp " + EconomyManager.instance.TotalUangKeseluruhan;
            }
        }
        else
        {
            // Keamanan: Jika developer lupa memasukkan UI Popup, game akan otomatis lompat ke level depan
            LanjutKeLevelBerikutnya();
        }
    }

    // Dipanggil saat pemain meng-klik tombol "Lanjut Hari Berikutnya" di popup rekap
    public void TombolLanjutHariDipencet()
    {
        // Menyembunyikan layar popup rekap agar pemain bisa melihat gamenya lagi
        if (PanelPopupResult != null)
        {
            PanelPopupResult.SetActive(false);
        }

        // MENGHUBUNGKAN KE BUKU HARIAN:
        // Jika DiaryManager sudah dipasang, tampilkan buku harian sebelum pindah level
        if (DiaryManager.instance != null)
        {
            DiaryManager.instance.TampilkanBukuHarian(indexLevelSekarang);
        }
        else
        {
            // Jika tidak ada DiaryManager, langsung loncat ke proses ganti level
            LanjutKeLevelBerikutnya();
        }
    }
    
    // Fungsi baru ini dipanggil oleh DiaryManager setelah pemain menutup panel buku harian
    // Fungsi ini bertugas mereset uang dan memicu animasi Fade
    public void LanjutKeLevelBerikutnya()
    {
        // Menyuruh kasir (EconomyManager) untuk me-reset catatan uang khusus hari itu
        if (EconomyManager.instance != null)
        {
            EconomyManager.instance.ResetPendapatanHarian();
        }

        // Mulai animasi transisi layar gelap (Fade Out), ganti data level, lalu layar terang lagi (Fade In)
        StartCoroutine(ProsesGantiLevelDenganFadeOut(indexLevelSekarang + 1));
    }
    
    // Coroutine untuk transisi antar level dengan animasi Fade yang halus
    private IEnumerator ProsesGantiLevelDenganFadeOut(int indexBerikutnya)
    {
        // 1. FADE OUT (Layar berangsur menjadi tertutup/gelap/putih)
        if (PanelFadeTransisi != null)
        {
            PanelFadeTransisi.gameObject.SetActive(true);
            float waktuBerjalan = 0f;
            
            while (waktuBerjalan < DurasiFade)
            {
                // Matematika fraksi eksak: waktu berjalan per total durasi
                // Hasilkan nilai transisi dari 0 menuju 1 secara akurat tanpa pembulatan kasar
                float fraksiFade = waktuBerjalan / DurasiFade;
                PanelFadeTransisi.alpha = fraksiFade;
                
                waktuBerjalan += Time.deltaTime;
                yield return null; // Tunggu satu frame
            }
            
            // Pastikan panel benar-benar pekat di akhir animasi
            PanelFadeTransisi.alpha = 1f;
        }
        
        // 2. GANTI LEVEL 
        // Pada titik ini layar sedang tertutup, kita bisa mengganti level tanpa ketahuan pemain
        MulaiLevel(indexBerikutnya);
        
        // 3. FADE IN (Layar berangsur menjadi tembus pandang/hilang)
        if (PanelFadeTransisi != null)
        {
            float waktuBerjalan = 0f;
            
            while (waktuBerjalan < DurasiFade)
            {
                // Menghitung mundur dari pekat (1) menuju hilang (0)
                float fraksiFade = waktuBerjalan / DurasiFade;
                PanelFadeTransisi.alpha = 1f - fraksiFade;
                
                waktuBerjalan += Time.deltaTime;
                yield return null;
            }
            
            // Pastikan panel benar-benar tembus pandang di akhir
            PanelFadeTransisi.alpha = 0f;
            // Matikan objeknya agar tidak menghalangi pemain mengeklik objek di layar
            PanelFadeTransisi.gameObject.SetActive(false); 
        }
    }
    #endregion
}
