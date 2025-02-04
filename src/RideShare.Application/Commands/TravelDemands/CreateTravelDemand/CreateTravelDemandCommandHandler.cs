﻿using AutoMapper;
using MediatR;
using System.Threading.Tasks;
using System.Threading;
using RideShare.Application.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using RideShare.Application.Exception;
using RideShare.Domain.Entities;

namespace RideShare.Application.Commands.TravelDemands.CreateTravelDemand
{
    public class CreateTravelDemandCommandHandler : IRequestHandler<CreateTravelDemandRequest, CreateTravelDemandResponse>
    {
        private readonly IRideShareDbContext _context;

        public CreateTravelDemandCommandHandler(IRideShareDbContext context)
        {
            _context = context;
        }

        public async Task<CreateTravelDemandResponse> Handle(CreateTravelDemandRequest request, CancellationToken cancellationToken)
        {
            var passenger = await _context.Passengers.Where(x => x.User.Id == request.UserId)
                .FirstOrDefaultAsync();
            
            if (passenger is null)
            {
                throw new NullReferenceException(ExceptionMessage.EntityNotFound(typeof(Passenger).Name));
            }

            var plan = await _context.TravelPlans.Where(x => x.Id == request.TravelPlanId)
                .Include(x=>x.Demands)
                .ThenInclude(x=>x.Passenger)
                .FirstOrDefaultAsync();

            if (plan is null)
            {
                throw new NullReferenceException(ExceptionMessage.EntityNotFound(typeof(TravelPlan).Name));
            }

            var demand = passenger.CreateTravelDemand(plan);

            _context.TravelDemands.Add(demand);
            var result = await _context.SaveChangesAsync(cancellationToken);
            return new CreateTravelDemandResponse
            {
                Result = result
            };
        }
    }

    public class CreateTravelDemandRequest : IRequest<CreateTravelDemandResponse>
    {
        public Guid UserId { get; set; }
        public int TravelPlanId { get; set; }
    }

    public class CreateTravelDemandResponse
    {
        public int Result { get; set; }
    }
}