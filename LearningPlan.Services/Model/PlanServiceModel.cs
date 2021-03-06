﻿using System.ComponentModel.DataAnnotations;

namespace LearningPlan.Services.Model
{
    public class PlanServiceModel
    {
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        public PlanAreaServiceModel[] PlanAreas { get; set; }
    }
}