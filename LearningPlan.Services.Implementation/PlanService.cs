﻿using LearningPlan.DataAccess;
using LearningPlan.DomainModel;
using LearningPlan.Services.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace LearningPlan.Services.Implementation
{
    public class PlanService : IPlanService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IReadRepository<Plan> _planReadRepository;
        private readonly IReadRepository<PlanArea> _planAreaReadRepository;
        private readonly IWriteRepository<AreaTopic> _areaTopicRepository;
        private readonly IWriteRepository<Plan> _planRepository;
        private readonly IWriteRepository<PlanArea> _planAreaRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PlanService(
            IHttpContextAccessor httpContextAccessor,
            IReadRepository<Plan> planReadRepository,
            IReadRepository<PlanArea> planAreaReadRepository,
            IWriteRepository<AreaTopic> areaTopicRepository,
            IWriteRepository<Plan> planRepository,
            IWriteRepository<PlanArea> planAreaRepository,
            IUnitOfWork unitOfWork)
        {
            _httpContextAccessor = httpContextAccessor;
            _planReadRepository = planReadRepository;
            _planAreaReadRepository = planAreaReadRepository;
            _areaTopicRepository = areaTopicRepository;
            _planRepository = planRepository;
            _planAreaRepository = planAreaRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PlanResponseModel> CreatePlanAsync(PlanServiceModel model)
        {
            using (_unitOfWork)
            {
                var user = (User)_httpContextAccessor.HttpContext.Items["User"];

                var plan = new Plan(model.Name, user.Id);

                await _planRepository.CreateAsync(plan);

                foreach (PlanAreaServiceModel planArea in model.PlanAreas)
                {
                    PlanArea area = new PlanArea
                    {
                        Name = planArea.Name,
                        Plan = plan,
                        PlanId = plan.Id
                    };

                    await _planAreaRepository.CreateAsync(area);
                    foreach (AreaTopicServiceModel areaTopic in planArea.AreaTopics)
                    {
                        await _areaTopicRepository.CreateAsync(new AreaTopic
                        {
                            Name = areaTopic.Name,
                            StartDate = DateTime.ParseExact(areaTopic.StartDate, "dd/mm/yyyy",
                                CultureInfo.CurrentCulture),
                            EndDate = DateTime.ParseExact(areaTopic.EndDate, "dd/mm/yyyy", CultureInfo.CurrentCulture),
                            Source = areaTopic.Source,
                            PlanArea = area,
                            PlanAreaId = area.Id
                        });
                    }
                }

                await _unitOfWork.CommitAsync();

                return new PlanResponseModel
                {
                    Name = plan.Name,
                    Id = plan.Id
                };
            }
        }

        public async Task<PlanServiceModel> GetByIdAsync(string id)
        {
            Plan plan = await _planReadRepository.GetByIdAsync(id);
            var planAreas = _planAreaReadRepository.GetAll().Where(x => x.PlanId == id).ToList();
           
            return await Task.FromResult(new PlanServiceModel
            {
                Name = plan.Name,
                PlanAreas = planAreas.Select(x => new PlanAreaServiceModel
                {
                    Name = x.Name,
                    PlanId = x.PlanId,
                    AreaTopics = x.AreaTopics.Select(areaTopic => new AreaTopicServiceModel
                    {
                        Name = areaTopic.Name,
                        StartDate = areaTopic.StartDate.ToString("d"),
                        EndDate = areaTopic.EndDate.ToString("d"),
                        Source = areaTopic.Source
                    }).ToArray()
                }).ToArray(),
            });
        }
    }
}
