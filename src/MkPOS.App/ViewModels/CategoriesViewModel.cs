using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MKPOS.Application.DTOs;
using MKPOS.Application.Services;
using System.Collections.ObjectModel;

namespace MKPOS.App.ViewModels;

public partial class CategoriesViewModel : ObservableObject, IModuleViewModel
{
    private readonly ICategoryService _categories;

    private Guid? _editingId;

    public ObservableCollection<CategoryDto> Categories { get; } = new();

    [ObservableProperty]
    private bool _includeInactive;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private CategoryDto? _selectedRow;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _editorTitle = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private bool _editorIsActive = true;

    public CategoriesViewModel(ICategoryService categories)
    {
        _categories = categories;
    }

    public async Task LoadAsync()
    {
        await RefreshAsync();
    }

    partial void OnIncludeInactiveChanged(bool value)
    {
        _ = RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        IsBusy = true;
        try
        {
            var list = await _categories.GetCategoriesAsync(IncludeInactive);
            Categories.Clear();
            foreach (var category in list)
            {
                Categories.Add(category);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void New()
    {
        _editingId = null;
        EditorTitle = "Nueva categoría";
        Name = string.Empty;
        Description = string.Empty;
        EditorIsActive = true;
        ErrorMessage = string.Empty;
        IsEditing = true;
    }

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private void Edit()
    {
        if (SelectedRow is null)
        {
            return;
        }

        var category = SelectedRow;
        _editingId = category.Id;
        EditorTitle = $"Editar: {category.Name}";
        Name = category.Name;
        Description = category.Description ?? string.Empty;
        EditorIsActive = category.IsActive;
        ErrorMessage = string.Empty;
        IsEditing = true;
    }

    private bool CanEdit() => SelectedRow is not null;

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy)
        {
            return;
        }

        var input = new CategoryInput(Name, Description, EditorIsActive);

        IsBusy = true;
        try
        {
            OperationResult result = _editingId is { } id
                ? await _categories.UpdateAsync(id, input)
                : await _categories.CreateAsync(input);

            if (!result.Success)
            {
                ErrorMessage = result.Error ?? "No se pudo guardar la categoría.";
                return;
            }

            IsEditing = false;
            ErrorMessage = string.Empty;
            await RefreshAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private async Task DeleteAsync()
    {
        if (SelectedRow is null)
        {
            return;
        }

        IsBusy = true;
        try
        {
            await _categories.DeleteAsync(SelectedRow.Id);
            SelectedRow = null;
            await RefreshAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }
}