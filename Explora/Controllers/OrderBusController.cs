﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Explora.dto;
using Explora.data;
using Explora.Entity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Explora.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderBusController : Controller
    {
        private ExploraContext context;
        public OrderBusController(ExploraContext context)
        {
            this.context = context;
        }
        // GET: api/values
        [HttpPost]
        [Authorize(Roles = "User")]
        public IActionResult CreateOrder([FromBody] CreateOrderBusDto dataInput)
        {
            var bus = context.TBus.FirstOrDefault(p => p.IdBus == dataInput.IdBus);
            if (bus == null)
            {
                return NotFound("Không có chuyến xe này");
            }
            if (bus.EmptySlot < dataInput.Amount)
            {
                return BadRequest("Chuyến xe không còn đủ chỗ");
            }
            List<TBusTicket> tickets = new List<TBusTicket>();
            foreach (var ticket in dataInput.createBusTicketDtos)
            {
                tickets.Add(new TBusTicket
                {
                    GuessName = ticket.GuessName,
                    GuessEmail = ticket.GuessEmail,
                    PhoneNumber = ticket.PhoneNumber,
                    IdBus = dataInput.IdBus
                });
                bus.EmptySlot--;
            }

            var order = context.TOrderBus.Add(new TOrderBu
            {
                TBusTickets = tickets,
                Amount = dataInput.Amount,
                TotalPrice = bus.Price * dataInput.Amount,
                BuyTime = DateTime.Now,
                IdBus = dataInput.IdBus,
                UserId = Int32.Parse(User.FindFirst("Id")?.Value ?? "0")
            });
            context.SaveChanges();
            return Ok();
        }

        [HttpGet("Get-all")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAllCreateOrder()
        {
            var bus = context.TOrderBus.Include(b => b.TBusTickets).Include(b => b.User).Select(b => new OrderBusDto
            {
                OrderId = b.OrderId,
                Amount = b.Amount,
                TotalPrice = b.TotalPrice,
                BuyTime = b.BuyTime,
                IdBus = b.IdBus,
                UserId = b.UserId,
                TBusTickets = b.TBusTickets,
                User = b.User
            });
            return Ok(bus);
        }
        [HttpGet("Get-by-id-user")]
        [Authorize(Roles = "User")]
        public IActionResult GetByIdUser()
        {
            var UserId = Int32.Parse(User.FindFirst("Id")?.Value ?? "0");

            var bill = context.TOrderBus.Include(b => b.TBusTickets).Include(b => b.User).Where(b => b.UserId == UserId).ToList();
            if (bill == null)
            {
                return NotFound();
            }
            var billbus = bill.Select(b => new OrderBusDto
            {
                OrderId = b.OrderId,
                Amount = b.Amount,
                TotalPrice = b.TotalPrice,
                BuyTime = b.BuyTime,
                IdBus = b.IdBus,
                UserId = b.UserId,
                TBusTickets = b.TBusTickets,
                
            });
            return Ok(billbus);
        }
    }
}
