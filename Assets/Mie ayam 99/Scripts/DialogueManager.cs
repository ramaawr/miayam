using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// =============================================================================
// DialogueManager.cs — Pengontrol Utama Sistem Dialog Terpusat
// =============================================================================
// Script ini mengontrol alur jalannya dialog di layar (typewriter, portrait,
// audio, branching choice, dan pemblokiran klik game di latar belakang).
// =============================================================================

public class DialogueManager : MonoBehaviour
{
    #region SINGLETON
    // Akses cepat global agar bisa dipanggil dari level manager atau NPC manapun
    public static DialogueManager instance;
    #endregion

    #region VARIABLES (REFERENSI UI & AUDIO)
    [Header("Referensi UI Canvas")]
    [Tooltip("Panel utama yang menampung seluruh tampilan dialog")]
    // Panel yang akan dinyalakan/dimatikan saat dialog aktif/selesai
    public GameObject panelDialog;

    [Tooltip("Komponen teks untuk menampilkan nama pembicara")]
    public TextMeshProUGUI teksNama;

    [Tooltip("Komponen teks untuk menampilkan isi percakapan")]
    public TextMeshProUGUI teksDialog;

    [Tooltip("Gambar portrait karakter di sebelah kiri")]
    public Image visualPortraitKiri;

    [Tooltip("Gambar portrait karakter di sebelah kanan")]
    public Image visualPortraitKanan;

    [Tooltip("Tombol Next untuk melanjutkan ke baris berikutnya")]
    public Button tombolNext;

    [Header("Referensi Pilihan Cabang (Branching)")]
    [Tooltip("Panel penampung tombol pilihan percakapan")]
    // Tempat menaruh tombol pilihan respon pemain secara dinamis
    public Transform panelPilihanParent;

    [Tooltip("Prefab tombol untuk pilihan percakapan")]
    // Prefab tombol UI yang memiliki komponen Button dan TextMeshProUGUI
    public GameObject prefabTombolPilihan;

    [Header("Referensi Suara (SFX)")]
    [Tooltip("AudioSource untuk memutar klip suara dialog (opsional)")]
    public AudioSource audioSource;
    #endregion

    #region VARIABLES (ALUR INTERNAL)
    // Menyimpan rangkaian dialog yang sedang berjalan saat ini
    private DialogueSequence urutanDialogSekarang;
    
    // Indeks baris dialog yang sedang aktif saat ini (dimulai dari 0)
    private int indexNodeSekarang;
    
    // Fungsi callback yang akan dijalankan setelah percakapan selesai seluruhnya
    private System.Action callbackSelesaiDialog;
    
    // Status apakah teks sedang berjalan mengetik karakter demi karakter
    private bool sedangMengetik = false;
    
    // Menyimpan referensi Coroutine mengetik agar bisa dihentikan saat di-skip
    private Coroutine coroutineKetik;
    #endregion

    #region INITIALIZATION
    void Awake()
    {
        // Setup Singleton tunggal
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Menyembunyikan panel dialog saat game pertama kali dimulai
        if (panelDialog != null)
        {
            panelDialog.SetActive(false);
        }

        // Menyembunyikan panel pilihan di awal game
        if (panelPilihanParent != null)
        {
            panelPilihanParent.gameObject.SetActive(false);
        }
    }
    #endregion

    #region DIALOGUE FLOW
    // Fungsi utama untuk memicu jalannya percakapan dari script lain
    public void MulaiDialog(DialogueSequence sequence, System.Action callbackSelesai)
    {
        // Guard: Pastikan data sequence dialog tidak null
        if (sequence == null || sequence.dialogueNodes.Count == 0)
        {
            Debug.LogWarning("MulaiDialog: Sequence dialog kosong atau null!");
            callbackSelesai?.Invoke();
            return;
        }

        // Simpan data sequence dan callback-nya
        urutanDialogSekarang = sequence;
        callbackSelesaiDialog = callbackSelesai;
        indexNodeSekarang = 0;

        // 1. Matikan jalannya waktu game (pause timer masak & pergerakan NPC)
        Time.timeScale = 0f;

        // 2. Nyalakan panel UI Dialog
        if (panelDialog != null)
        {
            panelDialog.SetActive(true);
            if (panelDialog.TryGetComponent<JuiceAnimator>(out var juice)) juice.PlayPopInUI();
        }

        // 3. Tampilkan baris dialog pertama
        TampilkanNode(indexNodeSekarang);
    }

