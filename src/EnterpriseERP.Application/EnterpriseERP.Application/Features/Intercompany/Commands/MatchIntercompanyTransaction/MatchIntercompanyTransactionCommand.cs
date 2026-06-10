using MediatR;

namespace EnterpriseERP.Application.Features.Intercompany.Commands.MatchIntercompanyTransaction;

public record MatchIntercompanyTransactionCommand(Guid TransactionId) : IRequest<bool>;
