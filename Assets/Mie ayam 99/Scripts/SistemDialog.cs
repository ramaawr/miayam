using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Kita pakai TextMeshPro agar tampilan teks tajam dan elegan
using UnityEngine.UI; // Butuh ini untuk komponen Button dan UI dasar

[System.Serializable]
public struct BarisDialog
{
    [Tooltip("Nama karakter yang sedang bicara pada baris ini.")]
    public string namaKarakter;

    [Tooltip("Kalimat yang diucapkan.")]
    [TextArea(2, 4)]
    public string kalimat;
}

[System.Serializable]
public class BlokDialog
{
    [Header("ID Blok (Wajib Unik)")]
    [Tooltip("Nama ID untuk memanggil blok percakapan ini (misal: 'awal_level', 'pilih_menu')")]
    public string dialogID;

    [Header("Daftar Percakapan (Otomatis Berurutan)")]
    [Tooltip("Kalimat-kalimat ini akan dimainkan berurutan dari atas ke bawah saat pemain klik layar.")]
    public List<BarisDialog> barisDialog = new List<BarisDialog>();

    [Header("Tujuan Setelah Blok Selesai")]
    [Tooltip("ID Blok selanjutnya jika percakapan ini lurus tanpa pilihan. Kosongkan (atau isi 'SELESAI') jika obrolan tamat di sini.")]
    public string nextDialogID;

    [Header("Pilihan Bercabang (Tampil di Akhir Blok)")]
    [Tooltip("Centang ini jika di akhir daftar percakapan di atas, pemain harus memilih 2 opsi.")]
    public bool punyaPilihan;

    public string pilihan1_Teks;
    public string pilihan1_NextID;
    public int pilihan1_EfekMoralitas;

    public string pilihan2_Teks;
    public string pilihan2_NextID;
    public int pilihan2_EfekMoralitas;
}

public class SistemDialog : MonoBehaviour
{
    #region SINGLETON
    public static SistemDialog instance;
    #endregion

    public enum TipeDialog
    {
        Biasa,
        AwalLevel,
        SebelumPesanan,
        SetelahPesanan,
        AkhirLevel
    }

    [Header("Referensi UI Utama")]
    // Panel blocker transparan untuk mencegah pemain klik area game (NPC/Dapur) saat dialog berlangsung.
    [SerializeField] private GameObject panelBlocker;

    // Objek Panel utama yang menampung seluruh UI dialog.
    [SerializeField] private GameObject panelDialog;
    
    // Tempat menampilkan nama karakter yang sedang bicara.
    [SerializeField] private TextMeshProUGUI teksNama;
    
    // Tempat menampilkan kalimat dialog.
    [SerializeField] private TextMeshProUGUI teksKalimat;

    // Tombol Exit (keluar) yang hanya muncul ketika dialog sudah selesai sepenuhnya
    [SerializeField] private GameObject tombolExit;

    [Header("Pengaturan Input Dialog")]
    // Jeda waktu minimal (dalam detik) antar klik agar pemain tidak sengaja melewati dialog dengan cepat (spam klik)
    [SerializeField] private float cooldownKlik = 0.2f;

    // Menyimpan catatan waktu kapan pemain terakhir kali mengklik untuk lanjut
    private float waktuKlikTerakhir = 0f;

    [Header("Referensi UI Pilihan Bercabang")]
    // Panel/Container yang menampung tombol-tombol pilihan.
    [SerializeField] private GameObject choicePanel;

    // Komponen teks pada Tombol Pilihan 1
    [SerializeField] private TextMeshProUGUI teksPilihan1;

    // Komponen teks pada Tombol Pilihan 2
    [SerializeField] private TextMeshProUGUI teksPilihan2;

    [Header("Daftar Alur Dialog (Jika Manual)")]
    // List seluruh blok dialog yang kamu buat secara manual. 
    [SerializeField] private List<BlokDialog> daftarBlokDialog;
    
