using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Kita pakai TextMeshPro agar tampilan teks tajam dan elegan
using UnityEngine.UI; // Butuh ini untuk komponen Button dan UI dasar

// Struct ini dibungkus dengan DataDialog untuk menyimpan konfigurasi satu blok kalimat dialog.
// [System.Serializable] ini wajib agar isinya bisa diisi langsung lewat Unity Inspector.
[System.Serializable]
public struct DataDialog
{
    [Header("ID & Alur Dialog")]
    // ID Unik untuk menandai kalimat ini (misal: "sapaan", "pilih_bakso", "tanya_minum")
    public string dialogID;

    // Nama karakter yang nanti bakal muncul di kotak dialog
    public string namaKarakter;

    // Kalimat dialog yang diucapkan.
    [TextArea(3, 5)]
    public string kalimatDialog;

    // ID dialog berikutnya jika alur cerita berjalan lurus/linier (tanpa pilihan bercabang).
    // Kosongkan atau isi "SELESAI" untuk menutup dialog setelah kalimat ini selesai.
    public string nextDialogID;

    [Header("Pengaturan Pilihan Bercabang (Branching)")]
    // Centang/aktifkan ini jika setelah kalimat ini selesai diketik, pemain harus memilih jawaban.
    public bool punyaPilihan;

    // Teks yang muncul di Tombol Pilihan 1
    public string pilihan1_Teks;
    // ID dialog tujuan jika Tombol Pilihan 1 diklik
    public string pilihan1_NextID;
    // Efek penambahan/pengurangan moralitas NPC jika Pilihan 1 dipilih (misal: 10 atau -10)
    public int pilihan1_EfekMoralitas;

    // Teks yang muncul di Tombol Pilihan 2
    public string pilihan2_Teks;
    // ID dialog tujuan jika Tombol Pilihan 2 diklik
    public string pilihan2_NextID;
    // Efek penambahan/pengurangan moralitas NPC jika Pilihan 2 dipilih
    public int pilihan2_EfekMoralitas;
}

public class SistemDialog : MonoBehaviour
{
    [Header("Referensi UI Utama")]
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

    [Header("Daftar Alur Dialog")]
    // List seluruh baris dialog yang kamu buat. 
    // Setiap baris harus diberi dialogID yang berbeda agar tidak tertukar!
    [SerializeField] private List<DataDialog> daftarDialog;
    
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

    // Data dialog yang saat ini sedang aktif berjalan di layar.
    private DataDialog dialogAktif;
    
    // Pengaman (Tameng Pelindung) agar jika pemain klik brutal saat teks mengetik, tidak terjadi bug.
    private bool isTyping = false;
    
    // Menyimpan Coroutine mengetik agar bisa dihentikan paksa (skip) saat diklik tengah jalan.
    private Coroutine coroutineMengetik;

    void Start()
    {
        // Pas game baru mulai, kita pastikan panel dialog dan panel pilihan tersembunyi terlebih dahulu.
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
        if (daftarDialog == null || daftarDialog.Count == 0)
        {
            Debug.LogWarning("Daftar Dialog masih kosong! Harap isi data dialog di Inspector.");
            return;
        }

        // Cari dialog pertama berdasarkan ID awal
        DataDialog dialogPertama = CariDialogBerdasarkanID(mulaiID);
        
        // Jika dialog dengan ID tersebut tidak ditemukan, ambil elemen pertama saja sebagai pengaman
        if (string.IsNullOrEmpty(dialogPertama.dialogID))
        {
            dialogPertama = daftarDialog[0];
        }

        dialogAktif = dialogPertama;
        
        // Aktifkan panel utama
        panelDialog.SetActive(true);
        // Sembunyikan panel pilihan di awal
        if (choicePanel != null) choicePanel.SetActive(false);
        // Sembunyikan tombol exit di awal dialog
        if (tombolExit != null) tombolExit.SetActive(false);

        TampilkanBarisDialog();
    }

    // Fungsi pembantu untuk mencari data dialog berdasarkan ID uniknya di dalam List daftarDialog
    private DataDialog CariDialogBerdasarkanID(string id)
    {
        foreach (DataDialog data in daftarDialog)
        {
            if (data.dialogID == id)
            {
                return data;
            }
        }
        
        // Jika tidak ditemukan, kembalikan objek kosong/default
        return default(DataDialog);
    }

    // Menampilkan baris dialog yang sedang aktif
    private void TampilkanBarisDialog()
    {
        // Set nama karakter
        teksNama.text = dialogAktif.namaKarakter;

        // Reset panel pilihan ke kondisi normal sebelum mengetik
        if (choicePanel != null) choicePanel.SetActive(false);
        // Pastikan tombol exit disembunyikan saat sedang memuat kalimat baru
        if (tombolExit != null) tombolExit.SetActive(false);

        // Hentikan coroutine mengetik yang lama agar tidak tumpang dian
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
                teksKalimat.text = dialogAktif.kalimatDialog;
                
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
        coroutineMengetik = StartCoroutine(KetikKalimat(dialogAktif.kalimatDialog));
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
        
        // Setelah kalimat selesai diketik, cek apakah dialog ini membutuhkan pilihan bercabang?
        if (dialogAktif.punyaPilihan)
        {
            MunculkanPilihanCabang();
        }
        else if (ApakahDialogSelesai())
        {
            // Munculkan tombol exit secara otomatis saat seluruh percakapan selesai diketik
            MunculkanTombolExit();
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
            teksPilihan1.text = dialogAktif.pilihan1_Teks;
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
            teksPilihan2.text = dialogAktif.pilihan2_Teks;
        }

        // Aktifkan panel pilihan
        if (choicePanel != null) choicePanel.SetActive(true);
    }

