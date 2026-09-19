namespace MKPOS.App.ViewModels;

/// <summary>
/// Contratos para los ViewModels que se muestran en el área de contenido
/// principal del <c>MainWindow</c>.
/// </summary>
public interface IModuleViewModel
{
    Task LoadAsync();
}