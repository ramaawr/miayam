using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// DialogueSequence.cs — Struktur Data Percakapan Terpusat
// =============================================================================
// File ini menyimpan kelas data untuk menyusun dialog di Unity.
// Menggunakan ScriptableObject agar desainer game bisa membuat dan mengedit dialog
// langsung melalui Unity Inspector tanpa menyentuh baris kode.
// =============================================================================

#region ENUM PORTRAIT SIDE
// Menentukan di sebelah mana gambar karakter (portrait) akan muncul di layar
public enum PortraitSide
{
    Left,  // Sebelah kiri layar
    Right  // Sebelah kanan layar
}
#endregion

#region CLASS DIALOGUE CHOICE (BRANCHING)
[System.Serializable]
public class DialogueChoice
{
    [Tooltip("Teks yang akan muncul di tombol pilihan")]
    // Teks pilihan jawaban (misal: "Saya mau yang super pedas!")
    public string choiceText;

    [Tooltip("Indeks baris dialog tujuan saat pilihan ini diklik (dimulai dari 0)")]
    // Lompat ke baris dialog dengan indeks ini di dalam sequence yang sama jika dipilih
    public int targetNodeIndex;
}
#endregion

#region CLASS DIALOGUE NODE
[System.Serializable]
public class DialogueNode
{
    [Header("Informasi Pembicara")]
    [Tooltip("Nama karakter yang sedang berbicara")]
    // Nama yang akan ditampilkan di kotak nama dialog UI
    public string speakerName;

    [Header("Konten Dialog")]
    [Tooltip("Teks percakapan yang akan diucapkan")]
    [TextArea(3, 5)] // Membuat area input teks lebih luas di Inspector
    public string dialogueText;

    [Header("Visual Portrait")]
    [Tooltip("Gambar ekspresi wajah karakter (opsional)")]
    // Sprite gambar portrait karakter
    public Sprite portraitSprite;

    [Tooltip("Sisi kemunculan wajah karakter di layar")]
    // Menentukan wajah digambar di kiri atau kanan panel dialog
    public PortraitSide portraitSide = PortraitSide.Left;

    [Header("Audio & Efek")]
    [Tooltip("Kecepatan efek mengetik (detik per karakter). Default: 0.03")]
    // Durasi jeda tiap karakter muncul di layar. Semakin kecil semakin cepat.
    public float typingSpeed = 0.03f;

    [Tooltip("Suara pengisi suara atau efek sfx saat baris ini dibaca (opsional)")]
    // Klip suara yang akan dimainkan begitu baris dialog ini muncul
    public AudioClip audioClip;

    [Tooltip("Lompat langsung ke indeks baris ini setelah menekan Next (setel ke -1 jika ingin lurus ke baris berikutnya)")]
    // Berguna untuk melewati baris dialog lain (misal setelah memilih respon A, lompat melewati respon B ke akhir cerita)
    public int jumpToNodeIndex = -1;

    [Header("Cabang Pilihan (Branching)")]
    [Tooltip("Daftar pilihan respon pemain (biarkan kosong jika ingin berlanjut lurus ke Next)")]
    // Jika diisi, pemain wajib memilih salah satu untuk melanjutkan percakapan
    public List<DialogueChoice> choices = new List<DialogueChoice>();
}
#endregion

#region SCRIPTABLE OBJECT DIALOGUE SEQUENCE
[CreateAssetMenu(fileName = "Dialogue Sequence Baru", menuName = "Mie Ayam/Dialogue Sequence")]
public class DialogueSequence : ScriptableObject
{
    [Header("Pengaturan Identifikasi")]
    [Tooltip("ID unik untuk percakapan ini (berguna untuk melacak state cerita)")]
    // Penanda unik untuk sequence dialog ini
    public string sequenceID;

    [Header("Pengaturan Visual & Latar")]
    [Tooltip("Jika dicentang, layar akan memudar dari gelap ke terang di awal dialog ini")]
    // Opsi untuk memicu efek fade in (layar perlahan terang) dari LevelManager
    public bool gunakanFadeInLayar = false;

    [Tooltip("Indeks background yang ingin ditampilkan (-1 jika tidak ingin merubah background atau biarkan kosong)")]
    // Memilih background mana yang menyala berdasarkan indeks daftar di DialogueManager
    public int indexBackgroundTerpilih = -1;

    [Header("Daftar Baris Dialog")]
    [Tooltip("Rentetan baris percakapan yang akan dimainkan berurutan")]
    // Menyimpan daftar seluruh baris percakapan dari awal hingga akhir
    public List<DialogueNode> dialogueNodes = new List<DialogueNode>();
}
#endregion
