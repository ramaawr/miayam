using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Data Level Baru", menuName = "Mie Ayam/Data Level")]
public class DataLevel : ScriptableObject
{
    #region PENGATURAN LEVEL
    [Tooltip("Nomor level untuk ditampilkan di UI (misal: 1, 2, 3)")]
    // Menyimpan nomor level ini agar bisa ditampilkan pada antarmuka (UI) permainan
    public int NomorLevel = 1;
    #endregion

    #region DATA ANTREAN NPC
    [Tooltip("Daftar NPC yang akan muncul berurutan di level ini")]
    // Menyimpan urutan antrean pelanggan NPC yang akan datang ke gerobak pada level ini
    public List<ProfilNPC> AntreanNPC = new List<ProfilNPC>();
    #endregion

    #region DATA DIALOG LEVEL
    [Header("Dialog Cerita Level")]
    [Tooltip("Percakapan awal level sebelum NPC pertama muncul (opsional)")]
    // Menyimpan dialog pembuka hari/level ini
    public DialogueSequence DialogAwalLevel;

    [Tooltip("Percakapan penutup level setelah semua NPC dilayani (opsional)")]
    // Menyimpan dialog rekap/akhir hari sebelum popup skor muncul
    public DialogueSequence DialogAkhirLevel;

    [Header("Dialog Moralitas (Fase Baru)")]
    [Tooltip("Percakapan dengan anak yang mempengaruhi moralitas sebelum rekap harian")]
    public MoralityDialogueSequence DialogMoralitasAkhirLevel;
    #endregion
}
