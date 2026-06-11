using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Capstone.Application.Common.Interfaces.Services;
using Microsoft.Extensions.Options;

namespace Capstone.Infrastructure.Services;

public class VietQrProvider : IVietQrProvider
{
    private readonly HttpClient _httpClient;
    private readonly VietQrSettings _settings;

    public VietQrProvider(HttpClient httpClient, IOptions<VietQrSettings> options)
    {
        _httpClient = httpClient;
        _settings = options.Value;
    }

    public async Task<CreatePaymentResponse> CreatePaymentAsync(int OrderCode, int Amount, string Description, string CancelUrl, string ReturnUrl)
    {
        var expireAtSeconds = (int)DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds();

        var payload = new
        {
            orderCode = OrderCode,
            amount = Amount,
            description = Description,
            cancelUrl = CancelUrl,
            returnUrl = ReturnUrl
        };

        var signature = CreateSignatureOfPaymentRequest(payload);

        var body = new
        {
            payload.orderCode,
            payload.amount,
            payload.description,
            payload.cancelUrl,
            payload.returnUrl,
            expiredAt = expireAtSeconds,
            signature
        };

        try
        {
            using var response = await _httpClient.PostAsJsonAsync("/v2/payment-requests", body);
            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new CreatePaymentResponse
                {
                    Status = $"HTTP {(int)response.StatusCode}",
                    Description = raw
                };
            }

            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            var code = root.TryGetProperty("code", out var c) ? c.GetString() : null;
            if (code != "00")
            {
                return new CreatePaymentResponse
                {
                    Status = code ?? "ERROR",
                    Description = root.TryGetProperty("desc", out var d) ? d.GetString() ?? string.Empty : string.Empty
                };
            }

            if (!root.TryGetProperty("data", out var data))
            {
                return new CreatePaymentResponse
                {
                    Status = "NO_DATA",
                    Description = raw
                };
            }

            return new CreatePaymentResponse
            {
                Bin = data.TryGetProperty("bin", out var bin) ? bin.GetString() ?? string.Empty : string.Empty,
                AccountNumber = data.TryGetProperty("accountNumber", out var an) ? an.GetString() ?? string.Empty : string.Empty,
                AccountName = data.TryGetProperty("accountName", out var aname) ? aname.GetString() ?? string.Empty : string.Empty,
                Amount = data.TryGetProperty("amount", out var amt) && amt.TryGetInt32(out var a) ? a : Amount,
                Description = data.TryGetProperty("description", out var desc) ? desc.GetString() ?? string.Empty : Description,
                OrderCode = data.TryGetProperty("orderCode", out var oc) && oc.TryGetInt32(out var ocv) ? ocv.ToString() : OrderCode.ToString(),
                PaymentLinkId = data.TryGetProperty("paymentLinkId", out var pl) ? pl.GetString() ?? string.Empty : string.Empty,
                Status = data.TryGetProperty("status", out var st) ? st.GetString() ?? string.Empty : string.Empty,
                CheckoutUrl = data.TryGetProperty("checkoutUrl", out var cu) ? cu.GetString() ?? string.Empty : string.Empty,
                QrCode = data.TryGetProperty("qrCode", out var q) ? q.GetString() ?? string.Empty : string.Empty
            };
        }
        catch (HttpRequestException ex)
        {
            return new CreatePaymentResponse
            {
                Status = "HTTP_ERROR",
                Description = ex.Message
            };
        }
        catch (JsonException ex)
        {
            return new CreatePaymentResponse
            {
                Status = "PARSE_ERROR",
                Description = ex.Message
            };
        }
    }

    private string CreateSignatureOfPaymentRequest(object payload)
    {
        var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(JsonSerializer.Serialize(payload))!;

        var data = string.Join("&", dict
            .OrderBy(kv => kv.Key, StringComparer.Ordinal)
            .Select(kv => $"{kv.Key}={kv.Value}"));

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_settings.ChecksumKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}