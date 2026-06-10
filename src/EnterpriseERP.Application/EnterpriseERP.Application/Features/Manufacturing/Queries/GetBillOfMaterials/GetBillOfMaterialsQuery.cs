using System.Collections.Generic;
using EnterpriseERP.Application.Features.Manufacturing.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.Manufacturing.Queries.GetBillOfMaterials;

public record GetBillOfMaterialsQuery : IRequest<List<BillOfMaterialsDto>>;
