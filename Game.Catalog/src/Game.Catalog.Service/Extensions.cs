using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game.Catalog.Service.Dtos;
using Game.Catalog.Service.Entities;

namespace Game.Catalog.Service
{
    public static class Extensions
    {
        
        public static ItemDto AsDto(this Item item)
        {
            return new ItemDto(item.Id, item.Name, item.Description, item.Price, item.CreatedDate);
        }
    }
}