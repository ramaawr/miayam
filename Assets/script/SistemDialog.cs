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

    // Tombol Next (lanjut) linier. Nanti disembunyikan saat pilihan muncul.
    [SerializeField] private GameObject tombolNext;

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
            // Pastikan kita hanya memproses input jika panel dialog sedang aktif dan panel pilihan TIDAK sedang aktif.
            // (Kalau sedang memilih jawaban, tombol Next dan input keyboard Spasi kita kunci agar pemain fokus memilih).
            if (panelDialog != null && panelDialog.activeSelf && (choicePanel == null || !choicePanel.activeSelf))
            {
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
        // Pastikan tombol next aktif di awal
        if (tombolNext != null) tombolNext.SetActive(true);

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

        // Reset panel pilihan dan tombol next ke kondisi normal sebelum mengetik
        if (choicePanel != null) choicePanel.SetActive(false);
        if (tombolNext != null) tombolNext.SetActive(true);

        // Hentikan coroutine mengetik yang lama agar tidak tumpang dian
        if (coroutineMengetik != null)
        {
            StopCoroutine(coroutineMengetik);
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
    }

    // Fungsi untuk memicu munculnya tombol pilihan jawaban di layar
    private void MunculkanPilihanCabang()
    {
        // Sembunyikan tombol Next linier agar pemain harus mengklik salah satu pilihan
        if (tombolNext != null) tombolNext.SetActive(false);

        // Pasang teks pilihan ke tombol UI
        if (teksPilihan1 != null) teksPilihan1.text = dialogAktif.pilihan1_Teks;
        if (teksPilihan2 != null) teksPilihan2.text = dialogAktif.pilihan2_Teks;

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
        }
        // KONDISI 2: Jika teks sudah selesai diketik dan tidak ada pilihan bercabang
        else
        {
            // Cek ke mana arah dialog berikutnya
            string nextID = dialogAktif.nextDialogID;

            // Jika tidak ada ID tujuan berikutnya atau diisi "SELESAI", akhiri percakapan
            if (string.IsNullOrEmpty(nextID) || nextID == "SELESAI")
            {
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

    // Fungsi untuk menutup dialog dan menyembunyikannya dari layar
    public void SelesaiDialog()
    {
        if (panelDialog != null) panelDialog.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);
    }
}
