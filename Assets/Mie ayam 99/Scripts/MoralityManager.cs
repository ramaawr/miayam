using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System;

// =============================================================================
// MoralityManager.cs — Pengelola Sistem Moralitas
// =============================================================================
// Script Singleton ini mengatur dialog moralitas, menyimpan data poin moralitas
// menggunakan PlayerPrefs, serta memperbarui antarmuka UI (Morality Bar).
// =============================================================================

public class MoralityManager : MonoBehaviour
{
    public static MoralityManager instance;

    [Header("Pengaturan Nilai Moralitas")]
    [Tooltip("Nilai maksimal poin moralitas (misalnya 100)")]
    public float NilaiMaksimalMoralitas = 100f;
    
    [Tooltip("Nilai awal moralitas jika pemain baru pertama kali main")]
    public float NilaiAwalMoralitas = 30f;

    [Header("Referensi UI Panel Dialog")]
    [Tooltip("Panel UI utama untuk dialog moralitas")]
    public GameObject PanelDialogMoralitas;
    [Tooltip("Teks untuk menampilkan nama karakter")]
    public TextMeshProUGUI TeksNamaKarakter;
    [Tooltip("Teks untuk menampilkan isi percakapan")]
    public TextMeshProUGUI TeksIsiDialog;
    [Tooltip("Gambar untuk menampilkan wajah karakter (opsional)")]
    public Image GambarPortrait;
    [Tooltip("Wadah untuk memunculkan tombol-tombol pilihan")]
    public Transform WadahPilihan;
    [Tooltip("Prefab tombol (Button) untuk setiap pilihan jawaban")]
    public GameObject PrefabTombolPilihan;

    [Header("Referensi UI Morality Bar")]
    [Tooltip("Slider UI untuk menampilkan persentase moralitas (harus punya fill/background)")]
    public Slider SliderMoralitas;
    [Tooltip("Waktu animasi pengisian bar (detik)")]
    public float DurasiAnimasiBar = 1f;

    [Header("Referensi Latar (Background)")]
    [Tooltip("Daftar background yang bisa dipilih melalui index di dalam file Sequence")]
    public GameObject[] daftarBackground;

    // Menyimpan aksi (callback) yang akan dijalankan setelah seluruh fase moralitas selesai
    private Action onSelesaiCallback;
    
    // Menyimpan sequence dialog yang sedang berjalan
    private MoralityDialogueSequence sequenceAktif;
    
    // Menyimpan indeks baris dialog saat ini
    private int indeksNodeSaatIni = 0;
    
    // Menyimpan status apakah teks sedang dalam proses efek pengetikan (typing effect)
    private bool sedangMengetik = false;
    private Coroutine typingCoroutine;

    // Key untuk PlayerPrefs
    private const string KUNCI_SAVE_MORALITAS = "PoinMoralitasPemain";

    // Komponen untuk memainkan suara (di-generate otomatis)
    private AudioSource audioSource;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Menambahkan AudioSource ke dalam sistem ini agar bisa memutar efek suara dialog
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (PanelDialogMoralitas != null)
        {
            PanelDialogMoralitas.SetActive(false);
        }
        
        // Memuat poin moralitas dari penyimpanan saat game dimulai untuk update bar awal
        MuatPoinMoralitas();
        PerbaruiUIBar(false); // Perbarui tanpa animasi di awal

