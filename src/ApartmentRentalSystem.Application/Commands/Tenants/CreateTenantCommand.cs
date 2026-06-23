using ApartmentRentalSystem.Core.Entities;
using ApartmentRentalSystem.Core.Enums;
using ApartmentRentalSystem.Core.Interfaces;
using ApartmentRentalSystem.Core.ValueObjects;
using MediatR;

namespace ApartmentRentalSystem.Application.Commands.Tenants;

public record CreateTenantCommand(
    long TelegramId,              // Обязательно (идентификация)
    string FullName,
    string Phone,
    string? Email = null,         // Опционально (для уведомлений)
    NotificationChannel PreferredChannel = NotificationChannel.Telegram
) : IRequest<Guid>;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Guid>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTenantCommandHandler(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Проверяем, не зарегистрирован ли уже этот Telegram ID
            var existingTenant = await _tenantRepository.GetByTelegramIdAsync(request.TelegramId);
            
            if (existingTenant != null)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw new InvalidOperationException($"Пользователь с Telegram ID {request.TelegramId} уже существует");
            }

            // Если указан email, проверяем его уникальность
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var existingByEmail = await _tenantRepository.GetByEmailAsync(request.Email);
                if (existingByEmail != null)
                {
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    throw new InvalidOperationException($"Email {request.Email} уже занят");
                }
            }

            // Создаем сущность Tenant
            var tenant = new Tenant(
                email: request.Email ?? string.Empty,  // Пустой email, если не указан
                contact: new ContactInfo(
                    fullName: request.FullName,
                    phone: request.Phone),
                telegramId: request.TelegramId,
                preferredChannel: request.PreferredChannel,
                isBlocked: false
            );

            await _tenantRepository.AddAsync(tenant);
            await _unitOfWork.CommitAsync(cancellationToken);

            return tenant.Id;
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}