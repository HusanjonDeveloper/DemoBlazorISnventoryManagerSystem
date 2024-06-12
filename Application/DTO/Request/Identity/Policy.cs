﻿namespace Application.DTO.Request.Identity;

public class Policy
{
    public const string AdminPolicy = "AdminPolicy";
    public const string ManagerPolicy = "ManagerPolicy";
    public const string UserPolicy = "UserPolicy";
    
    public static class  RoleClaim
    {
        public const string Admin = "Admin";
        public const string Manager = "Manage";
        public const string User = "User";
    }
}