        // Mematikan semua background agar tidak muncul sejak awal game
        MatikanSemuaBackground();
    }
    
    #region SAVE / LOAD SYSTEM (PLAYER PREFS)
    
    // Fungsi ini dipanggil untuk membaca nilai poin moralitas terakhir yang disimpan
    private void MuatPoinMoralitas()
    {
        // Mengecek apakah sebelumnya sistem sudah pernah menyimpan data moralitas
        if (PlayerPrefs.HasKey(KUNCI_SAVE_MORALITAS))
        {
            // Jika ada, ambil angkanya dari PlayerPrefs dan jadikan nilai saat ini.
            float nilaiTersimpan = PlayerPrefs.GetFloat(KUNCI_SAVE_MORALITAS);
            Debug.Log("Memuat poin moralitas: " + nilaiTersimpan);
        }
        else
        {
            // Jika belum ada (berarti pemain baru pertama kali main), simpan nilai awal
            PlayerPrefs.SetFloat(KUNCI_SAVE_MORALITAS, NilaiAwalMoralitas);
            // Wajib memanggil Save() agar datanya langsung ditulis secara permanen ke perangkat
            PlayerPrefs.Save();
            Debug.Log("Pemain baru. Mengatur poin moralitas ke nilai awal: " + NilaiAwalMoralitas);
        }
    }

    // Fungsi ini dipanggil setiap kali poin moralitas berubah karena pilihan pemain
    private void TambahPoinMoralitas(float tambahanPoin)
    {
        // 1. Ambil nilai poin yang saat ini ada di penyimpanan
        float poinSekarang = PlayerPrefs.GetFloat(KUNCI_SAVE_MORALITAS, NilaiAwalMoralitas);
        
        // 2. Tambahkan (atau kurangkan jika nilainya negatif) poin baru tersebut
        float poinBaru = poinSekarang + tambahanPoin;
        
        // 3. Pastikan nilai tidak kurang dari 0 dan tidak lebih dari nilai maksimal (dibatasi/Clamp)
        poinBaru = Mathf.Clamp(poinBaru, 0f, NilaiMaksimalMoralitas);
        
        // 4. Timpa nilai lama dengan nilai baru di PlayerPrefs
        PlayerPrefs.SetFloat(KUNCI_SAVE_MORALITAS, poinBaru);
        
        // 5. Paksa simpan ke memori permanen agar tidak hilang jika game tiba-tiba keluar
        PlayerPrefs.Save();
        
        Debug.Log("Poin Moralitas berubah. Sekarang: " + poinBaru);
        
        // Setelah nilai diperbarui di penyimpanan, mainkan animasi pergerakan bar UI-nya
        PerbaruiUIBar(true);
    }
    
    // Fungsi bantuan untuk mengambil nilai moralitas saat ini (berguna jika script lain butuh membacanya)
    public float DapatkanPoinSaatIni()
    {
        return PlayerPrefs.GetFloat(KUNCI_SAVE_MORALITAS, NilaiAwalMoralitas);
    }
    
    #endregion

    #region SISTEM DIALOG
    // Fungsi utama yang dipanggil oleh LevelManager untuk memulai fase dialog moralitas
    public void MulaiDialogMoralitas(MoralityDialogueSequence sequence, Action callbackSetelahSelesai)
    {
        sequenceAktif = sequence;
        onSelesaiCallback = callbackSetelahSelesai;
        indeksNodeSaatIni = 0;

        // Atur background yang menyala sesuai indeks yang diminta sequence
        if (daftarBackground != null && daftarBackground.Length > 0)
        {
            for (int i = 0; i < daftarBackground.Length; i++)
            {
                if (daftarBackground[i] != null)
                {
                    daftarBackground[i].SetActive(i == sequence.indexBackgroundTerpilih);
                }
            }
        }

        // Cek apakah perlu efek fade in layar
        if (sequence.gunakanFadeInLayar)
        {
            StartCoroutine(ProsesFadeInDialogMoralitas());
        }

        if (PanelDialogMoralitas != null)
        {
            PanelDialogMoralitas.SetActive(true);
        }

        TampilkanNodeSekarang();
    }

    // Coroutine khusus untuk memudarkan layar dari gelap ke terang (Fade In)
    private IEnumerator ProsesFadeInDialogMoralitas()
    {
        // Memastikan LevelManager dan PanelFadeTransisi tersedia
        if (LevelManager.instance != null && LevelManager.instance.PanelFadeTransisi != null)
        {
            CanvasGroup fader = LevelManager.instance.PanelFadeTransisi;
            fader.gameObject.SetActive(true);
            fader.alpha = 1f; // Mulai dari layar gelap (penuh)
            
            float durasi = LevelManager.instance.DurasiFade;
            float waktuBerjalan = 0f;

            // Fading dari alpha 1 turun perlahan ke 0
            while (waktuBerjalan < durasi)
            {
                // Harus menggunakan unscaledDeltaTime karena bisa jadi game sedang di-pause
                waktuBerjalan += Time.unscaledDeltaTime; 
                fader.alpha = 1f - (waktuBerjalan / durasi);
                yield return null; 
            }

            fader.alpha = 0f;
            fader.gameObject.SetActive(false);
        }
    }

    private void TampilkanNodeSekarang()
    {
        BersihkanPilihan();

        // Cek jika sudah mencapai akhir list dialog atau disuruh berhenti (indeks negatif)
        if (indeksNodeSaatIni >= sequenceAktif.dialogueNodes.Count || indeksNodeSaatIni < 0)
        {
            SelesaiDialog();
            return;
        }

        MoralityDialogueNode node = sequenceAktif.dialogueNodes[indeksNodeSaatIni];

        if (TeksNamaKarakter != null) TeksNamaKarakter.text = node.speakerName;
        
        if (GambarPortrait != null)
        {
            if (node.portraitSprite != null)
            {
                GambarPortrait.sprite = node.portraitSprite;
                GambarPortrait.gameObject.SetActive(true);
                // (Opsi) Bisa menambahkan logika untuk membalik (flip) gambar berdasarkan node.portraitSide
            }
            else
            {
                GambarPortrait.gameObject.SetActive(false);
            }
        }

        // Hentikan efek mengetik sebelumnya jika ada
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        
        // Mulai efek mengetik
        typingCoroutine = StartCoroutine(EfekMengetik(node.dialogueText, node.typingSpeed));

        // Memainkan audio jika ada (kita gunakan AudioSource bawaan sistem ini)
        if (node.audioClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(node.audioClip);
        }
    }

    private IEnumerator EfekMengetik(string kalimat, float jeda)
    {
        sedangMengetik = true;
        TeksIsiDialog.text = "";
        
        foreach (char huruf in kalimat.ToCharArray())
        {
            TeksIsiDialog.text += huruf;
            yield return new WaitForSeconds(jeda);
        }
        
        sedangMengetik = false;
        TampilkanTombolPilihan();
    }

    private void TampilkanTombolPilihan()
    {
        MoralityDialogueNode node = sequenceAktif.dialogueNodes[indeksNodeSaatIni];

        // Jika tidak ada pilihan bercabang, munculkan satu tombol generik untuk lanjut
        if (node.choices == null || node.choices.Count == 0)
        {
            BuatTombol("Lanjut...", () => {
                LanjutKeNode(node.jumpToNodeIndex != -1 ? node.jumpToNodeIndex : indeksNodeSaatIni + 1);
            });
        }
        else
        {
            // Jika ada pilihan, buat tombol untuk masing-masing pilihan
            for (int i = 0; i < node.choices.Count; i++)
            {
                MoralityDialogueChoice pilihan = node.choices[i];
                BuatTombol(pilihan.choiceText, () => {
                    MemilihJawaban(pilihan);
                });
            }
        }
    }

    private void BuatTombol(string teks, UnityEngine.Events.UnityAction aksi)
    {
        if (PrefabTombolPilihan == null || WadahPilihan == null) return;

        GameObject tombolBaru = Instantiate(PrefabTombolPilihan, WadahPilihan);
        
        // Mengubah teks di dalam tombol
        TextMeshProUGUI teksTombol = tombolBaru.GetComponentInChildren<TextMeshProUGUI>();
        if (teksTombol != null) teksTombol.text = teks;

        // Menambahkan fungsi saat diklik
        Button btn = tombolBaru.GetComponent<Button>();
        if (btn != null) btn.onClick.AddListener(aksi);
    }

    private void BersihkanPilihan()
    {
        if (WadahPilihan == null) return;
        
        foreach (Transform anak in WadahPilihan)
        {
            Destroy(anak.gameObject);
        }
    }

    private void MemilihJawaban(MoralityDialogueChoice pilihanTerpilih)
    {
        // Langsung sembunyikan tombol agar tidak bisa diklik berulang kali selama jeda
        BersihkanPilihan(); 

        // Gunakan coroutine untuk memberi jeda animasi bar
        StartCoroutine(ProsesPilihanDanJeda(pilihanTerpilih));
    }

    private IEnumerator ProsesPilihanDanJeda(MoralityDialogueChoice pilihanTerpilih)
    {
        // 1. Eksekusi perubahan poin moralitas
        if (pilihanTerpilih.moralityPointChange != 0)
        {
            TambahPoinMoralitas(pilihanTerpilih.moralityPointChange);
            
            // Tunggu selama animasi bar berjalan + jeda sedikit agar perubahan terasa
            yield return new WaitForSeconds(DurasiAnimasiBar + 0.5f);
        }
        else
        {
            // Jeda singkat meski tidak ada perubahan poin
            yield return new WaitForSeconds(0.5f);
        }

        // 2. Lanjut ke baris selanjutnya sesuai indeks target
        LanjutKeNode(pilihanTerpilih.targetNodeIndex);
    }

    private void LanjutKeNode(int indeksTarget)
    {
        indeksNodeSaatIni = indeksTarget;
        TampilkanNodeSekarang();
    }

    // Dipanggil oleh tombol layar jika pemain tidak sabar melihat efek ketik teks
    public void KlikLayarBuruBuru()
    {
        if (sedangMengetik)
        {
            // Langsung hentikan ketikan dan tampilkan seluruh teks
            StopCoroutine(typingCoroutine);
            TeksIsiDialog.text = sequenceAktif.dialogueNodes[indeksNodeSaatIni].dialogueText;
            sedangMengetik = false;
            TampilkanTombolPilihan();
        }
    }

    private void SelesaiDialog()
    {
        if (PanelDialogMoralitas != null)
        {
            PanelDialogMoralitas.SetActive(false);
        }

        // Matikan kembali background agar tidak menutupi gameplay
        MatikanSemuaBackground();

        // Memanggil callback agar LevelManager bisa memunculkan Popup Result setelah dialog selesai
        if (onSelesaiCallback != null)
        {
            // Menunggu sebentar (opsional) agar pemain bisa melihat perubahan bar yang baru saja terjadi
            StartCoroutine(JedaSelesaiDialog());
        }
    }

    private IEnumerator JedaSelesaiDialog()
    {
        // Memberi waktu 1.5 detik agar pemain sadar ada efek pada Moralitas Bar-nya
        yield return new WaitForSeconds(1.5f);
        
        onSelesaiCallback.Invoke();
        onSelesaiCallback = null;
    }

    // Fungsi utilitas untuk menyembunyikan semua background di daftar
    private void MatikanSemuaBackground()
    {
        if (daftarBackground != null)
        {
            foreach (GameObject bg in daftarBackground)
            {
                if (bg != null) bg.SetActive(false);
            }
        }
    }
    #endregion

    #region MORALITY BAR UI
    // Memperbarui UI visual persentase menggunakan pecahan eksak
    private void PerbaruiUIBar(bool denganAnimasi)
    {
        if (SliderMoralitas == null) return;

        float nilaiSaatIni = DapatkanPoinSaatIni();
        
        // PENTING: Perhitungan matematis eksak (pecahan)
        // Kita langsung membagi nilai saat ini dengan nilai maksimal (misal 50 / 100 = 0.5f)
        float targetPersentase = nilaiSaatIni / NilaiMaksimalMoralitas;

        if (denganAnimasi)
        {
            StartCoroutine(AnimasiBar(targetPersentase));
        }
        else
        {
            SliderMoralitas.value = targetPersentase;
        }
    }

    private IEnumerator AnimasiBar(float targetValue)
    {
        float nilaiAwal = SliderMoralitas.value;
        float waktuBerjalan = 0f;

        while (waktuBerjalan < DurasiAnimasiBar)
        {
            waktuBerjalan += Time.deltaTime;
            // Lerp untuk menghaluskan perpindahan nilai dari awal ke akhir
            float fraksi = waktuBerjalan / DurasiAnimasiBar;
            SliderMoralitas.value = Mathf.Lerp(nilaiAwal, targetValue, fraksi);
            yield return null; // Tunggu satu frame
        }

        // Memastikan akurasi paripurna di akhir animasi
        SliderMoralitas.value = targetValue;
    }
    #endregion
}
