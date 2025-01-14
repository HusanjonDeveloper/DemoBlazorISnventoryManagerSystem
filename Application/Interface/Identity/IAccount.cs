﻿using Application.DTO.Request.Identity;
using Application.DTO.Response.Identity;

namespace Application.Interface.Identity;

public interface IAccount
{
    Task<ServiceResponse> LoginAsync(LoginUserRequestDTO model);
    Task<ServiceResponse> CreateUserAsync(CreateUserRequestDTO model);
        Task<IEnumerable<GetUserWithClaimResponseDTO>> GetUsersWithClaimsAsync();
        Task SetUpAsync();
        Task<ServiceResponse> UpdateUserAsync (ChangeUserClainRequestDTO model);
       // Task SaveActivityAsync(ActivityTrackerRequestDTO model);
        //Task<IEnumerable<ActivityTrackerRequestDTO>> GetActivitiesAsync();
}