using System.Net.Http.Json;
using YildizCanAdmin.Shared;

namespace YildizCanAdmin.Web.Services;

// YıldızCan Node admin API'sine typed istemci. BaseAddress ve x-admin-key header'ı
// Program.cs'te AddHttpClient ile yapılandırılır; ADMIN_KEY yalnızca sunucuda kalır.
public sealed class YildizCanApiClient(HttpClient http)
{
    public async Task<List<AdminUser>> GetUsersAsync(CancellationToken ct = default)
        => (await http.GetFromJsonAsync<UsersResponse>("api/admin?resource=users", ct))?.Users ?? [];

    public async Task<List<Student>> GetStudentsAsync(string? user = null, CancellationToken ct = default)
    {
        var url = "api/admin?resource=students";
        if (!string.IsNullOrEmpty(user)) url += "&user=" + Uri.EscapeDataString(user);
        return (await http.GetFromJsonAsync<StudentsResponse>(url, ct))?.Students ?? [];
    }

    public Task<StudentDetailResponse?> GetStudentAsync(string user, string id, CancellationToken ct = default)
        => http.GetFromJsonAsync<StudentDetailResponse>(
            $"api/admin?resource=student&user={Uri.EscapeDataString(user)}&id={Uri.EscapeDataString(id)}", ct);

    public Task<SessionsResponse?> GetSessionsAsync(string user, string id, CancellationToken ct = default)
        => http.GetFromJsonAsync<SessionsResponse>(
            $"api/admin?resource=sessions&user={Uri.EscapeDataString(user)}&id={Uri.EscapeDataString(id)}", ct);

    public Task<StatsResponse?> GetStatsAsync(string user, string id, CancellationToken ct = default)
        => http.GetFromJsonAsync<StatsResponse>(
            $"api/admin?resource=stats&user={Uri.EscapeDataString(user)}&id={Uri.EscapeDataString(id)}", ct);

    public async Task<ContentListResponse> GetContentAsync(CancellationToken ct = default)
        => await http.GetFromJsonAsync<ContentListResponse>("api/admin?resource=content", ct)
           ?? new ContentListResponse([]);

    public async Task<bool> OverrideBuiltinAsync(string key, string? tr, string? en, string? query, bool hidden, CancellationToken ct = default)
    {
        var r = await http.PostAsJsonAsync("api/admin?resource=content", new
        {
            action = "override",
            key,
            tr,
            en,
            query,
            hidden
        }, ct);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> RestoreBuiltinAsync(string key, CancellationToken ct = default)
    {
        var r = await http.PostAsJsonAsync("api/admin?resource=content", new { action = "restore", key }, ct);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> UpsertContentAsync(string? id, string topic, string tr, string en, string goalTr, string goalEn, string query, CancellationToken ct = default)
    {
        var r = await http.PostAsJsonAsync("api/admin?resource=content", new
        {
            action = "upsert",
            question = new { id, topic, tr, en, goalTr, goalEn, query }
        }, ct);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> SetContentPublishedAsync(string id, bool published, CancellationToken ct = default)
    {
        var r = await http.PostAsJsonAsync("api/admin?resource=content", new { action = "publish", id, published }, ct);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteContentAsync(string id, CancellationToken ct = default)
    {
        var r = await http.PostAsJsonAsync("api/admin?resource=content", new { action = "delete", id }, ct);
        return r.IsSuccessStatusCode;
    }

    public async Task<List<AiReply>> GetAiRepliesAsync(bool flaggedOnly = false, CancellationToken ct = default)
        => (await http.GetFromJsonAsync<AiRepliesResponse>(
            "api/admin?resource=aireplies" + (flaggedOnly ? "&flagged=1" : ""), ct))?.Replies ?? [];

    public async Task<bool> SetAiFlagAsync(long id, bool flagged, string? note, CancellationToken ct = default)
    {
        var r = await http.PostAsJsonAsync("api/admin?resource=aireplies", new { id, flagged, note }, ct);
        return r.IsSuccessStatusCode;
    }
}
