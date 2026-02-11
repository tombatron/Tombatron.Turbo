using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;

namespace Tombatron.Turbo.Sample.Pages.Cart;

[IgnoreAntiforgeryToken]
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
        CartItem? newItem = null;

        if (!string.IsNullOrWhiteSpace(name) && price > 0)
        {
            newItem = new CartItem
            {
                Id = _nextId++,
                Name = name,
                Price = price,
                Quantity = 1
            };
            _items.Add(newItem);
        }

        // Check if this is a Turbo request (accepts turbo-stream)
        if (Request.Headers.Accept.ToString().Contains("text/vnd.turbo-stream.html"))
        {
            var response = new StringBuilder();

            if (newItem != null)
            {
                // Append the new item to the cart
                response.AppendLine($@"<turbo-stream action=""append"" target=""cart-items"">
<template>
{RenderCartItem(newItem)}
</template>
</turbo-stream>");

                // Update the cart summary
                response.AppendLine($@"<turbo-stream action=""replace"" target=""cart-summary"">
<template>
{RenderCartSummary()}
</template>
</turbo-stream>");

                // Clear the form by replacing it
                response.AppendLine($@"<turbo-stream action=""replace"" target=""add-item-form"">
<template>
{RenderAddItemForm()}
</template>
</turbo-stream>");

                // Remove empty cart message if it exists
                response.AppendLine(@"<turbo-stream action=""remove"" target=""empty-cart-message""></turbo-stream>");
            }

            return Content(response.ToString(), "text/vnd.turbo-stream.html");
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

        // Check if this is a Turbo request (accepts turbo-stream)
        if (Request.Headers.Accept.ToString().Contains("text/vnd.turbo-stream.html"))
        {
            var response = new StringBuilder();

            // Remove the item frame
            response.AppendLine($@"<turbo-stream action=""remove"" target=""item_{itemId}""></turbo-stream>");

            // Update the cart summary
            response.AppendLine($@"<turbo-stream action=""replace"" target=""cart-summary"">
<template>
{RenderCartSummary()}
</template>
</turbo-stream>");

            // Add empty cart message if cart is now empty
            if (!_items.Any())
            {
                response.AppendLine(@"<turbo-stream action=""append"" target=""cart-items"">
<template>
<div id=""empty-cart-message"" class=""cart-item"" style=""text-align: center; color: var(--text-muted);"">
    Your cart is empty
</div>
</template>
</turbo-stream>");
            }

            return Content(response.ToString(), "text/vnd.turbo-stream.html");
        }

        return RedirectToPage();
    }

    private string RenderCartItem(CartItem item)
    {
        var total = (item.Price * item.Quantity).ToString("C");
        return $@"<turbo-frame id=""item_{item.Id}"">
    <div style=""display: flex; justify-content: space-between; align-items: center;"">
        <div>
            <strong>{System.Net.WebUtility.HtmlEncode(item.Name)}</strong>
            <div style=""color: var(--text-secondary); font-size: 0.9rem; margin-top: 4px;"">
                {item.Price:C} &times; {item.Quantity} = <strong>{total}</strong>
            </div>
        </div>
        <form method=""post"" action=""/Cart?handler=RemoveItem&amp;itemId={item.Id}"">
            <button type=""submit"" class=""btn btn-danger btn-sm"">Remove</button>
        </form>
    </div>
</turbo-frame>";
    }

    private string RenderCartSummary()
    {
        var itemText = ItemCount != 1 ? "items" : "item";
        return $@"<turbo-frame id=""cart-summary"">
    <div style=""text-align: center;"">
        <div style=""font-size: 2rem; font-weight: 600; color: var(--accent);"">
            {CartTotal:C}
        </div>
        <div style=""color: var(--text-secondary); margin-top: 4px;"">
            {ItemCount} {itemText} in cart
        </div>
    </div>
</turbo-frame>";
    }

    private static string RenderAddItemForm()
    {
        return @"<turbo-frame id=""add-item-form"">
    <form method=""post"" action=""/Cart?handler=AddItem"">
        <div class=""form-row"">
            <input type=""text"" name=""name"" placeholder=""Item name"" required style=""flex: 1;"" />
        </div>
        <div class=""form-row"">
            <input type=""number"" name=""price"" placeholder=""Price"" step=""0.01"" min=""0.01"" required style=""width: 120px;"" />
            <button type=""submit"" class=""btn"">Add Item</button>
        </div>
    </form>
</turbo-frame>";
    }
}

public class CartItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
