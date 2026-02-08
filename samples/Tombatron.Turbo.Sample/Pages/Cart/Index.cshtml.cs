using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Tombatron.Turbo.Sample.Pages.Cart;

public class IndexModel : PageModel
{
    // In-memory cart storage (in a real app, this would be session or database-backed)
    private static readonly List<CartItem> _items = new()
    {
        new CartItem { Id = 1, Name = "Widget", Price = 9.99m, Quantity = 2 },
        new CartItem { Id = 2, Name = "Gadget", Price = 24.99m, Quantity = 1 },
        new CartItem { Id = 3, Name = "Gizmo", Price = 14.99m, Quantity = 3 }
    };

    private static int _nextId = 4;

    public List<CartItem> Items => _items;

    public decimal CartTotal => _items.Sum(i => i.Price * i.Quantity);

    public int ItemCount => _items.Sum(i => i.Quantity);

    public void OnGet()
    {
    }

    public IActionResult OnPostAddItem(string name, decimal price)
    {
        if (!string.IsNullOrWhiteSpace(name) && price > 0)
        {
            _items.Add(new CartItem
            {
                Id = _nextId++,
                Name = name,
                Price = price,
                Quantity = 1
            });
        }

        return RedirectToPage();
    }

    public IActionResult OnPostRemoveItem(int itemId)
    {
        CartItem? item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            _items.Remove(item);
        }

        // For Turbo Frame requests, return the updated page
        // The frame will be updated with an empty response or redirect
        if (HttpContext.IsTurboFrameRequest())
        {
            // Return a turbo-stream response to remove the item
            return Content(
                $"<turbo-frame id=\"item_{itemId}\"></turbo-frame>",
                "text/html");
        }

        return RedirectToPage();
    }
}

public class CartItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