    // Fungsi publik yang dipanggil ketika pemain mengklik tombol pilihan pertama atau kedua.
    // Menentukan ke ID dialog mana alur cerita akan melompat berikutnya.
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

        // Cari dialog berikutnya berdasarkan ID tujuan
        DataDialog dialogBerikutnya = CariDialogBerdasarkanID(nextID);

        if (!string.IsNullOrEmpty(dialogBerikutnya.dialogID))
        {
            dialogAktif = dialogBerikutnya;
            TampilkanBarisDialog();
        }
        else
        {
            // Jika ID terdaftar tapi datanya tidak ada di list, tutup dialog demi keamanan game
            Debug.LogWarning("Dialog dengan ID '" + nextID + "' tidak ditemukan! Dialog ditutup otomatis.");
            SelesaiDialog();
        }
    }

    // Fungsi pembantu tanpa parameter untuk dihubungkan ke tombol Pilihan 1 di Unity UI onClick
    public void PilihOpsi1()
    {
        // Berikan efek moralitas ke NPC jika target NPC dipasang di inspector
        if (npcTarget != null && dialogAktif.pilihan1_EfekMoralitas != 0)
        {
            npcTarget.UbahMoralitas(dialogAktif.pilihan1_EfekMoralitas);
        }
        
        PilihCabang(dialogAktif.pilihan1_NextID);
    }

    // Fungsi pembantu tanpa parameter untuk dihubungkan ke tombol Pilihan 2 di Unity UI onClick
    public void PilihOpsi2()
    {
        // Berikan efek moralitas ke NPC jika target NPC dipasang di inspector
        if (npcTarget != null && dialogAktif.pilihan2_EfekMoralitas != 0)
        {
            npcTarget.UbahMoralitas(dialogAktif.pilihan2_EfekMoralitas);
        }
        
        PilihCabang(dialogAktif.pilihan2_NextID);
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
            
            teksKalimat.text = dialogAktif.kalimatDialog;
            isTyping = false;

            // Karena skip, kita cek juga apakah dialog ini memicu pilihan bercabang?
            if (dialogAktif.punyaPilihan)
            {
                MunculkanPilihanCabang();
            }
            else if (ApakahDialogSelesai())
            {
                // Jika diskip dan ternyata dialog selesai, langsung munculkan tombol exit
                MunculkanTombolExit();
            }
        }
        // KONDISI 2: Jika teks sudah selesai diketik dan tidak ada pilihan bercabang
        else
        {
            // Cek ke mana arah dialog berikutnya
            string nextID = dialogAktif.nextDialogID;

            // Jika tidak ada ID tujuan berikutnya atau diisi "SELESAI", akhiri percakapan
            if (string.IsNullOrEmpty(nextID) || nextID == "SELESAI")
            {
                // Jika tombol exit dipasang di Inspector, kita tidak menutup otomatis lewat klik kiri sembarang
                // melainkan membiarkan pemain mengklik tombol Exit secara manual.
                if (tombolExit != null)
                {
                    return;
                }

                // Jika tombol exit tidak dipasang (opsional), klik kiri sembarang tetap menutup dialog
                SelesaiDialog();
                return;
            }

            // Muat dialog berikutnya berdasarkan ID
            DataDialog dialogBerikutnya = CariDialogBerdasarkanID(nextID);

            if (!string.IsNullOrEmpty(dialogBerikutnya.dialogID))
            {
                dialogAktif = dialogBerikutnya;
                TampilkanBarisDialog();
            }
            else
            {
                // Jika ID tujuan diisi tapi datanya tidak ditemukan di daftarDialog
                Debug.LogWarning("Dialog dengan ID '" + nextID + "' tidak ditemukan! Dialog ditutup.");
                SelesaiDialog();
            }
        }
    }

    // Fungsi pembantu untuk memeriksa apakah seluruh dialog saat ini sudah selesai secara alami
    public bool ApakahDialogSelesai()
    {
        // Dialog dianggap selesai jika:
        // 1. Teks tidak sedang dalam proses mengetik (typewriter selesai)
        // 2. ID dialog berikutnya kosong atau berisi "SELESAI"
        // 3. Dialog ini tidak memiliki pilihan bercabang yang menggantung
        bool tidakSedangMengetik = !isTyping;
        bool tidakAdaDialogBerikutnya = string.IsNullOrEmpty(dialogAktif.nextDialogID) || dialogAktif.nextDialogID == "SELESAI";
        bool tidakAdaPilihan = !dialogAktif.punyaPilihan;

        return tidakSedangMengetik && tidakAdaDialogBerikutnya && tidakAdaPilihan;
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
            if (panelDialog != null) panelDialog.SetActive(false);
            if (choicePanel != null) choicePanel.SetActive(false);
            // Sembunyikan kembali tombol exit saat panel dialog ditutup
            if (tombolExit != null) tombolExit.SetActive(false);
        }
        else
        {
            Debug.Log("Pemain mencoba keluar, tetapi dialog saat ini belum selesai diketik atau percakapan belum selesai.");
        }
    }
}
