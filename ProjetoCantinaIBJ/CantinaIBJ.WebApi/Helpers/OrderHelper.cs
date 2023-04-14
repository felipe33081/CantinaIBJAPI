using CantinaIBJ.Data.Contracts;
using CantinaIBJ.Data.Contracts.Customer;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Customer;
using CantinaIBJ.Model.Enumerations;
using CantinaIBJ.Model.Orders;
using CantinaIBJ.WebApi.Models;

namespace CantinaIBJ.WebApi.Helpers;

public class OrderHelper
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerPersonRepository _customerPersonRepository;
    public OrderHelper(IOrderRepository orderRepository,
        ICustomerPersonRepository customerPersonRepository)
    {
        _orderRepository = orderRepository;
        _customerPersonRepository = customerPersonRepository;
    }

    public async Task UpdateCalculatePaymentsOrder(UserContext contextUser, Order order, FinalizeOrderRequestModel requestModel, CustomerPerson? customerPerson = null)
    {
        try
        {
            var paymentValue = requestModel.PaymentValue;
            var paymentOfType = requestModel.PaymentOfType;
            switch (paymentOfType)
            {
                case PaymentOfType.Money:
                    //faz calculo do valor do pagamento com o valor do pedido pra saber se tem troco
                    order.ChangeValue = paymentValue - order.TotalValue;
                    order.Status = OrderStatus.Finished;
                    order.UpdatedAt = DateTime.UtcNow;
                    order.UpdatedBy = contextUser.GetCurrentUser();
                    await _orderRepository.UpdateAsync(order);

                    break;

                case PaymentOfType.Debitor:
                    if (customerPerson != null)
                    {
                        if (paymentValue <= order.TotalValue)
                        {
                            customerPerson.Balance += paymentValue - order.TotalValue;
                        }
                        
                        await _customerPersonRepository.UpdateAsync(customerPerson);

                        order.Status = OrderStatus.Finished;
                        order.UpdatedAt = DateTime.UtcNow;
                        order.UpdatedBy = contextUser.GetCurrentUser();
                        await _orderRepository.UpdateAsync(order);

                        break;
                    }
                    else
                        throw new Exception("Não é possível vender para pagar depois, sem o cliente estar cadastrado no sistema");

                case PaymentOfType.ExtraMoney:
                    if (customerPerson != null)
                    {
                        if (paymentValue >= order.TotalValue)
                        {
                            customerPerson.Balance += paymentValue - order.TotalValue;
                        }

                        await _customerPersonRepository.UpdateAsync(customerPerson);

                        order.Status = OrderStatus.Finished;
                        order.UpdatedAt = DateTime.UtcNow;
                        order.UpdatedBy = contextUser.GetCurrentUser();
                        await _orderRepository.UpdateAsync(order);

                        break;
                    }
                    else
                        throw new Exception("Não é possível vender e deixar um crédito na conta, sem o cliente estar cadastrado no sistema");
            }

            order.Status = OrderStatus.Finished;
            order.PaymentOfType = paymentOfType;
            order.PaymentValue = requestModel.PaymentValue;
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = contextUser.GetCurrentUser();
            await _orderRepository.UpdateAsync(order);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}