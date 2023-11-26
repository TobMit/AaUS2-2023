namespace DynamicHashFileStructure.StructureClasses;

public interface IRecordData<T> : IRecord<T>, IComparable<T>
{
    /// <summary>
    /// Hešovacia funckia pre záznam
    /// </summary>
    /// <returns>Vytvorí pole bytov</returns>
    public byte[] GetBytesForHash();
}