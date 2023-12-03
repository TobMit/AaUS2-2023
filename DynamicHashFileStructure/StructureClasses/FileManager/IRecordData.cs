namespace DynamicHashFileStructure.StructureClasses;

public interface IRecordData<T> : IRecord, IComparable<T>
{
    /// <summary>
    /// Hešovacia funckia pre záznam
    /// </summary>
    /// <returns>Vytvorí pole bytov</returns>
    public static abstract byte[] GetBytesForHash(T key);

    /// <summary>
    /// Tento kľúč sa používa na porovnávanie záznamov, preto musí byť unikátny a rovnaký ako ten ktorý sa vkladá pri inserte
    /// </summary>
    /// <returns>kľuč týchto súborov</returns>
    public abstract T GetKey();
}