    // Fungsi untuk menampilkan baris dialog tertentu berdasarkan indeks
    private void TampilkanNode(int index)
    {
        // Guard: Jika indeks melebihi jumlah baris dialog, percakapan selesai
        if (index < 0 || index >= urutanDialogSekarang.dialogueNodes.Count)
        {
            SelesaiDialog();
            return;
        }

        DialogueNode node = urutanDialogSekarang.dialogueNodes[index];

        // 1. Update UI Portrait & Nama Pembicara
        UpdateUI(node);

        // 2. Putar Suara (SFX) jika ada
        if (audioSource != null && node.audioClip != null)
        {
            audioSource.PlayOneShot(node.audioClip);
        }

        // 3. Bersihkan sisa tombol pilihan sebelumnya
        BersihkanPilihanUI();

        // 4. Mulai efek typewriter mengetik teks
        if (coroutineKetik != null)
        {
            StopCoroutine(coroutineKetik);
        }
        coroutineKetik = StartCoroutine(KetikTeks(node.dialogueText, node.typingSpeed));
    }

    // Coroutine untuk memunculkan teks per karakter secara berurutan
    private IEnumerator KetikTeks(string teksLengkap, float speed)
    {
        sedangMengetik = true;
        teksDialog.text = "";

        // Menggunakan WaitForSecondsRealtime agar efek mengetik tetap berjalan 
        // meskipun game di-pause (Time.timeScale = 0)
        float jedaDetik = speed > 0f ? speed : 0.03f;

        foreach (char huruf in teksLengkap.ToCharArray())
        {
            teksDialog.text += huruf;
            yield return new WaitForSecondsRealtime(jedaDetik);
        }

        SelesaiMengetik();
    }

    // Dipanggil saat teks selesai diketik (baik secara alami maupun di-skip)
    private void SelesaiMengetik()
    {
        sedangMengetik = false;

        DialogueNode node = urutanDialogSekarang.dialogueNodes[indexNodeSekarang];

        // Jika baris dialog ini memiliki pilihan cabang (branching)
        if (node.choices != null && node.choices.Count > 0)
        {
            // Sembunyikan tombol Next agar pemain harus memilih salah satu cabang
            if (tombolNext != null)
            {
                tombolNext.gameObject.SetActive(false);
            }
            TampilkanPilihanUI(node);
        }
        else
        {
            // Tampilkan tombol Next untuk lanjut percakapan lurus
            if (tombolNext != null)
            {
                tombolNext.gameObject.SetActive(true);
            }
        }
    }

    // Dipanggil saat pemain mengklik area tombol Next
    public void TombolNextDipencet()
    {
        // Kasus 1: Teks masih berjalan mengetik -> skip agar langsung muncul penuh
        if (sedangMengetik)
        {
            if (coroutineKetik != null)
            {
                StopCoroutine(coroutineKetik);
            }
            teksDialog.text = urutanDialogSekarang.dialogueNodes[indexNodeSekarang].dialogueText;
            SelesaiMengetik();
        }
        // Kasus 2: Teks sudah tampil penuh -> lanjut ke baris berikutnya
        else
        {
            DialogueNode node = urutanDialogSekarang.dialogueNodes[indexNodeSekarang];
            // Memeriksa apakah ada instruksi lompatan indeks (jumpToNodeIndex)
            if (node.jumpToNodeIndex != -1)
            {
                indexNodeSekarang = node.jumpToNodeIndex;
            }
            else
            {
                indexNodeSekarang++;
            }
            TampilkanNode(indexNodeSekarang);
        }
    }

