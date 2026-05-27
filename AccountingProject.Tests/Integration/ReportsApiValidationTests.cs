using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AccountingProject.Tests.TestHelpers;
using ClosedXML.Excel;

namespace AccountingProject.Tests.Integration;

/// <summary>בדיקות API לדוחות — פרמטרים חסרים, הודעות שגיאה, קבצים לא תקינים.</summary>
public sealed class ReportsApiValidationTests
{
    private const string Year = "תשפ\"ו";

    public static TheoryData<string, string> GetMissingParamCases => new()
    {
        {
            "/api/reports/kindergarten-annual?employerId=0&academicYear=",
            "employerId ו-academicYear נדרשים."
        },
        {
            "/api/reports/school-annual?employerId=1&academicYear=",
            "employerId ו-academicYear נדרשים."
        },
        {
            "/api/reports/employees-personal?employerId=0",
            "employerId נדרש."
        },
        {
            "/api/reports/employees-employment-data?employerId=1&academicYear=",
            "employerId ו-academicYear נדרשים."
        },
        {
            "/api/reports/institution-hours?employerId=1&academicYear=תשפ\"ו&institutionSymbol=",
            "employerId, academicYear ו-institutionSymbol נדרשים."
        },
        {
            "/api/reports/annual-comparison-saved?employerId=0&academicYear=",
            "employerId ו-academicYear נדרשים."
        },
    };

    [Theory]
    [MemberData(nameof(GetMissingParamCases))]
    public async Task Reports_GetEndpoints_MissingParams_Return400WithMessage(string url, string expectedMessage)
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        var resp = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        var message = await IntegrationResponseAssert.ReadMessageAsync(resp);
        Assert.True(
            message.Contains(expectedMessage, StringComparison.Ordinal)
            || message.Contains("validation", StringComparison.OrdinalIgnoreCase),
            $"Expected '{expectedMessage}' or validation error, got: {message}");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public async Task Reports_MonthlyComparison_InvalidMonth_Return400(int month)
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "123456789", null, "Worker", 9, 2025, b => b.Band1());
        using var mp = CreateMultipart(upload);

        var resp = await client.PostAsync(
            $"/api/reports/monthly-comparison?employerId=1&academicYear={Uri.EscapeDataString(Year)}&month={month}",
            mp);

        await IntegrationResponseAssert.AssertBadRequestMessageAsync(
            resp, "employerId, academicYear וחודש תקין");
    }

    [Fact]
    public async Task Reports_MonthlyComparison_MissingFile_Return400WithMessage()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        var resp = await client.PostAsync(
            $"/api/reports/monthly-comparison?employerId=1&academicYear={Uri.EscapeDataString(Year)}&month=9",
            null);

        await IntegrationResponseAssert.AssertBadRequestMessageAsync(
            resp, "יש לצרף קובץ Excel");
    }

    [Fact]
    public async Task Reports_AnnualComparison_MissingFile_Return400WithMessage()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        var resp = await client.PostAsync(
            $"/api/reports/annual-comparison?employerId=1&academicYear={Uri.EscapeDataString(Year)}",
            null);

        await IntegrationResponseAssert.AssertBadRequestMessageAsync(
            resp, "יש לצרף קובץ Excel");
    }

    [Fact]
    public async Task Reports_AnnualComparison_MissingAcademicYear_Return400()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "123456789", null, "Worker", 9, 2025, b => b.Band1());
        using var mp = CreateMultipart(upload);

        var resp = await client.PostAsync(
            "/api/reports/annual-comparison?employerId=1&academicYear=",
            mp);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Reports_MonthlyComparison_NoDataRows_Return400WithHebrewMessage()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);
        var employerId = await ReportsApiIntegrationTests.SeedEmployerWithDataViaApiAsync(client);

        await using var upload = InvalidUploadWorkbooks.WrongMonthOnly();
        using var mp = CreateMultipart(upload);

        var resp = await client.PostAsync(
            $"/api/reports/monthly-comparison?employerId={employerId}&academicYear={Uri.EscapeDataString(Year)}&month=9",
            mp);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        var message = await IntegrationResponseAssert.ReadMessageAsync(resp);
        Assert.Contains("לא נמצאו שורות נתונים", message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Reports_AnnualComparison_NoDataRows_Return400WithHebrewMessage()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);
        var employerId = await ReportsApiIntegrationTests.SeedEmployerWithDataViaApiAsync(client);

        await using var upload = InvalidUploadWorkbooks.NoPayrollHeaders();
        using var mp = CreateMultipart(upload);

        var resp = await client.PostAsync(
            $"/api/reports/annual-comparison?employerId={employerId}&academicYear={Uri.EscapeDataString(Year)}",
            mp);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        var message = await IntegrationResponseAssert.ReadMessageAsync(resp);
        Assert.Contains("כותרות", message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Reports_Unauthorized_Return401()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = factory.CreateClient();

        var resp = await client.GetAsync(
            $"/api/reports/employees-personal?employerId=1");

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Reports_KindergartenAnnual_ValidSeeded_ReturnsXlsxWithSheetName()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);
        var employerId = await ReportsApiIntegrationTests.SeedEmployerWithDataViaApiAsync(client);

        var resp = await client.GetAsync(
            $"/api/reports/kindergarten-annual?employerId={employerId}&academicYear={Uri.EscapeDataString(Year)}");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var bytes = await resp.Content.ReadAsByteArrayAsync();
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        Assert.NotNull(wb.Worksheet("מצבת גנים"));
    }

    [Fact]
    public async Task Reports_InstitutionHours_ValidSeeded_ReturnsThreeRowLayout()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);
        var employerId = await ReportsApiIntegrationTests.SeedEmployerWithDataViaApiAsync(client);

        var resp = await client.GetAsync(
            $"/api/reports/institution-hours?employerId={employerId}&academicYear={Uri.EscapeDataString(Year)}&institutionSymbol=G-API");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        using var wb = new XLWorkbook(new MemoryStream(await resp.Content.ReadAsByteArrayAsync()));
        var ws = wb.Worksheet("בדיקת שעות לסמל");
        Assert.Equal("G-API", ws.Cell(2, 1).GetString());
        Assert.Equal("מצבת", ws.Cell(3, 1).GetString());
        Assert.Equal("הפרש", ws.Cell(4, 1).GetString());
    }

    private static MultipartFormDataContent CreateMultipart(Stream upload)
    {
        var mp = new MultipartFormDataContent();
        var fileContent = new StreamContent(upload);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        mp.Add(fileContent, "file", "payroll.xlsx");
        return mp;
    }
}
