using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FinalProject_ERMS.Controllers;
using FinalProject_ERMS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using Xunit;
using System.Text.Json;
using System.Collections.Generic;

public class ProjectsControllerTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly HttpClient _httpClient;

    public ProjectsControllerTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _handlerMock = new Mock<HttpMessageHandler>();

        _httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new System.Uri("https://localhost:7174/")
        };

        var fakeContext = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(_ => _.HttpContext).Returns(fakeContext);
    }

    private IHttpClientFactory MockHttpClientFactory()
    {
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(_httpClient);
        return factoryMock.Object;
    }

    private void SetupMockResponse<T>(T content)
    {
        var json = JsonSerializer.Serialize(content);

        _handlerMock.Protected()
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
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);
    }

    private ProjectsController CreateController()
    {
        return new ProjectsController(MockHttpClientFactory(), _httpContextAccessorMock.Object);
    }

    [Fact]
    public async Task Index_ReturnsViewResult_WithListOfProjects()
    {
        var projects = new List<Project> { new Project { ProjectId = 1, Name = "Test Project" } };
        SetupMockResponse(projects);

        var controller = new ProjectsController(MockHttpClientFactory(), _httpContextAccessorMock.Object);
        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<Project>>(viewResult.ViewData.Model);
        Assert.Single(model);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenIdIsNull()
    {
        var controller = new ProjectsController(MockHttpClientFactory(), _httpContextAccessorMock.Object);
        var result = await controller.Details(null);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsViewResult_WithProject()
    {
        var project = new Project { ProjectId = 1, Name = "Test Project" };
        SetupMockResponse(project);

        var controller = new ProjectsController(MockHttpClientFactory(), _httpContextAccessorMock.Object);
        var result = await controller.Details(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<Project>(viewResult.ViewData.Model);
        Assert.Equal(1, model.ProjectId);
    }


    [Fact]
    public async Task Create_Post_ValidModel_RedirectsToIndex()
    {
        SetupMockResponse(new HttpResponseMessage(HttpStatusCode.OK));

        var controller = new ProjectsController(MockHttpClientFactory(), _httpContextAccessorMock.Object);
        var project = new Project { ProjectId = 1, Name = "Test Project", ManagerId = 1 };

        var result = await controller.Create(project);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }

    [Fact]
    public async Task Edit_Post_ValidModel_RedirectsToIndex()
    {
        SetupMockResponse(new HttpResponseMessage(HttpStatusCode.OK));

        var controller = new ProjectsController(MockHttpClientFactory(), _httpContextAccessorMock.Object);
        var project = new Project { ProjectId = 1, Name = "Updated Project", ManagerId = 1 };

        var result = await controller.Edit(1, project);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }

    [Fact]
    public async Task Edit_Post_IdMismatch_ReturnsNotFound()
    {
        var controller = new ProjectsController(MockHttpClientFactory(), _httpContextAccessorMock.Object);
        var project = new Project { ProjectId = 2, Name = "Mismatch Project" };

        var result = await controller.Edit(1, project);

        Assert.IsType<NotFoundResult>(result);
    }


    [Fact]
    public async Task Delete_Get_ReturnsViewResult_WithProject()
    {
        var project = new Project { ProjectId = 1, Name = "Delete Project" };
        SetupMockResponse(project);

        var controller = new ProjectsController(MockHttpClientFactory(), _httpContextAccessorMock.Object);
        var result = await controller.Delete(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<Project>(viewResult.ViewData.Model);
        Assert.Equal(1, model.ProjectId);
    }

    [Fact]
    public async Task DeleteConfirmed_ReturnsRedirectToIndex()
    {
        SetupMockResponse(new HttpResponseMessage(HttpStatusCode.OK));

        var controller = new ProjectsController(MockHttpClientFactory(), _httpContextAccessorMock.Object);
        var result = await controller.DeleteConfirmed(1);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }
    [Fact]
    public async Task Index_ReturnsProblem_WhenApiFails()
    {
        SetupMockResponse(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var controller = CreateController();

        var result = await controller.Index();

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problemResult.StatusCode);
    }

    [Fact]
    public async Task Details_ReturnsProblem_WhenApiFails()
    {
        SetupMockResponse(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var controller = CreateController();

        var result = await controller.Details(1);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problemResult.StatusCode);
    }

    [Fact]
    public async Task Edit_Get_ReturnsProblem_WhenApiFails()
    {
        SetupMockResponse(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var controller = CreateController();

        var result = await controller.Edit(1);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problemResult.StatusCode);
    }

    [Fact]
    public async Task Delete_Get_ReturnsProblem_WhenApiFails()
    {
        SetupMockResponse(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var controller = CreateController();

        var result = await controller.Delete(1);

        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problemResult.StatusCode);
    }


    [Fact]
    public async Task Create_Post_InvalidModel_ReturnsViewWithErrors()
    {
        var employees = new List<Employee> { new Employee { EmployeeId = 1, Name = "Manager", Role = "Manager" } };
        SetupMockResponse(employees);

        var controller = CreateController();
        controller.ModelState.AddModelError("Name", "Required");

        var project = new Project();
        var result = await controller.Create(project);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsAssignableFrom<Project>(viewResult.Model);
    }


    [Fact]
    public async Task Index_ReturnsEmptyList_WhenNoProjects()
    {
        SetupMockResponse(new List<Project>());

        var controller = CreateController();

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<Project>>(viewResult.ViewData.Model);
        Assert.Empty(model);
    }
}