    // Dipanggil saat tombol pilihan diklik
    public void PilihCabang(int targetIndex)
    {
        // Lompati indeks percakapan ke node tujuan
        indexNodeSekarang = targetIndex;
        TampilkanNode(indexNodeSekarang);
    }

    // Menutup layar percakapan dan mengembalikan gameplay normal
    private void SelesaiDialog()
    {
        // 1. Sembunyikan panel UI Dialog
        if (panelDialog != null)
        {
            panelDialog.SetActive(false);
        }

        // 2. Kembalikan jalannya waktu game (resume timer masak & NPC)
        Time.timeScale = 1f;

        // 3. Jalankan callback selesai
        // Menggunakan variabel sementara (temp) karena fungsi MulaiDialog berpotensi
        // dipanggil lagi di dalam Invoke(), yang akan menimpa callbackSelesaiDialog.
        System.Action tempCallback = callbackSelesaiDialog;
        callbackSelesaiDialog = null;
        tempCallback?.Invoke();
    }
    #endregion

    #region UI HANDLING
    // Mengatur visual portrait, nama pembicara, dan layout dialog
    private void UpdateUI(DialogueNode node)
    {
        // Update Nama Pembicara
        if (teksNama != null)
        {
            teksNama.text = node.speakerName;
        }

        // Sembunyikan kedua portrait terlebih dahulu
        if (visualPortraitKiri != null) visualPortraitKiri.gameObject.SetActive(false);
        if (visualPortraitKanan != null) visualPortraitKanan.gameObject.SetActive(false);

        // Jika node memiliki sprite portrait, tampilkan di sisi yang sesuai
        if (node.portraitSprite != null)
        {
            if (node.portraitSide == PortraitSide.Left)
            {
                if (visualPortraitKiri != null)
                {
                    visualPortraitKiri.gameObject.SetActive(true);
                    visualPortraitKiri.sprite = node.portraitSprite;
                    if (visualPortraitKiri.TryGetComponent<JuiceAnimator>(out var juice)) juice.StartPortraitBreathing();
                }
            }
            else
            {
                if (visualPortraitKanan != null)
                {
                    visualPortraitKanan.gameObject.SetActive(true);
                    visualPortraitKanan.sprite = node.portraitSprite;
                    if (visualPortraitKanan.TryGetComponent<JuiceAnimator>(out var juice)) juice.StartPortraitBreathing();
                }
            }
        }
    }

    // Menampilkan tombol-tombol pilihan cabang percakapan di layar
    private void TampilkanPilihanUI(DialogueNode node)
    {
        if (panelPilihanParent == null || prefabTombolPilihan == null) return;

        panelPilihanParent.gameObject.SetActive(true);

        // Instansiasi tombol pilihan baru secara dinamis
        foreach (DialogueChoice pilihan in node.choices)
        {
            GameObject tombolObj = Instantiate(prefabTombolPilihan, panelPilihanParent);
            
            // Set teks tombol pilihan
            TextMeshProUGUI teksTombol = tombolObj.GetComponentInChildren<TextMeshProUGUI>();
            if (teksTombol != null)
            {
                teksTombol.text = pilihan.choiceText;
            }

            // Daftarkan event klik tombol ke fungsi PilihCabang
            Button tombol = tombolObj.GetComponent<Button>();
            if (tombol != null)
            {
                // Menghindari masalah variabel closure C# dengan membuat salinan lokal
                int indexTujuan = pilihan.targetNodeIndex;
                tombol.onClick.AddListener(() => PilihCabang(indexTujuan));
            }
        }
    }

    // Menghapus seluruh tombol pilihan percakapan dari layar
    private void BersihkanPilihanUI()
    {
        if (panelPilihanParent == null) return;

        panelPilihanParent.gameObject.SetActive(false);

        // Hancurkan semua anak (child) GameObject tombol di dalam panel pilihan
        foreach (Transform anak in panelPilihanParent)
        {
            Destroy(anak.gameObject);
        }
    }
    #endregion
}
