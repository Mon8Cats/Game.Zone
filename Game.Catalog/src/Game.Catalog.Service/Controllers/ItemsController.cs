using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game.Catalog.Service.Dtos;
using Game.Catalog.Service.Entities;
using Game.Common;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Game.Catalog.Contracts;


namespace Game.Catalog.Service.Controllers
{
    [ApiController]
    [Route("items")]
    //[Authorize(Roles = AdminRole)]
    public class ItemsController : ControllerBase
    {
        /*
        private static readonly List<ItemDto> items = new()
        {
            new ItemDto(Guid.NewGuid(), "Potion", "Restores a small amount of HP", 5, DateTimeOffset.UtcNow),
            new ItemDto(Guid.NewGuid(), "Antidote", "Cures poison", 7, DateTimeOffset.UtcNow),
            new ItemDto(Guid.NewGuid(), "Bronze sword", "Deals a small amount of damage", 20, DateTimeOffset.UtcNow),
        };
        */

        private const string AdminRole = "Admin";
        private readonly IRepository<Item> itemsRepository;
        //private static int requestCounter = 0;
        private readonly IPublishEndpoint publishEndpoint;

        public ItemsController(IRepository<Item> itemsRepository, IPublishEndpoint publishEndpoint)
        {
            this.itemsRepository = itemsRepository;
            this.publishEndpoint = publishEndpoint;
        }

        // GET /items
        [HttpGet]
        [Authorize(Policies.Read)]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetAsync()
        {
            /*
            requestCounter++;
            Console.WriteLine($"Request {requestCounter} Starting...");

            if(requestCounter <= 2)
            {
                 Console.WriteLine($"Request {requestCounter} Delaying...");
                 await Task.Delay(TimeSpan.FromSeconds(10));

            }

             if(requestCounter <= 4)
            {
                 Console.WriteLine($"Request {requestCounter} 500 (Internal Server Error).");
                 return StatusCode(500);

            }
            */

            var items = (await itemsRepository.GetAllAsync())
                        .Select(item => item.AsDto());

            //Console.WriteLine($"Request {requestCounter} 200 (OK).");
            return Ok(items);
        }

        // GET /items/{id}
        [HttpGet("{id}")]
        [Authorize(Policies.Read)]
        public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id)
        {
            var item = await itemsRepository.GetAsync(id);

            if (item == null)
            {
                return NotFound();
            }
            return item.AsDto();
        }

        // POST /items
        [HttpPost]
        [Authorize(Policies.Write)]
        public async Task<ActionResult<ItemDto>> PostAsync(CreateItemDto createItemDto)
        {
            var item = new Item{                
                Name = createItemDto.Name, 
                Description = createItemDto.Description, 
                Price = createItemDto.Price, 
                CreatedDate = DateTimeOffset.UtcNow
            };
            
            await itemsRepository.CreateAsync(item);

            await publishEndpoint.Publish(new CatalogItemCreated(item.Id, item.Name, item.Description));

            return CreatedAtAction(nameof(GetByIdAsync), new {item.Id}, item);
        }

        // Put /items/{id}
        [HttpPut("{id}")]
        [Authorize(Policies.Write)]
        public async Task<IActionResult> PutAsync(Guid id, UpdateItemDto updateItemDto)
        {
            var existingItem =  await itemsRepository.GetAsync(id);

            if (existingItem == null)
            {
                return NotFound();
            }

            existingItem.Name = updateItemDto.Name;
            existingItem.Description = updateItemDto.Description;
            existingItem.Price = updateItemDto.Price;

            await itemsRepository.UpdateAsync(existingItem);

            await publishEndpoint.Publish(new CatalogItemUpdated(existingItem.Id, existingItem.Name, existingItem.Description));

            return NoContent();

        }

        // DELETE /items/{id}
        [HttpDelete("{id}")]
        [Authorize(Policies.Write)]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {

            var item =  await itemsRepository.GetAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            await itemsRepository.DeleteAsync(id);

            await publishEndpoint.Publish(new CatalogItemDeleted(item.Id));

            return NoContent();
        }
    }
}