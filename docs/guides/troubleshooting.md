# Troubleshooting Guide

Common issues and solutions when working with Tombatron.Turbo.

## Turbo Frames Issues

### Frame Not Updating

**Symptom:** Clicking a link inside a frame loads the full page instead of updating just the frame.

**Solutions:**

1. **Check that Turbo.js is loaded:**
   ```html
   <script type="module" src="https://cdn.jsdelivr.net/npm/@hotwired/turbo@8/dist/turbo.es2017-esm.min.js"></script>
   ```

2. **Verify the response contains a matching frame:**
   ```html
   <!-- Response must include a frame with the same ID -->
   <turbo-frame id="cart-items">
       <!-- Updated content -->
   </turbo-frame>
   ```

3. **Check your handler returns a partial:**
   ```csharp
   if (HttpContext.IsTurboFrameRequest())
   {
       return Partial("_MyPartial", Model);
   }
   ```

4. **Verify the partial includes the turbo-frame element:**
   ```html
   <!-- _MyPartial.cshtml -->
   <turbo-frame id="cart-items">
       @* Content must be wrapped in the frame *@
   </turbo-frame>
   ```

### Full Page Returned Instead of Frame Content

**Symptom:** Request works but returns entire page HTML.

**Solutions:**

1. **Check for the Turbo-Frame header:**
   ```csharp
   // Check the header directly
   if (Request.Headers.ContainsKey("Turbo-Frame"))

   // Or use the extension method
   if (HttpContext.IsTurboFrameRequest())
   ```

2. **Ensure middleware is configured:**
   ```csharp
   app.UseTurbo(); // Must be called
   ```

3. **Check the request in browser DevTools:**
   - Open Network tab
   - Click the link
   - Look for `Turbo-Frame` header in request headers

### Frame Shows "Content Missing"

**Symptom:** Frame displays error about missing content.

**Solution:** Ensure the response contains a `<turbo-frame>` with a matching ID. The frame ID in the response must exactly match the requesting frame's ID.

### Caching Issues

**Symptom:** Stale content appears or wrong content is served.

**Solutions:**

1. **Verify Vary header is enabled:**
   ```csharp
   options.AddVaryHeader = true; // Default
   ```

2. **Check CDN configuration:** Ensure your CDN respects the `Vary: Turbo-Frame` header.

3. **Clear browser cache** when testing.

## Turbo Streams Issues

### Not Receiving Updates

**Symptom:** Server sends updates but client doesn't receive them.

**Solutions:**

1. **Check SignalR connection:**
   ```javascript
   window.addEventListener('turbo-signalr:connected', () => {
       console.log('Connected');
   });
   ```

2. **Verify hub is mapped:**
   ```csharp
   app.MapTurboHub(); // Required
   ```

3. **Check stream name matches:**
   ```html
   <!-- Client -->
   <turbo-stream-source-signalr stream="notifications">
   ```
   ```csharp
   // Server - must match exactly
   await _turbo.Stream("notifications", ...);
   ```

4. **Check browser console for errors**

### SignalR Connection Fails

**Symptom:** SignalR won't connect.

**Solutions:**

1. **Check SignalR is loaded:**
   ```html
   <script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@8/dist/browser/signalr.min.js"></script>
   ```

2. **Verify hub URL:**
   ```html
   <turbo-stream-source-signalr hub-url="/turbo-hub">
   ```
   ```csharp
   // Must match the configured path
   options.HubPath = "/turbo-hub";
   ```

3. **Check for CORS issues** if hub is on a different origin.

4. **Check server logs** for connection errors.

### Subscription Denied

**Symptom:** Client connects but subscription fails.

**Solutions:**

1. **If using signed stream names:**
   - Ensure the stream name in the HTML matches what the server signed
   - Check token hasn't expired
   - Verify Data Protection is configured

2. **If using custom authorization:**
   - Check your `ITurboStreamAuthorization` implementation
   - Verify the user context is available

### Updates Applied to Wrong Element

**Symptom:** DOM updates target incorrect elements.

**Solution:** Ensure target IDs are unique across the page:

```csharp
// Target must be a unique ID
builder.Update("cart-total", ...); // ID: cart-total
```

## Form Issues

### Form Submission Not Working

**Symptom:** Forms don't submit or page reloads fully.

**Solutions:**

1. **For Turbo Frame forms**, ensure the response contains a matching frame.

2. **For Turbo Stream forms**, return the correct content type:
   ```csharp
   if (Request.Headers.Accept.ToString().Contains("text/vnd.turbo-stream.html"))
   {
       return Content(streamHtml, "text/vnd.turbo-stream.html");
   }
   ```

3. **Check anti-forgery tokens** are included if required.

### 400 Bad Request on POST

**Symptom:** POST requests return 400 error.

**Solutions:**

1. **Anti-forgery token missing:**
   ```html
   <form method="post">
       @Html.AntiForgeryToken()
       ...
   </form>
   ```

2. **Or disable for the page:**
   ```csharp
   [IgnoreAntiforgeryToken]
   public class MyPageModel : PageModel
   ```

## Performance Issues

### Slow Initial Load

**Solutions:**

1. **Use lazy loading** for below-the-fold frames:
   ```html
   <turbo-frame id="comments" src="..." loading="lazy">
   ```

2. **Reduce the number of streams** subscribed to initially.

### High Memory Usage

**Solutions:**

1. **Limit active subscriptions** - unsubscribe from streams when not needed.

2. **Check for memory leaks** in custom event handlers.

## Debugging Tips

### Enable Detailed Logging

```csharp
builder.Logging.AddFilter("Tombatron.Turbo", LogLevel.Debug);
builder.Logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
```

### Check Request Headers

Use browser DevTools to inspect:
- `Turbo-Frame` header on frame requests
- `Accept` header for stream requests

### Monitor SignalR Traffic

In browser DevTools:
1. Go to Network tab
2. Filter by "WS" for WebSocket
3. Click on the SignalR connection
4. View Messages tab to see traffic

### Test Without Turbo

Temporarily disable Turbo to verify server-side logic:

```html
<meta name="turbo-visit-control" content="reload">
```

## Common Error Messages

### "Frame not found"

The response didn't contain a `<turbo-frame>` with the expected ID.

### "Content missing from response"

Same as above - ensure your partial includes the frame element.

### "Subscription failed"

Authorization denied the stream subscription. Check authorization logic and user context.

### "Connection refused"

SignalR hub isn't reachable. Verify `MapTurboHub()` is called and the URL is correct.

## Getting Help

If you're still stuck:

1. Check the [GitHub Issues](https://github.com/tombatron/Tombatron.Turbo/issues)
2. Create a minimal reproduction case
3. Include relevant logs and configuration

## See Also

- [Turbo Frames Guide](turbo-frames.md)
- [Turbo Streams Guide](turbo-streams.md)
- [Testing Guide](testing.md)
