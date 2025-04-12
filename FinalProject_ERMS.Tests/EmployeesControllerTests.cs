using Xunit;
using Moq;
using Moq.Protected;
using FinalProject_ERMS.Controllers;
using Microsoft.AspNetCore.Mvc;
using FinalProject_ERMS.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;

public class EmployeesControllerTests
{
    private readonly EmployeesController _controller;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;

    public EmployeesControllerTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        var client = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new System.Uri("https://localhost:7174/")
        };

        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(new DefaultHttpContext());

        _controller = new EmployeesController(mockHttpClientFactory.Object, mockHttpContextAccessor.Object);
    }

    [Fact]
    public async Task Index_ReturnsViewResult_WithEmployeeList()
    {
        var employees = new List<Employee>
        {
            new Employee { EmployeeId = 1, Name = "Test Employee", Email = "test@example.com", Role = "Admin" }
        };
        SetupMockResponse(employees);

        var result = await _controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<Employee>>(viewResult.Model);
        Assert.Single(model);
        Assert.Equal("Test Employee", model[0].Name);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenIdIsNull()
    {
        var result = await _controller.Details(null);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsViewResult_WithEmployee()
    {
        var employee = new Employee { EmployeeId = 1, Name = "Test Employee", Email = "test@example.com", Role = "Admin" };
        SetupMockResponse(employee);

        var result = await _controller.Details(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<Employee>(viewResult.Model);
        Assert.Equal("Test Employee", model.Name);
    }

    [Fact]
    public void Create_Get_ReturnsViewResult()
    {
        var result = _controller.Create();
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Create_Post_ValidModel_RedirectsToIndex()
    {
        var employee = new Employee { EmployeeId = 1, Name = "New Employee", Email = "new@example.com", Role = "Admin" };
        SetupMockResponse(new HttpResponseMessage(HttpStatusCode.OK));

        var result = await _controller.Create(employee);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }

    [Fact]
    public async Task Create_Post_InvalidModel_ReturnsView()
    {
        _controller.ModelState.AddModelError("Name", "Required");

        var employee = new Employee();
        var result = await _controller.Create(employee);

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Edit_Get_ReturnsViewResult_WithEmployee()
    {
        var employee = new Employee { EmployeeId = 1, Name = "Existing Employee", Email = "existing@example.com", Role = "Admin" };
        SetupMockResponse(employee);

        var result = await _controller.Edit(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<Employee>(viewResult.Model);
        Assert.Equal(1, model.EmployeeId);
    }

    [Fact]
    public async Task Edit_Post_ValidModel_RedirectsToIndex()
    {
        var employee = new Employee { EmployeeId = 1, Name = "Edited Employee", Email = "edited@example.com", Role = "Admin" };
        SetupMockResponse(new HttpResponseMessage(HttpStatusCode.OK));

        var result = await _controller.Edit(1, employee);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }

    [Fact]
    public async Task Edit_Post_IdMismatch_ReturnsNotFound()
    {
        var employee = new Employee { EmployeeId = 2, Name = "Mismatch", Email = "mismatch@example.com", Role = "Admin" };
        var result = await _controller.Edit(1, employee);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Get_ReturnsViewResult_WithEmployee()
    {
        var employee = new Employee { EmployeeId = 1, Name = "Delete Employee", Email = "delete@example.com", Role = "Admin" };
        SetupMockResponse(employee);

        var result = await _controller.Delete(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<Employee>(viewResult.Model);
        Assert.Equal(1, model.EmployeeId);
    }

    [Fact]
    public async Task DeleteConfirmed_ReturnsRedirectToIndex()
    {
        SetupMockResponse(new HttpResponseMessage(HttpStatusCode.OK));

        var result = await _controller.DeleteConfirmed(1);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }

    [Fact]
    public async Task Index_ReturnsProblem_WhenApiFails()
    {
        SetupMockResponse(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var result = await _controller.Index();

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problemResult.StatusCode);
    }

    [Fact]
    public async Task Details_ReturnsProblem_WhenApiFails()
    {
        SetupMockResponse(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var result = await _controller.Details(1);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problemResult.StatusCode);
    }

    [Fact]
    public async Task Create_Post_InvalidModel_ReturnsViewWithErrors()
    {
        var employee = new Employee { EmployeeId = 1, Name = "", Email = "invalidemail", Role = "" };
        _controller.ModelState.AddModelError("Name", "Required");

        var result = await _controller.Create(employee);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsAssignableFrom<Employee>(viewResult.Model);
    }

    [Fact]
    public async Task Edit_Get_ReturnsProblem_WhenApiFails()
    {
        SetupMockResponse(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var result = await _controller.Edit(1);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problemResult.StatusCode);
    }

    [Fact]
    public async Task Index_ReturnsEmptyList_WhenNoEmployees()
    {
        SetupMockResponse(new List<Employee>());

        var result = await _controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<Employee>>(viewResult.Model);
        Assert.Empty(model);
    }

    private void SetupMockResponse<T>(T content)
    {
        var json = JsonSerializer.Serialize(content);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            });
    }

    private void SetupMockResponse(HttpResponseMessage responseMessage)
    {
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);
    }
}
