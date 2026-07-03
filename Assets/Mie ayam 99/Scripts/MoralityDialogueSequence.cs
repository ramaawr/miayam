using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// MoralityDialogueSequence.cs — Struktur Data Percakapan Moralitas
// =============================================================================
// File ini menyimpan kelas data untuk menyusun dialog dengan anak.
// Dialog ini memiliki pilihan (choices) yang dapat menambah atau mengurangi
// nilai poin moralitas pemain.
// =============================================================================

#region CLASS MORALITY DIALOGUE CHOICE
[System.Serializable]
public class MoralityDialogueChoice
{
    [Tooltip("Teks yang akan muncul di tombol pilihan")]
    // Teks pilihan jawaban (misal: "Iya nak, bantu bapak.")
    public string choiceText;

    [Tooltip("Poin yang ditambahkan/dikurangkan dari total moralitas jika memilih ini (misal: 10 atau -5)")]
    // Nilai moralitas yang didapatkan pemain jika menekan opsi ini
    public float moralityPointChange;

    [Tooltip("Indeks baris dialog tujuan saat pilihan ini diklik (dimulai dari 0)")]
    // Lompat ke baris dialog dengan indeks ini di dalam sequence yang sama jika dipilih
    public int targetNodeIndex;
}
#endregion

#region CLASS MORALITY DIALOGUE NODE
[System.Serializable]
public class MoralityDialogueNode
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
    public float typingSpeed = 0.03f;

    [Tooltip("Suara pengisi suara atau efek sfx saat baris ini dibaca (opsional)")]
    public AudioClip audioClip;

    [Tooltip("Lompat langsung ke indeks baris ini setelah menekan Next (setel ke -1 jika ingin lurus ke baris berikutnya)")]
    // Berguna untuk melompat jika tidak ada pilihan (misalnya percakapan standar)
    public int jumpToNodeIndex = -1;

    [Header("Cabang Pilihan (Branching) Moralitas")]
    [Tooltip("Daftar pilihan respon pemain yang memiliki dampak poin moralitas")]
    // Jika diisi, pemain wajib memilih salah satu untuk melanjutkan percakapan
    public List<MoralityDialogueChoice> choices = new List<MoralityDialogueChoice>();
}
#endregion

#region SCRIPTABLE OBJECT MORALITY DIALOGUE SEQUENCE
[CreateAssetMenu(fileName = "Morality Dialogue Baru", menuName = "Mie Ayam/Morality Dialogue")]
public class MoralityDialogueSequence : ScriptableObject
{
    [Header("Pengaturan Identifikasi")]
    [Tooltip("ID unik untuk percakapan ini")]
    public string sequenceID;

    [Header("Daftar Baris Dialog")]
    [Tooltip("Rentetan baris percakapan yang akan dimainkan berurutan")]
    public List<MoralityDialogueNode> dialogueNodes = new List<MoralityDialogueNode>();
}
#endregion
