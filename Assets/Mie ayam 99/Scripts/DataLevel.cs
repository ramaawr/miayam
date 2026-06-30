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
}
