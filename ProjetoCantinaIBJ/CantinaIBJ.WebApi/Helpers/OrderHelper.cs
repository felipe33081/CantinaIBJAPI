using CantinaIBJ.Data.Contracts;
using CantinaIBJ.Data.Contracts.Customer;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Customer;
using CantinaIBJ.Model.Enumerations;
using CantinaIBJ.Model.Orders;

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

    public async Task UpdateCalculatePaymentsOrder(UserContext contextUser, Order order, CustomerPerson customerPerson = null)
    {
        //cria validações pro Status do pedido, caso o usuario/vendedor tenha atualizado o status para "Encerrado"(verificar o tipo de pagamento se foi avista, pix, ou ficou devendo, para
        //atualiza também o debitBalance ou creditBalando do cliente, caso o cliente tenha cadastro) e retornar o endpoint com status code 201
        //Se o Status for Cancelado, exclui

        try
        {
            var status = order.Status;
            var paymentOfType = order.PaymentOfType;
            if (status != OrderStatus.Created && status != OrderStatus.InProgress)
            {
                switch (status)
                {
                    case OrderStatus.Finished://finalizado
                        if (paymentOfType != null)
                        {
                            switch (paymentOfType)
                            {
                                case PaymentOfType.Money:
                                    //faz calculo do valor do pagamento com o valor do pedido pra saber se tem troco
                                    decimal changeValue = 0;
                                    if (order.PaymentValue != null)
                                    {
                                        changeValue = order.PaymentValue.Value - order.TotalValue;
                                    }

                                    order.ChangeValue = changeValue;
                                    order.UpdatedAt = DateTime.UtcNow;
                                    order.UpdatedBy = contextUser.GetCurrentUser();
                                    await _orderRepository.UpdateAsync(order);

                                    break;

                                case PaymentOfType.Debitor:
                                    if (customerPerson != null)
                                    {
                                        decimal totalValue = 0;
                                        if (order.PaymentValue != null)
                                        {
                                            totalValue = order.TotalValue - order.PaymentValue.Value;
                                            customerPerson.DebitBalance += totalValue;
                                        }
                                        else
                                            customerPerson.DebitBalance += order.TotalValue;

                                        await _customerPersonRepository.UpdateAsync(customerPerson);

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
                                        decimal totalValueDiffCredit = 0;
                                        if (order.PaymentValue != null)
                                        {
                                            totalValueDiffCredit = order.PaymentValue.Value - order.TotalValue;
                                            customerPerson.CreditBalance += totalValueDiffCredit;
                                        }
                                        else
                                            customerPerson.CreditBalance += order.TotalValue;

                                        await _customerPersonRepository.UpdateAsync(customerPerson);

                                        order.UpdatedAt = DateTime.UtcNow;
                                        order.UpdatedBy = contextUser.GetCurrentUser();
                                        await _orderRepository.UpdateAsync(order);

                                        break;
                                    }
                                    else
                                        throw new Exception("Não é possível vender e deixar um crédito na conta, sem o cliente estar cadastrado no sistema");
                            }

                            order.UpdatedAt = DateTime.UtcNow;
                            order.UpdatedBy = contextUser.GetCurrentUser();
                            await _orderRepository.UpdateAsync(order);

                            break;
                        }
                        else
                        {
                            throw new Exception("Obrigatório escolher o tipo do pagamento mediante o encerramento do pedido");
                        }
                    case OrderStatus.Canceled:

                        order.UpdatedAt = DateTime.UtcNow;
                        order.UpdatedBy = contextUser.GetCurrentUser();

                        await _orderRepository.UpdateAsync(order);

                        break;
                }
            }
            else
            {
                order.UpdatedAt = DateTime.UtcNow;
                order.UpdatedBy = contextUser.GetCurrentUser();

                await _orderRepository.UpdateAsync(order);
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}