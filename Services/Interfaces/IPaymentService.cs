using PayGate.DTOs;
using PayGate.Models;

namespace PayGate.Services.Interfaces;

public interface IPaymentService
{
    Task<Payment> ProcessPaymentAsync(CreatePaymentDto dto, Guid clientAppId);
    Task<Payment?> GetPaymentByIdAsync(Guid id);
    Task<IEnumerable<Payment>> GetAllPaymentsAsync();
}