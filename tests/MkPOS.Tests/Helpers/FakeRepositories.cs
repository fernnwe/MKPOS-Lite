using MKPOS.Application.Abstractions.Repositories;
using MKPOS.Domain.Entities;

namespace MKPOS.Tests.Helpers;

public sealed class FakeUserRepository : IUserRepository
{
    private readonly List<User> _users = new();

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.Username == username));
    }

    public Task<User?> GetFirstAdminAsync(CancellationToken ct = default)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.IsAdmin));
    }

    public Task<bool> ExistsAsync(string username, CancellationToken ct = default)
    {
        return Task.FromResult(_users.Any(u => u.Username == username));
    }

    public Task AddAsync(User user, CancellationToken ct = default)
    {
        _users.Add(user);
        return Task.CompletedTask;
    }
}

public sealed class FakeCompanyRepository : ICompanyRepository
{
    private Company? _company;

    public Task<Company?> GetCurrentAsync(CancellationToken ct = default)
    {
        return Task.FromResult(_company);
    }

    public Task SaveAsync(Company company, CancellationToken ct = default)
    {
        _company = company;
        return Task.CompletedTask;
    }
}

public sealed class FakeCategoryRepository : ICategoryRepository
{
    private readonly List<Category> _categories = new();

    public Task<IReadOnlyList<Category>> GetAllAsync(bool includeInactive, CancellationToken ct = default)
    {
        IReadOnlyList<Category> result = _categories
            .Where(c => includeInactive || c.IsActive)
            .OrderBy(c => c.Name)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return Task.FromResult(_categories.FirstOrDefault(c => c.Id == id));
    }

    public Task<bool> ExistsNameAsync(string name, Guid? excludeId, CancellationToken ct = default)
    {
        return Task.FromResult(_categories.Any(c =>
            string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase) && c.Id != excludeId));
    }

    public Task<IReadOnlyDictionary<Guid, int>> GetProductCountsAsync(CancellationToken ct = default)
    {
        IReadOnlyDictionary<Guid, int> counts = _categories.Count == 0
            ? new Dictionary<Guid, int>()
            : new Dictionary<Guid, int>();
        return Task.FromResult(counts);
    }

    public Task AddAsync(Category category, CancellationToken ct = default)
    {
        _categories.Add(category);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Category category, CancellationToken ct = default)
    {
        var index = _categories.FindIndex(c => c.Id == category.Id);
        if (index >= 0)
        {
            _categories[index] = category;
        }

        return Task.CompletedTask;
    }
}

public sealed class FakeProductRepository : IProductRepository
{
    private readonly List<Product> _products = new();

    public Task<IReadOnlyList<Product>> GetAllAsync(
        string? search,
        Guid? categoryId,
        bool includeInactive,
        CancellationToken ct = default)
    {
        var query = _products.Where(p => includeInactive || p.IsActive);

        if (categoryId is { } id)
        {
            query = query.Where(p => p.CategoryId == id);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                p.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                || p.Sku.Contains(term, StringComparison.OrdinalIgnoreCase)
                || (p.Barcode != null && p.Barcode.Contains(term, StringComparison.OrdinalIgnoreCase)));
        }

        IReadOnlyList<Product> result = query
            .OrderBy(p => p.Name)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return Task.FromResult(_products.FirstOrDefault(p => p.Id == id));
    }

    public Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var idSet = ids.ToHashSet();
        IReadOnlyList<Product> result = _products.Where(p => idSet.Contains(p.Id)).ToList();
        return Task.FromResult(result);
    }

    public Task<bool> ExistsSkuAsync(string sku, Guid? excludeId, CancellationToken ct = default)
    {
        return Task.FromResult(_products.Any(p =>
            string.Equals(p.Sku, sku, StringComparison.OrdinalIgnoreCase) && p.Id != excludeId));
    }

    public Task<bool> ExistsBarcodeAsync(string barcode, Guid? excludeId, CancellationToken ct = default)
    {
        return Task.FromResult(_products.Any(p =>
            p.Barcode != null
            && string.Equals(p.Barcode, barcode, StringComparison.OrdinalIgnoreCase)
            && p.Id != excludeId));
    }

    public Task AddAsync(Product product, CancellationToken ct = default)
    {
        _products.Add(product);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        var index = _products.FindIndex(p => p.Id == product.Id);
        if (index >= 0)
        {
            _products[index] = product;
        }

        return Task.CompletedTask;
    }
}

public sealed class FakeSaleRepository : ISaleRepository
{
    private readonly List<Sale> _sales = new();

    public Task<int> GetLastTicketNumberAsync(CancellationToken ct = default)
    {
        return Task.FromResult(_sales.Count == 0 ? 0 : _sales.Max(s => s.TicketNumber));
    }

    public Task CompleteAsync(Sale sale, IReadOnlyList<Product> productsToAdjust, Customer? customer, CancellationToken ct = default)
    {
        foreach (var product in productsToAdjust)
        {
            product.UpdatedAt = DateTime.UtcNow;
        }

        if (customer is not null)
        {
            customer.UpdatedAt = DateTime.UtcNow;
        }

        _sales.Add(sale);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Sale>> GetRecentAsync(int take, CancellationToken ct = default)
    {
        IReadOnlyList<Sale> result = _sales
            .OrderByDescending(s => s.SaleDate)
            .Take(take)
            .ToList();
        return Task.FromResult(result);
    }
}

public sealed class FakeCustomerRepository : ICustomerRepository
{
    private readonly List<Customer> _customers = new();

    public Task<IReadOnlyList<Customer>> GetAllAsync(string? search, bool includeInactive, CancellationToken ct = default)
    {
        var query = _customers.Where(c => includeInactive || c.IsActive);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c =>
                c.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                || (c.Phone != null && c.Phone.Contains(term, StringComparison.OrdinalIgnoreCase))
                || (c.Email != null && c.Email.Contains(term, StringComparison.OrdinalIgnoreCase)));
        }

        IReadOnlyList<Customer> result = query.OrderBy(c => c.Name).ToList();
        return Task.FromResult(result);
    }

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return Task.FromResult(_customers.FirstOrDefault(c => c.Id == id));
    }

    public Task AddAsync(Customer customer, CancellationToken ct = default)
    {
        _customers.Add(customer);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Customer customer, CancellationToken ct = default)
    {
        var index = _customers.FindIndex(c => c.Id == customer.Id);
        if (index >= 0)
        {
            _customers[index] = customer;
        }

        return Task.CompletedTask;
    }
}