using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game.Identity.Service.Dtos;
using Game.Identity.Service.Entities;

namespace Game.Identity.Service
{
    public static class Extensions
    {
        public static UserDto AsDto(this ApplicationUser user)
        {
            return new UserDto(user.Id, user.UserName, user.Email, user.Gil, user.CreatedOn);
        }
    }
}