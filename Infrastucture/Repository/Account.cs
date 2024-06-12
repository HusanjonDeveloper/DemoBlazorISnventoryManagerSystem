using System.Security.Claims;
using Application.DTO.Request.Identity;
using Application.DTO.Response.Identity;
using Application.Extension.Identity;
using Application.Interface.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public class Account
    (UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) 
{
    public async Task<ServiceResponse> CreateUserAsync(CreateUserRequestDTO model)
    {
        var user = await FindUserByEmail(model.Email);
        if (user != null)
            return new ServiceResponse(false,"User Alrady exist");

        var newUser = new ApplicationUser()
        {
            UserName = model.Email,
            PasswordHash = model.Password,
            Email = model.Email,
            Name = model.Name
        };
        var result = CheskResult(await userManager.CreateAsync(newUser, model.Password));
        if (!result.Flag)
            return result;
        else
        {
            return await CreateUserClaims(model);
        }
    }

    private async Task<ServiceResponse> CreateUserClaims(CreateUserRequestDTO model)
    {
        if (string.IsNullOrEmpty(model.Policy)) return new ServiceResponse(false, "No Policy specified");
        Claim[] userClaim = [];
        if (model.Policy.Equals(Policy.AdminPolicy, StringComparison.OrdinalIgnoreCase))
        {
            userClaim =
            [
                new Claim(ClaimTypes.Email, model.Email),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("Name", model.Name),
                new Claim("Create", "true"),
                new Claim("Update", "true"),
                new Claim("Delete", "true"),
                new Claim("Read", "true"),
                new Claim("ManagerUser", "true")
            ];
        }
        else if(model.Policy.Equals(Policy.ManagerPolicy, StringComparison.OrdinalIgnoreCase))
        {
            userClaim = 
            [
                new Claim(ClaimTypes.Email, model.Email),
                new Claim(ClaimTypes.Role, "User"),
                new Claim("Name", model.Name),
                new Claim("Create", "false"),
                new Claim("Update", "false"),
                new Claim("Delete", "false"),
                new Claim("Read", "false"),
                new Claim("ManagerUser", "false")
            ];

        }

        var result = CheskResult(await userManager.AddClaimAsync(await FindUserByEmail(model.Email)), userClaim);
        if (result.Flag)
            return new ServiceResponse(true, "UserCreated");
        else
            return result;
    }


    public async Task<ServiceResponse> LoginAsync(LoginUserRequestDTO model)
    {
        var user = await FindUserByEmail(model.Email);
        if (user is null) return new ServiceResponse(false, "User not Found");

        var verifyPassword = await signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!verifyPassword.Succeeded) return new ServiceResponse(false, "Incorrect Credentials provided");

        var resuly = await signInManager.PasswordSignInAsync(user, model.Password, false, false);
        if (resuly.Succeeded)
            return new ServiceResponse(false, "Unknown error occurd while loging you in");
        else
            return new ServiceResponse(true, null);
    }
    private async Task<ApplicationUser?> FindUserByEmail(string email)
        => await userManager.FindByEmailAsync(email);

    private async Task<ApplicationUser?> FindUserById(string id)
        => await userManager.FindByIdAsync(id);

         private static ServiceResponse CheskResult(IdentityResult result)
         {
             if (result.Succeeded) return new ServiceResponse(true, null);
             var errors = result.Errors.Select(o => o.Description);
             return new ServiceResponse(false, string.Join(Environment.NewLine, errors));
         }

         public async Task<IEnumerable<GetUserWithClaimResponseDTO>> GetUserWithClaimsAsync()
         {
             var userList = new List<GetUserWithClaimResponseDTO>();
             var allUsers = await userManager.Users.ToListAsync();
             if (allUsers.Count == 0) return userList;

             foreach (var user in allUsers)
             {
                 var currentUser = await userManager.FindByIdAsync(user!.Id);
                 var getCurrentUsersClaims = await userManager.GetClaimsAsync(currentUser!);

                 if (getCurrentUsersClaims.Any())
                 {
                     userList.Add(new GetUserWithClaimResponseDTO()
                     {
                         UserId = user.Id,
                         Email = getCurrentUsersClaims.FirstOrDefault( o => o.Type == ClaimTypes.Email)?.Value,
                         RoleName = getCurrentUsersClaims.FirstOrDefault(o => o.Type == ClaimTypes.Role)?.Value,
                         Name = getCurrentUsersClaims.FirstOrDefault(o => o.Type == "Name")?.Value,
                         ManagerUser = Convert.ToBoolean(getCurrentUsersClaims.FirstOrDefault(o => o.Type == "ManagerUser")?.Value),
                         Create = Convert.ToBoolean(getCurrentUsersClaims.FirstOrDefault(o =>o.Type == "Create")?.Value),
                         Update = Convert.ToBoolean(getCurrentUsersClaims.FirstOrDefault(o => o.Type == "Update")?.Value),
                         Delete = Convert.ToBoolean(getCurrentUsersClaims.FirstOrDefault(o =>o.Type == "Delete")?.Value),
                         Read = Convert.ToBoolean(getCurrentUsersClaims.FirstOrDefault(o =>o.Type == "Read")?.Value)
                     });
                 }
             }

             return userList;
         }

         public async Task SetUpAsync() => await CreateUserAsync(new CreateUserRequestDTO
         {
             Name = "Administrator",
             Email = "admin@admin.com",
             Password = "Admin@123",
             Policy = Policy.AdminPolicy
         });

         public async Task<ServiceResponse> UpdateUserAsync(ChangeUserClainRequestDTO model)
         {
             var user = await userManager.FindByIdAsync(model.UserId);
             if (user == null) return new ServiceResponse(false, "User Not Found");

             var oldUserClaims = await userManager.GetClaimsAsync(user);

             Claim[] newUserClaims =
             [
                  new Claim(ClaimTypes.Email, user.Email!),
                  new Claim(ClaimTypes.Role, model.RoleName),
                  new Claim("Name", model.Name),
                  new Claim("Create", model.Create.ToString()),
                  new Claim("Update", model.Update.ToString()),
                  new Claim("Read", model.Read.ToString()),
                  new Claim("ManagerUser", model.ManagerUser.ToString()),
                  new Claim("Delete", model.Delete.ToString())
             ];

             var result = await userManager.RemoveClaimsAsync(user, oldUserClaims);
             var respons = CheskResult(result);
             if (respons.Flag)
                 return new ServiceResponse(false, respons.Message);

             var addNewClaims = await userManager.AddClaimAsync(user, newUserClaims);
             var outcome = CheskResult(addNewClaims);
             if (outcome.Flag)
                 return new ServiceResponse(true," User Update");
             else
                 return outcome;
         }
         
}