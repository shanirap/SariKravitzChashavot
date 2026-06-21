using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AccountingProject.Contracts;
using AccountingProject.Domain;
using ClosedXML.Excel;

namespace AccountingProject.Tests.Integration;

public sealed class BulkImportApiIntegrationTests
{
    [Fact]
    public async Task TemplateEmployers_ReturnsXlsx()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        var resp = await client.GetAsync("/api/bulk-import/template/employers");

        resp.EnsureSuccessStatusCode();
        Assert.Equal(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            resp.Content.Headers.ContentType?.MediaType);
        Assert.True((await resp.Content.ReadAsByteArrayAsync()).Length > 100);
    }

    [Fact]
    public async Task TemplateEmployees_WithEmployerId_ReturnsXlsx()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        var er = await client.PostAsJsonAsync("/api/employers", new EmployerDto { Name = "Tpl Employer" });
        er.EnsureSuccessStatusCode();
        var created = await er.Content.ReadFromJsonAsync<EmployerIdOnly>(RelaxedJson);
        Assert.NotNull(created);
        var id = created!.Id;

        var resp = await client.GetAsync($"/api/bulk-import/template/employees?includeEmployerName=false&employerId={id}");

        resp.EnsureSuccessStatusCode();
        Assert.True((await resp.Content.ReadAsByteArrayAsync()).Length > 100);
    }

    [Fact]
    public async Task ImportEmployees_EmptyFile_ReturnsBadRequest()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        using var mp = new MultipartFormDataContent();
        var bytes = Array.Empty<byte>();
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        mp.Add(file, "file", "empty.xlsx");

        var resp = await client.PostAsync("/api/bulk-import/employees", mp);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task ImportEmployeesForEmployer_ComputesSupplementaryAndAgeHours()
    {
        const string year = "תשפ\"ו";
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        var er = await client.PostAsJsonAsync("/api/employers", new EmployerDto { Name = "Bulk Calc API" });
        er.EnsureSuccessStatusCode();
        var employer = await er.Content.ReadFromJsonAsync<EmployerIdOnly>(RelaxedJson);
        Assert.NotNull(employer);

        (await client.PostAsJsonAsync($"/api/employers/{employer!.Id}/institution-symbols",
            new EmployerInstitutionSymbolDto { InstitutionSymbol = "API-1", InstitutionSymbolName = "Garden" }))
            .EnsureSuccessStatusCode();

        var bytes = BuildEmployerScopedImportRow("888777666", "API-1", year, new DateOnly(1973, 1, 15));
        using var mp = new MultipartFormDataContent();
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        mp.Add(file, "file", "import.xlsx");

        var importResp = await client.PostAsync($"/api/bulk-import/employers/{employer.Id}/employees", mp);
        importResp.EnsureSuccessStatusCode();
        var importResult = await importResp.Content.ReadFromJsonAsync<ImportResult>(RelaxedJson);
        Assert.NotNull(importResult);
        Assert.Equal(1, importResult!.Imported);

        var empResp = await client.GetAsync($"/api/employers/{employer.Id}/employees/by-id-number/888777666");
        empResp.EnsureSuccessStatusCode();
        var employee = await empResp.Content.ReadFromJsonAsync<EmployeeIdOnly>(RelaxedJson);
        Assert.NotNull(employee);

        var edResp = await client.GetAsync($"/api/employment-data/employee/{employee!.Id}/employer/{employer.Id}");
        edResp.EnsureSuccessStatusCode();
        var records = await edResp.Content.ReadFromJsonAsync<List<EmploymentDataDto>>(RelaxedJson);
        var record = Assert.Single(records!);
        Assert.Equal(23.50m, record.Grade1Total);
        Assert.Equal(2m, record.Grade1AgeHours);
        Assert.Contains(record.Slots!, s => s.SlotIndex == 2 && s.SupplementaryParentSlotIndex == 1 && s.WeeklyHours == 3m);
    }

    private static byte[] BuildEmployerScopedImportRow(
        string idNumber,
        string symbol,
        string academicYear,
        DateOnly birthDate)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("עובדים");
        var headers = new List<string>
        {
            "תז", "מספר_עובד_בעוקץ", "שם_משפחה", "שם_פרטי", "מין", "תאריך_לידה", "טל",
            "תאריך_לידה_ילד_1", "תאריך_לידה_ילד_2", "תאריך_לידה_ילד_3", "תאריך_לידה_ילד_4",
            "תאריך_לידה_ילד_5", "תאריך_לידה_ילד_6", "תאריך_לידה_ילד_7", "תאריך_לידה_ילד_8",
            "תאריך_לידה_ילד_9", "תאריך_לידה_ילד_10", "שנת_לימודים",
            "דרגה1_שם_הדירוג", "דרגה1_דרגה", "דרגה1_תפקיד", "דרגה1_ותק",
            "דרגה1_סהכ", "דרגה1_אחוז_משרה", "דרגה1_קרן_השתלמות_אחוז", "דרגה1_שעות_גיל",
            "דרגה1_אחוז_תוספת_אם", "דרגה1_גמולי_השתלמות", "דרגה1_כפל_תואר",
            "דרגה1_1_סמל_מוסד", "דרגה1_1_שעות_שבועיות", "דרגה1_1_בסיס_משרה",
        };
        for (var i = 0; i < headers.Count; i++)
            ws.Cell(1, i + 1).Value = headers[i];

        int Map(string h) => headers.IndexOf(h) + 1;
        const int row = 2;
        var col = 1;
        ws.Cell(row, col++).Value = idNumber;
        col++;
        ws.Cell(row, col++).Value = "כהן";
        ws.Cell(row, col++).Value = "רחל";
        ws.Cell(row, col++).Value = "נקבה";
        ws.Cell(row, col++).Value = birthDate.ToString("yyyy-MM-dd");
        col += 11;
        ws.Cell(row, col++).Value = academicYear;
        ws.Cell(row, Map("דרגה1_שם_הדירוג")).Value = "יסודי וגנים";
        ws.Cell(row, Map("דרגה1_דרגה")).Value = "ב";
        ws.Cell(row, Map("דרגה1_תפקיד")).Value = "גננת ראשית";
        ws.Cell(row, Map("דרגה1_ותק")).Value = 1;
        ws.Cell(row, Map("דרגה1_1_סמל_מוסד")).Value = symbol;
        ws.Cell(row, Map("דרגה1_1_שעות_שבועיות")).Value = 20.50m;
        ws.Cell(row, Map("דרגה1_1_בסיס_משרה")).Value = 30;

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private static readonly JsonSerializerOptions RelaxedJson = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private sealed class EmployerIdOnly
    {
        public int Id { get; set; }
    }

    private sealed class EmployeeIdOnly
    {
        public int Id { get; set; }
    }
}
