using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/billing-invoice")]
public sealed class BillingInvoicesController(TmsDbContext dbContext) : CrudController<BillingInvoice>(dbContext);
