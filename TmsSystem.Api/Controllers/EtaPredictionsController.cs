using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/eta-prediction")]
public sealed class EtaPredictionsController(TmsDbContext dbContext) : CrudController<EtaPrediction>(dbContext);