    // Kecepatan mesin tik mengetik huruf demi huruf (semakin kecil, semakin cepat).
    [SerializeField] private float kecepatanKetik = 0.04f;

    [Header("Pengaturan Auto-Size Teks")]
    // Aktifkan ini kalau kamu ingin ukuran font teks otomatis mengecil/membesar agar pas dengan ukuran kotak dialogbox (RectTransform)
    [SerializeField] private bool gunakanAutoSize = true;

    // Batas terkecil ukuran font agar teks tidak kekecilan dan tetap nyaman dibaca oleh pemain
    [SerializeField] private float ukuranFontMinimal = 12f;

    // Batas terbesar ukuran font agar teks tidak terlalu raksasa saat kalimatnya sangat pendek
    [SerializeField] private float ukuranFontMaksimal = 36f;

    [Header("Pengaturan Auto-Size Teks Pilihan (Choices)")]
    // Aktifkan ini kalau kamu ingin ukuran font teks pilihan otomatis mengecil/membesar agar pas dengan ukuran tombol pilihan
    [SerializeField] private bool gunakanAutoSizePilihan = true;

    // Batas terkecil ukuran font teks pilihan agar tetap terbaca dengan jelas
    [SerializeField] private float ukuranFontPilihanMinimal = 10f;

    // Batas terbesar ukuran font teks pilihan agar tidak terlalu besar saat teksnya pendek
    [SerializeField] private float ukuranFontPilihanMaksimal = 28f;

    [Header("Referensi NPC (Opsional)")]
    // Referensi ke script NPCMoralitas, dipakai jika dialog ini akan memengaruhi moralitas/health NPC.
    [SerializeField] private NPCMoralitas npcTarget;

    // Menyimpan tipe dialog yang sedang berjalan agar kita tahu ke mana alur gamenya setelah ini.
    private TipeDialog tipeDialogAktif = TipeDialog.Biasa;

    // Blok dialog yang saat ini sedang aktif.
    private BlokDialog blokAktif;
    
    // Indeks baris percakapan yang sedang dimainkan di dalam blokAktif.
    private int indeksBarisAktif = 0;
    
    // Pengaman (Tameng Pelindung) agar jika pemain klik brutal saat teks mengetik, tidak terjadi bug.
    private bool isTyping = false;
    
