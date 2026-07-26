using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using API.Authorization;

public class OwnResourceHandler : AuthorizationHandler<OwnResourceRequirement, Guid>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OwnResourceRequirement requirement,
        Guid resourceOwnerId)
    {
    
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var currentUserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId == resourceOwnerId.ToString())
        {
            context.Succeed(requirement);
        }  

        // If neither branch calls Succeed, the requirement fails automatically —
        // no need for an explicit Fail() call.
        return Task.CompletedTask;
    }
}