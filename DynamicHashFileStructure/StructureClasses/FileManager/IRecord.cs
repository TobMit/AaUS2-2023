using System.ComponentModel;

namespace DynamicHashFileStructure.StructureClasses;

public interface IRecord : IComparable
{
    /// <summary>
    /// Veľkosť záznamu v bajtoch
    /// </summary>
    /// <returns> veľkosť záznamu v bajtoch</returns>
    public static abstract int GetSize();
    
    /// <summary>
    /// Vráti zoznam bajtov daného objektu, ktoré sa budú zapisovať na disk
    /// </summary>
    /// <returns>Array bitov</returns>
    public byte[] GetBytes();
    
    /// <summary>
    /// Vytvorí napäť triedu z poľa bitov ktorá je na disku
    /// </summary>
    /// <param name="bytes"> pole bitov ktoré sa načítali zo súboru</param>
    /// <returns> vytvorený objekt</returns>
    public static abstract object FromBytes(byte[] bytes);
    
    public string ToString();
    
}