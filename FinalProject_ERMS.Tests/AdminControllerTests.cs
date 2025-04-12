using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalProject_ERMS.Controllers;
using FinalProject_ERMS.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

public class AdminControllerTests
{
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
    private readonly AdminController _controller;

    public AdminControllerTests()
    {
        var userStoreMock = new Mock<IUserStore<IdentityUser>>();
        _userManagerMock = new Mock<UserManager<IdentityUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

        var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
        _roleManagerMock = new Mock<RoleManager<IdentityRole>>(roleStoreMock.Object, null, null, null, null);

        _controller = new AdminController(_userManagerMock.Object, _roleManagerMock.Object);
    }

    [Fact]
    public void Index_ReturnsViewResult()
    {
        var result = _controller.Index();
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void ListUsers_ReturnsViewResult_WithUsersAndRoles()
    {
        var users = new List<IdentityUser> { new IdentityUser { Id = "1", Email = "test@example.com" } }.AsQueryable();
        var roles = new List<IdentityRole> { new IdentityRole("Admin") }.AsQueryable();

        _userManagerMock.Setup(u => u.Users).Returns(users);
        _roleManagerMock.Setup(r => r.Roles).Returns(roles);
        _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync(new List<string> { "Admin" });

        var result = _controller.ListUsers();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<UserWithRoleViewModel>>(viewResult.Model);
        Assert.Single(model);
        Assert.Equal("test@example.com", model[0].Email);
    }

    [Fact]
    public async Task EditUserRole_Get_ReturnsNotFound_WhenUserNotFound()
    {
        _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((IdentityUser)null);

        var result = await _controller.EditUserRole("nonexistent");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditUserRole_Get_ReturnsViewResult_WithViewModel()
    {
        var user = new IdentityUser { Id = "1", Email = "test@example.com" };

        _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
        _userManagerMock.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });
        _roleManagerMock.Setup(r => r.Roles).Returns(new List<IdentityRole> { new IdentityRole("Admin") }.AsQueryable());

        var result = await _controller.EditUserRole("1");

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<EditUserRoleViewModel>(viewResult.Model);
        Assert.Equal("test@example.com", model.Email);
    }

    [Fact]
    public void CreateUser_Get_ReturnsViewResult()
    {
        _roleManagerMock.Setup(r => r.Roles).Returns(new List<IdentityRole> { new IdentityRole("Admin") }.AsQueryable());

        var result = _controller.CreateUser();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task DeleteUser_Get_ReturnsNotFound_WhenUserNotFound()
    {
        _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((IdentityUser)null);

        var result = await _controller.DeleteUser("nonexistent");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteUser_Get_ReturnsViewResult_WithUserViewModel()
    {
        var user = new IdentityUser { Id = "1", Email = "test@example.com" };

        _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
        _userManagerMock.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });

        var result = await _controller.DeleteUser("1");

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<UserWithRoleViewModel>(viewResult.Model);
        Assert.Equal("test@example.com", model.Email);
    }


    [Fact]
    public async Task EditUserRole_Post_ValidUser_RedirectsToListUsers()
    {
        var user = new IdentityUser { Id = "1", Email = "test@example.com" };

        _userManagerMock.Setup(u => u.FindByIdAsync("1")).ReturnsAsync(user);
        _userManagerMock.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });
        _userManagerMock.Setup(u => u.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.AddToRoleAsync(user, "Manager"))
            .ReturnsAsync(IdentityResult.Success);

        var model = new EditUserRoleViewModel
        {
            UserId = "1",
            SelectedRole = "Manager"
        };

        var result = await _controller.EditUserRole(model);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ListUsers", redirectResult.ActionName);
    }

    [Fact]
    public async Task CreateUser_Post_ValidModel_RedirectsToListUsers()
    {
        var model = new CreateUserViewModel
        {
            Email = "test@example.com",
            Password = "Password123!",
            Role = "Admin"
        };

        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<IdentityUser>(), model.Password))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<IdentityUser>(), model.Role))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _controller.CreateUser(model);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ListUsers", redirectResult.ActionName);
    }

    [Fact]
    public async Task CreateUser_Post_InvalidModel_ReturnsViewWithModel()
    {
        var model = new CreateUserViewModel
        {
            Email = "test@example.com",
            Password = "Password123!",
            Role = "Admin"
        };

        _controller.ModelState.AddModelError("Email", "Required");

        var result = await _controller.CreateUser(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        var returnedModel = Assert.IsAssignableFrom<CreateUserViewModel>(viewResult.Model);
        Assert.Equal(model.Email, returnedModel.Email);
    }

    [Fact]
    public async Task DeleteUserConfirmed_Post_ValidUser_RedirectsToListUsers()
    {
        var user = new IdentityUser { Id = "1", Email = "test@example.com" };

        _userManagerMock.Setup(u => u.FindByIdAsync("1")).ReturnsAsync(user);
        _userManagerMock.Setup(u => u.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

        var result = await _controller.DeleteUserConfirmed("1");

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ListUsers", redirectResult.ActionName);
    }
}
