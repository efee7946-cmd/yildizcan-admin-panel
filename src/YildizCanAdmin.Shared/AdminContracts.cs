using System.Text.Json;

namespace YildizCanAdmin.Shared;

// YıldızCan Node admin API (api/admin.js) JSON sözleşmeleriyle birebir.
// JSON camelCase; GetFromJsonAsync web defaults ile PascalCase'e büyük/küçük
// harf duyarsız eşlenir. Şifreli roster sunucuda çözülüp düz gelir.

public record AdminUser(
    string Username,
    string? DisplayName,
    string? Email,
    bool EmailVerified,
    int StudentCount);

public record UsersResponse(List<AdminUser> Users);

public record Student(
    string Username,
    string Id,
    string? FullName,
    int? BirthYear,
    string? SupportNotes,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt);

public record StudentsResponse(List<Student> Students);

public record Summary(
    int Plays,
    int Items,
    int Errors,
    int? Accuracy,
    int Perfect,
    DateTimeOffset? LastPlayed);

public record StudentDetailResponse(Student Student, Summary Summary);

public record ObjResult(
    string? Id,
    DateTimeOffset? Date,
    int? Items,
    int? Errors,
    int? Accuracy);

// speechHistory ve stats alt nesneleri (adaptive/speechMap/stars) v1'de birebir
// modellenmedi — şimdilik ham JsonElement. İhtiyaç oldukça tiplenir.
public record SessionsResponse(
    List<ObjResult> ObjResults,
    List<JsonElement> SpeechHistory);

public record StatsResponse(
    Summary Summary,
    JsonElement Adaptive,
    JsonElement SpeechMap,
    JsonElement Stars);

public record ContentQuestion(
    string Id,
    string Topic,
    string Tr,
    string En,
    string GoalTr,
    string GoalEn,
    string Query,
    bool Published,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt);

public record ContentOverride(
    bool? Hidden,
    string? Tr,
    string? En,
    string? Query,
    DateTimeOffset? UpdatedAt);

public record ContentListResponse(
    List<ContentQuestion> Questions,
    Dictionary<string, ContentOverride>? Overrides = null);

public record BuiltinQuestion(int Idx, string Key, string Tr, string En);