    // Menyimpan Coroutine mengetik agar bisa dihentikan paksa (skip) saat diklik tengah jalan.
    private Coroutine coroutineMengetik;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Pas game baru mulai, kita pastikan panel dialog dan panel pilihan tersembunyi terlebih dahulu.
        if (panelBlocker != null) panelBlocker.SetActive(false);
        if (panelDialog != null) panelDialog.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);
    }

    void Update()
    {
        // Deteksi klik kiri mouse atau tombol Spasi
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            // Cek apakah waktu klik saat ini sudah melewati batas cooldown pelindung
            if (Time.time - waktuKlikTerakhir < cooldownKlik)
            {
                return; // Abaikan klik jika terlalu cepat (anti-spam)
            }

            // Pastikan kita hanya memproses input jika panel dialog sedang aktif dan panel pilihan TIDAK sedang aktif.
            // (Kalau sedang memilih jawaban, input klik/spasi kita kunci agar pemain fokus memilih).
            if (panelDialog != null && panelDialog.activeSelf && (choicePanel == null || !choicePanel.activeSelf))
            {
                // Catat waktu klik terakhir untuk cooldown berikutnya
                waktuKlikTerakhir = Time.time;
                TampilkanDialogBerikutnya();
            }
        }
    }

    // Fungsi utama untuk memulai atau mengulang kembali sistem dialog dari awal.
    // Kita buat versi tanpa parameter agar tombol Start di UI bisa langsung memanggilnya secara dinamis.
    public void MulaiDialog()
    {
        MulaiDialog("mulai");
    }

    // Fungsi utama dengan parameter ID awal dialog, berguna jika dipicu dari script game lain.
    public void MulaiDialog(string mulaiID)
    {
        tipeDialogAktif = TipeDialog.Biasa;
        JalankanBlok(mulaiID);
    }

    // Fungsi baru untuk memulai dialog dari kumpulan blok khusus level (Awal, Akhir, dll)
    public void MulaiDialogLevel(List<BlokDialog> daftarBlokBaru, TipeDialog tipe)
    {
        if (daftarBlokBaru == null || daftarBlokBaru.Count == 0) return;
        
        daftarBlokDialog = daftarBlokBaru;
        tipeDialogAktif = tipe;
        JalankanBlok(daftarBlokDialog[0].dialogID);
    }

    // Fungsi internal untuk mereset UI dan memutar blok berdasarkan ID-nya
    private void JalankanBlok(string mulaiID)
    {
        if (daftarBlokDialog == null || daftarBlokDialog.Count == 0)
        {
            Debug.LogWarning("Daftar Blok Dialog masih kosong! Harap isi data dialog.");
            return;
        }

        // Cari blok pertama berdasarkan ID awal
        BlokDialog blokPertama = CariBlokBerdasarkanID(mulaiID);
        
        // Jika dialog dengan ID tersebut tidak ditemukan, ambil elemen pertama saja sebagai pengaman
        if (string.IsNullOrEmpty(blokPertama.dialogID))
        {
            blokPertama = daftarBlokDialog[0];
        }

        blokAktif = blokPertama;
        indeksBarisAktif = 0; // Mulai dari baris pertama di blok ini
        
        // Aktifkan panel blocker layar
        if (panelBlocker != null) panelBlocker.SetActive(true);
        // Aktifkan panel utama
        panelDialog.SetActive(true);
        // Sembunyikan panel pilihan di awal
        if (choicePanel != null) choicePanel.SetActive(false);
        // Sembunyikan tombol exit di awal dialog
        if (tombolExit != null) tombolExit.SetActive(false);

        TampilkanBarisSekarang();
    }

    // Fungsi pembantu untuk mencari data blok berdasarkan ID uniknya di dalam List daftarBlokDialog
    private BlokDialog CariBlokBerdasarkanID(string id)
    {
        foreach (BlokDialog blok in daftarBlokDialog)
        {
            if (blok.dialogID == id)
            {
                return blok;
            }
        }
        
        // Jika tidak ditemukan, kembalikan objek kosong/default
        return default(BlokDialog);
    }

    // Menampilkan baris dialog yang sedang aktif (berdasarkan indeksBarisAktif)
    private void TampilkanBarisSekarang()
    {
        // Pengaman jika blok tidak memiliki baris dialog sama sekali
        if (blokAktif.barisDialog == null || blokAktif.barisDialog.Count == 0)
        {
            Debug.LogWarning("Blok dialog '" + blokAktif.dialogID + "' tidak memiliki baris percakapan!");
            SelesaiDialog();
            return;
        }

        BarisDialog barisSekarang = blokAktif.barisDialog[indeksBarisAktif];

        // Set nama karakter
        teksNama.text = barisSekarang.namaKarakter;

        // Reset panel pilihan ke kondisi normal sebelum mengetik
        if (choicePanel != null) choicePanel.SetActive(false);
        // Pastikan tombol exit disembunyikan saat sedang memuat kalimat baru
        if (tombolExit != null) tombolExit.SetActive(false);

        // Hentikan coroutine mengetik yang lama agar tidak tumpang tindih
        if (coroutineMengetik != null)
        {
            StopCoroutine(coroutineMengetik);
        }

        // --- SISTEM AUTO-SIZE DENGAN EFEK TYPEWRITER ---
        // Logika di bawah ini berfungsi agar teks bisa muat di dalam kotak dialogbox yang sudah ditentukan ukurannya.
        // Kita tidak bisa langsung menyalakan Auto-Size bawaan TextMeshPro karena nanti ukuran hurufnya akan mengecil secara bertahap saat diketik (kelihatan bergoyang/aneh).
        // Solusinya: Kita hitung dulu ukuran font terbaik untuk kalimat lengkap tersebut secara diam-diam, lalu kita kunci ukurannya baru jalankan efek mengetik.
        if (teksKalimat != null)
        {
            if (gunakanAutoSize)
            {
                // Langkah 1: Aktifkan fitur auto sizing di komponen TextMeshPro
                teksKalimat.enableAutoSizing = true;
                
                // Langkah 2: Berikan batas minimal dan maksimal ukuran font dari Inspector
                teksKalimat.fontSizeMin = ukuranFontMinimal;
                teksKalimat.fontSizeMax = ukuranFontMaksimal;
                
                // Langkah 3: Masukkan kalimat lengkap ke komponen teks agar TMP bisa mengukur dimensinya
                teksKalimat.text = barisSekarang.kalimat;
                
                // Langkah 4: Paksa TMP untuk memperbarui layout dan menghitung ukuran font optimal secara instan
                teksKalimat.ForceMeshUpdate();
                
                // Langkah 5: Simpan ukuran font optimal hasil kalkulasi tersebut
                float ukuranFontOptimal = teksKalimat.fontSize;
                
                // Langkah 6: Matikan fitur auto sizing agar ukuran font terkunci (tidak melompat-lompat saat diketik)
                teksKalimat.enableAutoSizing = false;
                
                // Langkah 7: Terapkan ukuran font optimal yang sudah dikunci tadi ke teks
                teksKalimat.fontSize = ukuranFontOptimal;
            }
            else
            {
                // Jika tidak menggunakan auto size, pastikan fiturnya mati dan pakai ukuran font maksimal sebagai default
                teksKalimat.enableAutoSizing = false;
                teksKalimat.fontSize = ukuranFontMaksimal;
            }
        }

        // Jalankan coroutine mengetik untuk kalimat saat ini
        coroutineMengetik = StartCoroutine(KetikKalimat(barisSekarang.kalimat));
    }

    // Coroutine efek mengetik mesin tik
    private IEnumerator KetikKalimat(string kalimat)
    {
        isTyping = true;
        teksKalimat.text = "";

        foreach (char huruf in kalimat.ToCharArray())
        {
            teksKalimat.text += huruf;
            yield return new WaitForSeconds(kecepatanKetik);
        }

        isTyping = false;
        
        // Setelah kalimat selesai diketik, cek apakah ini baris terakhir di dalam blok ini?
        if (indeksBarisAktif >= blokAktif.barisDialog.Count - 1)
        {
            if (blokAktif.punyaPilihan)
            {
                MunculkanPilihanCabang();
            }
            else if (ApakahDialogSelesai())
            {
                // Jika tidak ada pilihan dan blok sudah habis, munculkan tombol exit (jika ada)
                MunculkanTombolExit();
            }
        }
    }

    // Fungsi untuk memicu munculnya tombol pilihan jawaban di layar
    private void MunculkanPilihanCabang()
    {
        // Konfigurasi Auto Size untuk Teks Pilihan 1
        if (teksPilihan1 != null)
        {
            if (gunakanAutoSizePilihan)
            {
                // Aktifkan fitur auto sizing di komponen TextMeshPro pilihan
                teksPilihan1.enableAutoSizing = true;
                teksPilihan1.fontSizeMin = ukuranFontPilihanMinimal;
                teksPilihan1.fontSizeMax = ukuranFontPilihanMaksimal;
            }
            else
            {
                // Matikan auto sizing dan gunakan ukuran font maksimal secara default
                teksPilihan1.enableAutoSizing = false;
                teksPilihan1.fontSize = ukuranFontPilihanMaksimal;
            }
            teksPilihan1.text = blokAktif.pilihan1_Teks;
        }

        // Konfigurasi Auto Size untuk Teks Pilihan 2
        if (teksPilihan2 != null)
        {
            if (gunakanAutoSizePilihan)
            {
                // Aktifkan fitur auto sizing di komponen TextMeshPro pilihan
                teksPilihan2.enableAutoSizing = true;
                teksPilihan2.fontSizeMin = ukuranFontPilihanMinimal;
                teksPilihan2.fontSizeMax = ukuranFontPilihanMaksimal;
            }
            else
            {
                // Matikan auto sizing dan gunakan ukuran font maksimal secara default
                teksPilihan2.enableAutoSizing = false;
                teksPilihan2.fontSize = ukuranFontPilihanMaksimal;
            }
            teksPilihan2.text = blokAktif.pilihan2_Teks;
        }

        // Aktifkan panel pilihan
        if (choicePanel != null) choicePanel.SetActive(true);
    }

    // Fungsi publik yang dipanggil ketika pemain mengklik tombol pilihan pertama atau kedua.
    // Menentukan ke ID blok dialog mana alur cerita akan melompat berikutnya.
    public void PilihCabang(string nextID)
    {
        // Sembunyikan kembali panel pilihan
        if (choicePanel != null) choicePanel.SetActive(false);

        // Jika ID tujuan kosong atau berisi "SELESAI", maka dialog ditutup
        if (string.IsNullOrEmpty(nextID) || nextID == "SELESAI")
        {
            SelesaiDialog();
            return;
        }

        // Jalankan blok baru sesuai pilihan
        JalankanBlok(nextID);
    }

    // Fungsi pembantu tanpa parameter untuk dihubungkan ke tombol Pilihan 1 di Unity UI onClick
    public void PilihOpsi1()
    {
        // Berikan efek moralitas ke NPC jika target NPC dipasang di inspector
        if (npcTarget != null && blokAktif.pilihan1_EfekMoralitas != 0)
        {
            npcTarget.UbahMoralitas(blokAktif.pilihan1_EfekMoralitas);
        }
        
        PilihCabang(blokAktif.pilihan1_NextID);
    }

    // Fungsi pembantu tanpa parameter untuk dihubungkan ke tombol Pilihan 2 di Unity UI onClick
    public void PilihOpsi2()
    {
        // Berikan efek moralitas ke NPC jika target NPC dipasang di inspector
        if (npcTarget != null && blokAktif.pilihan2_EfekMoralitas != 0)
        {
            npcTarget.UbahMoralitas(blokAktif.pilihan2_EfekMoralitas);
        }
        
        PilihCabang(blokAktif.pilihan2_NextID);
    }

    // Menangani aksi tombol Next linier atau shortcut klik/keyboard
    public void TampilkanDialogBerikutnya()
    {
        // KONDISI 1: Jika teks masih berjalan mengetik, percepat teks hingga langsung tampil penuh
        if (isTyping)
        {
            if (coroutineMengetik != null)
            {
                StopCoroutine(coroutineMengetik);
            }
            
            BarisDialog barisSekarang = blokAktif.barisDialog[indeksBarisAktif];
            teksKalimat.text = barisSekarang.kalimat;
            isTyping = false;

            // Karena skip, kita cek apakah ini di akhir blok?
            if (indeksBarisAktif >= blokAktif.barisDialog.Count - 1)
            {
                if (blokAktif.punyaPilihan)
                {
                    MunculkanPilihanCabang();
                }
                else if (ApakahDialogSelesai())
                {
                    // Jika diskip dan ternyata blok selesai, langsung munculkan tombol exit
                    MunculkanTombolExit();
                }
            }
        }
        // KONDISI 2: Jika teks sudah selesai diketik
        else
        {
            // Jika belum di baris terakhir blok, maju 1 baris
            if (indeksBarisAktif < blokAktif.barisDialog.Count - 1)
            {
                indeksBarisAktif++;
                TampilkanBarisSekarang();
            }
            // Jika sudah di baris terakhir, cek kelanjutan blok
            else
            {
                // Cek ke mana arah blok berikutnya
                string nextID = blokAktif.nextDialogID;

                // Jika tidak ada ID tujuan berikutnya atau diisi "SELESAI", akhiri percakapan
                if (string.IsNullOrEmpty(nextID) || nextID == "SELESAI")
                {
                    // Jika tombol exit dipasang di Inspector, biarkan pemain mengklik tombol Exit secara manual.
                    if (tombolExit != null)
                    {
                        return;
                    }

                    // Jika tombol exit tidak dipasang (opsional), klik kiri sembarang tetap menutup dialog
                    SelesaiDialog();
                    return;
                }

                // Muat blok berikutnya berdasarkan ID
                JalankanBlok(nextID);
            }
        }
    }

    // Fungsi pembantu untuk memeriksa apakah seluruh dialog saat ini sudah selesai secara alami
    public bool ApakahDialogSelesai()
    {
        // Dialog dianggap selesai jika:
        // 1. Teks tidak sedang dalam proses mengetik (typewriter selesai)
        // 2. Baris yang sedang dibaca adalah baris terakhir di dalam blok
        // 3. ID blok berikutnya kosong atau berisi "SELESAI"
        // 4. Blok ini tidak memiliki pilihan bercabang yang menggantung
        bool tidakSedangMengetik = !isTyping;
        bool sudahDiAkhirBaris = indeksBarisAktif >= blokAktif.barisDialog.Count - 1;
        bool tidakAdaDialogBerikutnya = string.IsNullOrEmpty(blokAktif.nextDialogID) || blokAktif.nextDialogID == "SELESAI";
        bool tidakAdaPilihan = !blokAktif.punyaPilihan;

        return tidakSedangMengetik && sudahDiAkhirBaris && tidakAdaDialogBerikutnya && tidakAdaPilihan;
    }

    // Menampilkan tombol exit secara aktif jika referensinya dipasang di Inspector
    private void MunculkanTombolExit()
    {
        if (tombolExit != null)
        {
            tombolExit.SetActive(true);
        }
    }

    // Fungsi untuk menutup dialog dan menyembunyikannya dari layar.
    // Kita tambahkan parameter 'paksa' (default false) agar sistem game lain bisa menutup paksa jika ada kejadian darurat.
    public void SelesaiDialog(bool paksa = false)
    {
        // Tombol exit atau fungsi penutup hanya bisa dipakai jika dipaksa oleh sistem,
        // ATAU jika dialog dari sistem memang sudah selesai secara alami.
        if (paksa || ApakahDialogSelesai())
        {
            if (panelBlocker != null) panelBlocker.SetActive(false);
            if (panelDialog != null) panelDialog.SetActive(false);
            if (choicePanel != null) choicePanel.SetActive(false);
            // Sembunyikan kembali tombol exit saat panel dialog ditutup
            if (tombolExit != null) tombolExit.SetActive(false);

            // Beri tahu LevelManager kelanjutan alur berdasarkan tipe dialog yang baru selesai
            if (LevelManager.instance != null)
            {
                if (tipeDialogAktif == TipeDialog.AwalLevel) LevelManager.instance.LanjutSetelahDialogAwal();
                else if (tipeDialogAktif == TipeDialog.AkhirLevel) LevelManager.instance.LanjutSetelahDialogAkhir();
                else if (tipeDialogAktif == TipeDialog.SebelumPesanan) LevelManager.instance.LanjutSetelahDialogNPC_Datang();
                else if (tipeDialogAktif == TipeDialog.SetelahPesanan) LevelManager.instance.LanjutSetelahDialogNPC_Pulang();
            }

            tipeDialogAktif = TipeDialog.Biasa;
        }
        else
        {
            Debug.Log("Pemain mencoba keluar, tetapi dialog saat ini belum selesai diketik atau percakapan belum selesai.");
        }
    }
}
