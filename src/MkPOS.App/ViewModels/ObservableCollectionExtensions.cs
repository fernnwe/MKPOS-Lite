using System.Collections.ObjectModel;

namespace MKPOS.App.ViewModels;

public static class ObservableCollectionExtensions
{
    /// <summary>Sustituye el contenido de la colección manteniendo la instancia enlazada.</summary>
    public static void Replace<T>(this ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source)
        {
            target.Add(item);
        }
    }
}