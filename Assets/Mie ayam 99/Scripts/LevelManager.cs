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
    
    [Header("Pengaturan Latar Belakang")]
    [Tooltip("Masukkan GameObject background warung jualan normal ke sini.")]
    public GameObject BackgroundNormal;

    [Tooltip("Masukkan GameObject background khusus tutorial/dapur ke sini.")]
    public GameObject BackgroundProlog;
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

    [Tooltip("Teks untuk rincian struk (opsional, ditarik dari Inspector)")]
    // Menampilkan daftar jualan dan potongan bahan/pungli secara detail
    public TextMeshProUGUI TeksRincianReceipt;
    
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

            // Mengatur Latar Belakang (Background) berdasarkan tipe level (Prolog atau Normal)
            // Mematikan keduanya terlebih dahulu agar layar bersih
            if (BackgroundNormal != null) BackgroundNormal.SetActive(false);
            if (BackgroundProlog != null) BackgroundProlog.SetActive(false);

            // Cukup periksa apakah level ini dicentang sebagai "Adalah Prolog" di DataLevel
            if (levelAktif.AdalahProlog)
            {
                // Jika ini adalah prolog, aktifkan gambar latar belakang khusus prolog (dapur)
                if (BackgroundProlog != null) BackgroundProlog.SetActive(true);
            }
            else
            {
                // Jika level normal, aktifkan gambar latar belakang warung utama
                if (BackgroundNormal != null) BackgroundNormal.SetActive(true);
            }

            // --- PENGATURAN KAMERA OTOMATIS ---
            // Memindahkan kamera ke dapur (jika prolog) atau ke depan (jika normal)
            NavigasiKamera navigasi = FindObjectOfType<NavigasiKamera>();
            if (navigasi != null)
            {
                if (levelAktif.AdalahProlog)
                {
                    // Langsung set posisi instan agar tidak melihat warung depan saat loading
                    navigasi.transform.position = navigasi.PosisiDapur;
                    navigasi.PindahKeDapur(); // Set status internalnya juga
                }
                else
                {
                    // Pastikan kamera kembali ke depan untuk level normal
                    navigasi.transform.position = navigasi.PosisiDepan;
                    navigasi.PindahKeDepan();
                }
            }

            // Memastikan antrean selalu dimulai dari orang pertama (indeks 0) setiap ganti level
            indexAntreanNPC = 0;

            // Memeriksa apakah ada dialog awal level untuk dimainkan terlebih dahulu
            if (DialogueManager.instance != null && levelAktif.DialogAwalLevel != null)
            {
                // Memulai dialog awal level sebelum NPC pertama di-spawn
                DialogueManager.instance.MulaiDialog(levelAktif.DialogAwalLevel, () => {
                    // BUG FIX: Hanya mulai antrean NPC jika ini BUKAN level prolog
                    if (!levelAktif.AdalahProlog) 
                    {
                        MulaiAntreanNPC();
                    }
                    else
                    {
                        // Jika level prolog, otomatis buka buku tutorial setelah dialog awal selesai
                        if (TutorialManager.instance != null) TutorialManager.instance.BukaTutorial();
                    }
                });
            }
            else
            {
                // Jika tidak ada dialog, langsung memulai antrean NPC (KECUALI level prolog)
                if (!levelAktif.AdalahProlog) 
                {
                    MulaiAntreanNPC();
                }
                else
                {
                    // Jika level prolog, otomatis buka buku tutorial langsung
                    if (TutorialManager.instance != null) TutorialManager.instance.BukaTutorial();
                }
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

    // Fungsi tambahan untuk mengecek apakah level saat ini adalah level prolog
    public bool ApakahLevelProlog()
    {
        if (DaftarLevel.Count == 0 || indexLevelSekarang >= DaftarLevel.Count) return false;
        return DaftarLevel[indexLevelSekarang].AdalahProlog;
    }

    // Fungsi untuk mengakhiri level prolog secara paksa/manual (dipanggil dari dapur)
    public void SelesaikanPrologManual()
    {
        if (DaftarLevel.Count > 0 && indexLevelSekarang < DaftarLevel.Count)
        {
            DataLevel levelAktif = DaftarLevel[indexLevelSekarang];
            
            Debug.Log("Tutorial/Prolog diselesaikan secara manual oleh pemain!");
            
            // Sama seperti alur normal, kita cek apakah ada dialog penutup
            if (DialogueManager.instance != null && levelAktif.DialogAkhirLevel != null)
            {
                DialogueManager.instance.MulaiDialog(levelAktif.DialogAkhirLevel, () => {
                    LanjutKeFaseAkhir(levelAktif);
                });
            }
            else
            {
                LanjutKeFaseAkhir(levelAktif);
            }
        }
    }

    // Fungsi tambahan untuk memisahkan logika pemanggilan dialog moralitas
    // Bertindak sebagai jembatan antara selesainya dialog biasa dan munculnya Popup Result atau Diary
    private void LanjutKeFaseAkhir(DataLevel levelAktif)
    {
        // Memeriksa apakah ada dialog moralitas yang diatur untuk level ini
        if (MoralityManager.instance != null && levelAktif.DialogMoralitasAkhirLevel != null)
        {
            // Memanggil sistem moralitas, dan memerintahkan memproses akhir level setelahnya
            MoralityManager.instance.MulaiDialogMoralitas(levelAktif.DialogMoralitasAkhirLevel, () => {
                ProsesPenyelesaianLevel(levelAktif);
            });
        }
        else
        {
            // Jika tidak ada data dialog moralitas, langsung proses penyelesaian level
            ProsesPenyelesaianLevel(levelAktif);
        }
    }

    // Menentukan apakah level akan menampilkan panel rekap uang atau langsung ke buku harian
    private void ProsesPenyelesaianLevel(DataLevel levelAktif)
    {
        if (levelAktif.AdalahProlog)
        {
            // Jika ini level prolog (tutorial), LEWATI panel result/rekap uang
            // Langsung panggil Buku Harian
            if (DiaryManager.instance != null)
            {
                DiaryManager.instance.TampilkanBukuHarian(indexLevelSekarang);
            }
            else
            {
                // Jika buku harian tidak ada, langsung lanjut ke level 1
                LanjutKeLevelBerikutnya();
            }
        }
        else
        {
            // Jika bukan prolog, tampilkan popup rekap hasil jualan normal
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
                TeksJudulResult.text = "Today's Sales";

            // Mengambil angka-angka keuangan dari kasir (EconomyManager)
            if (EconomyManager.instance != null)
            {
                // Eksekusi potong bahan & pungli terlebih dahulu sebelum memunculkan popup
                EconomyManager.instance.ProsesPotonganAkhirHari();

                // Isi rincian struk ke dalam panel jika slot teksnya sudah dimasukkan dari Inspector
                if (TeksRincianReceipt != null)
                {
                    TeksRincianReceipt.text = EconomyManager.instance.DapatkanTeksStruk();
                }

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
                
                waktuBerjalan += Time.unscaledDeltaTime;
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
                
                waktuBerjalan += Time.unscaledDeltaTime;
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
